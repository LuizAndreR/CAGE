import { Component, inject, OnInit, signal } from '@angular/core'; // Removi o 'computed' daqui
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs/operators'; 
import { PedidoService } from '../../core/services/pedido.service'; 
// DICA DE SÊNIOR: Importando a interface PedidoResponse
import { AllPedidosResponse, PedidoResponse, StatusPedido } from '../../core/models/pedido.interface'; 

import { PedidosList } from '../../shared/components/pedidos/pedidos-list/pedidos-list';
import { PedidosForm } from '../../shared/components/pedidos/pedidos-form/pedidos-form';
import { PedidoDetail } from '../../shared/components/pedidos/pedido-detail/pedido-detail';

@Component({
  selector: 'app-pedidos-page',
  standalone: true,
  imports: [CommonModule, PedidosList, PedidosForm, PedidoDetail],
  templateUrl: './pedidos.html',
  styleUrl: './pedidos.css'
})
export default class Pedidos implements OnInit {
  private pedidoService = inject(PedidoService);

  view = signal<'list' | 'create' | 'detail'>('list');
  isLoading = signal<boolean>(true);
  
  pedidos = signal<AllPedidosResponse[]>([]);
  selectedId = signal<number | null>(null);

  pedidoSelecionado = signal<PedidoResponse | null>(null);

  ngOnInit(): void {
    this.carregarPedidos();
  }

  carregarPedidos(): void {
    this.isLoading.set(true);
    
    this.pedidoService.getPedidos()
      .pipe(finalize(() => this.isLoading.set(false))) 
      .subscribe({
        next: (dados) => this.pedidos.set(dados),
        error: (erro) => console.error('Erro ao buscar pedidos:', erro)
      });
  }

  onNovoPedido(): void {
    this.selectedId.set(null);
    this.pedidoSelecionado.set(null); // Limpa o estado para o formulário nascer vazio
    this.view.set('create');
  }

  onSelecionarPedido(pedidoLista: AllPedidosResponse): void {
    this.isLoading.set(true);
    
    this.pedidoService.getPedidoById(pedidoLista.id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (dadosCompletos) => {
          this.pedidoSelecionado.set(dadosCompletos);
          this.selectedId.set(dadosCompletos.id);
          this.view.set('detail'); // Só muda a tela quando os dados chegarem da API
        },
        error: (erro) => {
          console.error('Erro ao buscar detalhes do pedido:', erro);
        }
      });
  }

  onSalvarPedido(command: any): void {
    this.isLoading.set(true);
    
    this.pedidoService.createPedido(command)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.view.set('list');
          this.carregarPedidos(); 
        },
        error: (erro) => console.error('Erro ao criar pedido:', erro)
      });
  }

  onAlterarStatus(evento: { id: number, status: StatusPedido }): void {
    this.isLoading.set(true);
    this.pedidoService.updateStatus(evento.id, evento.status)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.carregarPedidos(); 
          
          this.onSelecionarPedido({ id: evento.id } as any); 
        },
        error: (erro) => console.error('Erro ao atualizar status:', erro)
      });
  }

  onAlterarPagamento(evento: { id: number, pago: boolean }): void {
    this.isLoading.set(true);
    this.pedidoService.updatePagamento(evento.id, evento.pago)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.carregarPedidos();
          
          this.onSelecionarPedido({ id: evento.id } as any);
        },
        error: (erro) => console.error('Erro ao atualizar pagamento:', erro)
      });
  }

  onExcluirPedido(id: number): void {
    this.isLoading.set(true);
    this.pedidoService.deletePedido(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.view.set('list'); 
          this.selectedId.set(null);
          this.pedidoSelecionado.set(null);
          this.carregarPedidos();
        },
        error: (erro) => console.error('Erro ao excluir pedido:', erro)
      });
  }
}