import { Component, inject, OnInit, signal, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';

import { EstoqueService } from '../../../core/service/estoque.service';
import { EstoqueItem } from '../../../core/models/estoque.interface';
import { ReceitaService } from '../../../core/service/receita.service';

@Component({
  selector: 'app-receita-form', 
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], 
  templateUrl: './receita-form.html', 
  styleUrl: './receita-form.css'
})
export class ReceitaForm implements OnInit {
  private fb = inject(FormBuilder);
  private receitaService = inject(ReceitaService);
  private estoqueService = inject(EstoqueService);

  receitaId = input<number | null>(null);
  voltar = output<void>();
  salvo = output<void>();

  isEditing = signal<boolean>(false);
  estoqueItens = signal<EstoqueItem[]>([]);

  receitaForm: FormGroup = this.fb.group({
    nome: ['', [Validators.required]],
    modoPreparo: ['', [Validators.required]],
    precoVenda: [0, [Validators.required, Validators.min(0)]],
    percentualCustoExtra: [0, [Validators.min(0)]],
    percentualMargemLucro: [0, [Validators.required, Validators.min(0)]],
    ingredientes: this.fb.array([])
  });

  novoIngredienteForm: FormGroup = this.fb.group({
    itemId: ['', Validators.required],
    nome: [''], 
    quantidade: ['', [Validators.required, Validators.min(0.1)]],
    unidadeMedida: ['G', Validators.required]
  });

  get ingredientesArray(): FormArray {
    return this.receitaForm.get('ingredientes') as FormArray;
  }

  ngOnInit(): void {
    this.carregarEstoque();

    const id = this.receitaId();
    if (id !== null) {
      this.isEditing.set(true);
      this.carregarDadosEdicao(id);
    }
  }

  carregarDadosEdicao(id: number): void {
    this.receitaService.getReceitaById(id).subscribe({
      next: (receita) => {
        this.receitaForm.patchValue({
          nome: receita.nome,
          modoPreparo: receita.modoPreparo,
          precoVenda: receita.precoVenda,
          percentualCustoExtra: receita.percentualCustoExtra || 0, 
          percentualMargemLucro: receita.percentualMargemLucro || 0
        });

        this.ingredientesArray.clear();
        
        if (receita.ingredientes && receita.ingredientes.length > 0) {
          receita.ingredientes.forEach(ing => {
            this.ingredientesArray.push(this.fb.group({
              itemId: [ing.itemId, Validators.required],
              nome: [ing.nome], 
              quantidade: [ing.quantidade, [Validators.required, Validators.min(0.1)]],
              unidadeMedida: [ing.unidadeMedida, Validators.required]
            }));
          });
        }
      },
      error: (erro) => {
        console.error('Falha ao buscar a receita na API:', erro);
      }
    });
  }

  private carregarEstoque(): void {
    this.estoqueService.getItens().subscribe({
      next: (itens) => this.estoqueItens.set(itens),
      error: (erro) => console.error('Erro ao carregar o estoque:', erro)
    });
  }

  onItemSelect(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const idSelecionado = Number(select.value);
    
    const itemEncontrado = this.estoqueItens().find(item => item.id === idSelecionado);
    
    if (itemEncontrado) {
      this.novoIngredienteForm.patchValue({ 
        nome: itemEncontrado.nome,
        unidadeMedida: itemEncontrado.unidadeMedida
      });
    }
  }

  // --- MÉTODOS RESTAURADOS ---
  adicionarIngrediente(): void {
    if (this.novoIngredienteForm.invalid) return;

    const val = this.novoIngredienteForm.value;
    
    this.ingredientesArray.push(this.fb.group({
      itemId: [Number(val.itemId), Validators.required],
      nome: [val.nome], 
      quantidade: [Number(val.quantidade), Validators.required],
      unidadeMedida: [val.unidadeMedida, Validators.required]
    }));

    this.novoIngredienteForm.reset({ unidadeMedida: 'G' });
  }

  removerIngrediente(index: number): void {
    this.ingredientesArray.removeAt(index);
  }
  // -----------------------------

  onVoltar(): void {
    this.voltar.emit();
  }

  salvar(): void {
    if (this.receitaForm.invalid) {
      this.receitaForm.markAllAsTouched();
      return;
    }

    const payload = this.receitaForm.value;
    
    if (this.isEditing()) {
      payload.id = this.receitaId();
      
      // AGORA SIM: Chamando o serviço para disparar o PUT
      this.receitaService.atualizarReceita(payload.id, payload).subscribe({
        next: () => {
          console.log('Atualização enviada para a API com sucesso!');
          this.salvo.emit(); // Só fecha a tela se a API retornar sucesso
        },
        error: (erro) => {
          console.error('Falha ao atualizar na API:', erro);
        }
      });
      
    } else {
      
      // AGORA SIM: Chamando o serviço para disparar o POST
      this.receitaService.criarReceita(payload).subscribe({
        next: () => {
          console.log('Criação enviada para a API com sucesso!');
          this.salvo.emit(); // Só fecha a tela se a API retornar sucesso
        },
        error: (erro) => {
          console.error('Falha ao criar na API:', erro);
        }
      });
      
    }
  }
}