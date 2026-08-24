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
    precoVenda: [null, [Validators.required, Validators.min(0)]],
    percentualCustoExtra: [null, [Validators.min(0)]],
    percentualMargemLucro: [null, [Validators.required, Validators.min(0)]],
    ingredientes: this.fb.array([])
  });

  novoIngredienteForm: FormGroup = this.fb.group({
    itemId: ['', Validators.required],
    nome: [''], 
    quantidade: ['', [Validators.required, Validators.min(0.1)]],
    unidadeMedida: ['G', Validators.required]
  });
  
  // NOVO: Signal para controlar quais opções aparecem no select de Medida
  unidadesPermitidas = signal<{ valor: string, label: string }[]>([]);

  // Dicionário de Nomes Amigáveis
  private nomesUnidades: Record<string, string> = {
    'G': 'Gramas (G)',
    'KG': 'Quilogramas (KG)',
    'ML': 'Mililitros (ML)',
    'L': 'Litros (L)',
    'TSP': 'Colher de Chá (TSP)',
    'TBS': 'Colher de Sopa (TBS)',
    'CUP': 'Xícara (CUP)',
    'UN': 'Unidade (UN)' // Adicionada a Unidade que faltava!
  };

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

    this.configurarRegraPrecificacao();
  }

  // --- NOVA REGRA DE NEGÓCIO DA PRECIFICAÇÃO ---
  private configurarRegraPrecificacao(): void {
    const precoControl = this.receitaForm.get('precoVenda');
    const margemControl = this.receitaForm.get('percentualMargemLucro');

    if (!precoControl || !margemControl) return;

    // 1. Quando o usuário digitar no Preço de Venda
    precoControl.valueChanges.subscribe(valorPreco => {
      // Se o preço for maior que zero, obrigatoriamente a margem vira 0
      if (valorPreco && valorPreco > 0) {
        margemControl.setValue(0, { emitEvent: false });
      }
    });

    // 2. Quando o usuário digitar na Margem de Lucro
    margemControl.valueChanges.subscribe(valorMargem => {
      // Se a margem for maior que zero, obrigatoriamente o preço vira 0
      if (valorMargem && valorMargem > 0) {
        precoControl.setValue(0, { emitEvent: false });
      }
    });
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
        }, { emitEvent: false }); 

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
    
    const item = this.estoqueItens().find(i => i.id === idSelecionado);
    
    if (item) {
      // 1. Descobre a base do item no estoque (Massa, Volume ou Unidade)
      const umEstoque = item.unidadeMedida.toUpperCase();
      let siglasPermitidas: string[] = [];

      // 2. Aplica as suas regras de negócio de conversão
      if (['G', 'KG'].includes(umEstoque)) {
        siglasPermitidas = ['G', 'KG', 'TSP', 'TBS', 'CUP']; // Massa + Culinárias
      } else if (['ML', 'L'].includes(umEstoque)) {
        siglasPermitidas = ['ML', 'L', 'TSP', 'TBS', 'CUP']; // Volume + Culinárias
      } else if (umEstoque === 'UN') {
        siglasPermitidas = ['UN']; // Unidade estrita
      }

      // 3. Monta a lista de objetos para o HTML ler (Valor e Texto Amigável)
      const listaParaSelect = siglasPermitidas.map(sigla => ({
        valor: sigla,
        label: this.nomesUnidades[sigla] || sigla
      }));

      // Atualiza o Signal para refletir no HTML imediatamente
      this.unidadesPermitidas.set(listaParaSelect);

      // 4. Preenche o formulário com o nome e a unidade padrão do estoque
      this.novoIngredienteForm.patchValue({ 
        nome: item.nome,
        unidadeMedida: umEstoque 
      });
    } else {
      // Se desmarcar, limpa a lista
      this.unidadesPermitidas.set([]);
      this.novoIngredienteForm.get('unidadeMedida')?.reset();
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