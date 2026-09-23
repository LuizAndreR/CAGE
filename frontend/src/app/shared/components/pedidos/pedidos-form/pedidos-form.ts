import { Component, input, output, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';

// Importações com os nomes atualizados e reaproveitando os modelos centrais
import { ReceitaService } from '../../../../core/services/receita.service'; 
import { PedidoResponse } from '../../../../core/models/pedido.interface';
import { ReceitaListResponse } from '../../../../core/models/receita.interface';

@Component({
  selector: 'app-pedidos-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './pedidos-form.html',
  styleUrl: './pedidos-form.css'
})
export class PedidosForm implements OnInit {
  private fb = inject(FormBuilder);
  private receitaService = inject(ReceitaService); 

  pedidoParaEditar = input<PedidoResponse | null>(null); 
  cancelar = output<void>();
  salvar = output<any>(); 

  pedidoForm!: FormGroup;

  receitasDisponiveis = signal<ReceitaListResponse[]>([]);

  ngOnInit() {
    this.iniciarFormulario();
    this.carregarReceitasDoBanco(); 
  }

  private carregarReceitasDoBanco() {
    this.receitaService.getReceitas().subscribe({
      next: (receitasReais) => {
        this.receitasDisponiveis.set(receitasReais);
      },
      error: (erro) => {
        console.error('Falha ao carregar o menu de receitas:', erro);
      }
    });
  }

  private iniciarFormulario() {
    this.pedidoForm = this.fb.group({
        clienteNome: ['', Validators.required],
        telefoneCliente: ['', [
          Validators.pattern('^[0-9]*$'), 
          Validators.minLength(10),      
          Validators.maxLength(11)        
        ]],
      descricao: [''],      
      dataEntrega: [''],
      itens: this.fb.array([])
    });

    const pedidoEdit = this.pedidoParaEditar();
    
    if (pedidoEdit) {
      const dataFormatada = pedidoEdit.dataEntrega ? pedidoEdit.dataEntrega.substring(0, 16) : '';

      this.pedidoForm.patchValue({
        clienteNome: pedidoEdit.clienteNome,
        telefoneCliente: pedidoEdit.telefoneCliente || '',
        descricao: pedidoEdit.descricao || '',
        dataEntrega: dataFormatada
      });
      
      if (pedidoEdit.itens && pedidoEdit.itens.length > 0) {
        pedidoEdit.itens.forEach(item => {
          this.adicionarItem(item.receitaId, item.quantidade);
        });
      } else {
        this.adicionarItem(); 
      }
    } else {
      this.adicionarItem(); 
    }
  } 

  onTelefoneInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    
    // Remove tudo que não for número (0 a 9)
    const apenasNumeros = input.value.replace(/[^0-9]/g, '');
    
    // Atualiza o controle do formulário silenciosamente
    this.pedidoForm.get('telefoneCliente')?.setValue(apenasNumeros, { emitEvent: false });
    
    // Atualiza o valor visual na tela
    input.value = apenasNumeros;
  }

  get itens(): FormArray {
    return this.pedidoForm.get('itens') as FormArray;
  }

  adicionarItem(receitaId: number | string = '', quantidade: number = 1) {
    const itemForm = this.fb.group({
      receitaId: [receitaId, Validators.required],
      quantidade: [quantidade, [Validators.required, Validators.min(1)]]
    });
    this.itens.push(itemForm);
  }

  removerItem(index: number) {
    if (this.itens.length > 1) {
      this.itens.removeAt(index);
    }
  }

  calcularTotalEstimado(): string {
    let total = 0;
    
    this.itens.controls.forEach(control => {
      const receitaId = Number(control.get('receitaId')?.value);
      const qtd = Number(control.get('quantidade')?.value) || 0;
      
      const receita = this.receitasDisponiveis().find(r => r.id === receitaId);
      if (receita) {
        total += receita.precoVenda * qtd;
      }
    });

    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(total);
  }

  onSubmit() {
    if (this.pedidoForm.valid) {
      const payload = this.pedidoForm.value;
      
      const command = {
        clienteNome: payload.clienteNome,
        telefoneCliente: payload.telefoneCliente || null, 
        descricao: payload.descricao || null,             
        dataEntrega: payload.dataEntrega ? new Date(payload.dataEntrega).toISOString() : null,
        itens: payload.itens.map((i: any) => ({
          receitaId: Number(i.receitaId),
          quantidade: Number(i.quantidade)
        }))
      };

      this.salvar.emit(command);
    } else {
      this.pedidoForm.markAllAsTouched(); 
    }
  }
}