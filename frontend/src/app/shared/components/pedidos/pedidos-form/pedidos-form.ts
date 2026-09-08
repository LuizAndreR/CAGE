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

  // Tipagem forte com a interface correta atualizada
  pedidoParaEditar = input<PedidoResponse | null>(null); 
  cancelar = output<void>();
  salvar = output<any>(); 

  pedidoForm!: FormGroup;

  // Signal utilizando a interface ReceitaListResponse (Aplicando DRY - Clean Code)
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
      telefoneCliente: [''], // NOVO CAMPO ADCIONADO (Opcional)
      descricao: [''],       // NOVO CAMPO ADCIONADO (Opcional)
      dataEntrega: [''],
      itens: this.fb.array([])
    });

    const pedidoEdit = this.pedidoParaEditar();
    
    if (pedidoEdit) {
      // Ajuste para o input datetime-local (YYYY-MM-DDTHH:mm)
      const dataFormatada = pedidoEdit.dataEntrega ? pedidoEdit.dataEntrega.substring(0, 16) : '';

      // Atualizado para carregar o telefone e a descrição caso a usuária esteja editando
      this.pedidoForm.patchValue({
        clienteNome: pedidoEdit.clienteNome,
        telefoneCliente: pedidoEdit.telefoneCliente || '',
        descricao: pedidoEdit.descricao || '',
        dataEntrega: dataFormatada
      });
      
      // Lógica de preenchimento dos itens quando a usuária for editar um pedido
      if (pedidoEdit.itens && pedidoEdit.itens.length > 0) {
        pedidoEdit.itens.forEach(item => {
          this.adicionarItem(item.receitaId, item.quantidade);
        });
      } else {
        this.adicionarItem(); 
      }
    } else {
      // Se for um novo pedido, inicia com uma linha vazia
      this.adicionarItem(); 
    }
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
      
      // Monta o payload exatamente como o CreatePedidoCommand ou UpdatePedidoCommand espera
      const command = {
        clienteNome: payload.clienteNome,
        telefoneCliente: payload.telefoneCliente || null, // Garante envio nulo se estiver vazio
        descricao: payload.descricao || null,             // Garante envio nulo se estiver vazio
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