import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs/operators'; 
import { PedidoService } from '../../core/services/pedido.service'; 
import { GetAllPedidosResponse } from '../../core/models/pedido.interface'; 

import { PedidosList } from '../../shared/components/pedidos/pedidos-list/pedidos-list';
import { PedidosForm } from '../../shared/components/pedidos/pedidos-form/pedidos-form';

@Component({
  selector: 'app-pedidos-page',
  standalone: true,
  imports: [CommonModule, PedidosList, PedidosForm],
  templateUrl: './pedidos.html',
  styleUrl: './pedidos.css'
})
export default class Pedidos implements OnInit {
  private pedidoService = inject(PedidoService);

  view = signal<'list' | 'create' | 'detail'>('list');
  isLoading = signal<boolean>(true);
  
  pedidos = signal<GetAllPedidosResponse[]>([]);
  selectedId = signal<number | null>(null);

  pedidoSelecionado = computed(() => {
    const id = this.selectedId();
    if (!id) return null;
    return this.pedidos().find(p => p.id === id) || null;
  });

  ngOnInit(): void {
    this.carregarPedidos();
  }

  carregarPedidos(): void {
    this.isLoading.set(true);
    
    this.pedidoService.getPedidos()
      .pipe(finalize(() => this.isLoading.set(false))) // DICA: O finalize desliga o loading em caso de sucesso OU erro
      .subscribe({
        next: (dados) => {
          this.pedidos.set(dados);
        },
        error: (erro) => {
          console.error('Erro ao buscar pedidos:', erro);
          // TODO: Integrar ToastService
        }
      });
  }

  onNovoPedido(): void {
    this.selectedId.set(null);
    this.view.set('create');
  }

  onSelecionarPedido(pedido: GetAllPedidosResponse): void {
    this.selectedId.set(pedido.id);
    this.view.set('detail');
  }

  // 3. Novo método para receber o command do componente filho e salvar na API
  onSalvarPedido(command: any): void {
    this.isLoading.set(true);
    
    // Assumindo que seu PedidoService tem o método createPedido mapeado para POST /api/pedido
    this.pedidoService.createPedido(command)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.view.set('list');
          this.carregarPedidos(); // Recarrega a tabela de pedidos para exibir o pedido novo
        },
        error: (erro) => {
          console.error('Erro ao criar pedido:', erro);
        }
      });
  }
}