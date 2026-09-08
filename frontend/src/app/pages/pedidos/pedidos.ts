import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PedidoService } from '../../core/service/pedido.service'; // Ajuste o caminho
import { GetAllPedidosResponse } from '../../core/models/pedido.interface'; // Ajuste o caminho

import { PedidosList } from '../../shared/components/pedidos/pedidos-list/pedidos-list';

@Component({
  selector: 'app-pedidos-page',
  standalone: true,
  imports: [CommonModule, PedidosList],
  templateUrl: './pedidos.html',
  styleUrl: './pedidos.css'
})
export default class Pedidos implements OnInit {
  private pedidoService = inject(PedidoService);

  // Controle de estado da tela usando Angular 22 Signals
  view = signal<'list' | 'create' | 'detail'>('list');
  isLoading = signal<boolean>(true);
  
  // Armazena os dados vindos do C#
  pedidos = signal<GetAllPedidosResponse[]>([]);
  selectedId = signal<number | null>(null);

  // Encontra automaticamente o pedido selecionado na memória
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
    
    this.pedidoService.getPedidos().subscribe({
      next: (dados) => {
        this.pedidos.set(dados);
        this.isLoading.set(false);
      },
      error: (erro) => {
        console.error('Erro ao buscar pedidos:', erro);
        // Aqui no futuro podemos integrar um ToastService para avisar o usuário
        this.isLoading.set(false);
      }
    });
  }

  // Métodos engatilhados pelo componente filho (Dumb Component)
  onNovoPedido(): void {
    this.selectedId.set(null);
    this.view.set('create');
  }

  onSelecionarPedido(pedido: GetAllPedidosResponse): void {
    this.selectedId.set(pedido.id);
    this.view.set('detail');
  }
}