import { Component, input, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GetAllPedidosResponse } from '../../../../core/models/pedido.interface'; // Ajuste o caminho conforme seu projeto

@Component({
  selector: 'app-pedidos-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pedidos-list.html',
  styleUrl: './pedidos-list.css'
})
export class PedidosList {
  // Recebe a lista tipada exatamente como o DTO da API
  pedidos = input.required<GetAllPedidosResponse[]>();

  // Eventos emitidos para o Orquestrador
  novoPedido = output<void>();
  selecionarPedido = output<GetAllPedidosResponse>();

  // Estado reativo da busca
  termoBusca = signal('');

  // Filtro reativo otimizado com computed do Angular 22
  pedidosFiltrados = computed(() => {
    const busca = this.termoBusca().toLowerCase().trim();
    if (!busca) return this.pedidos();

    return this.pedidos().filter(pedido => 
      pedido.clienteNome.toLowerCase().includes(busca) || 
      pedido.id.toString().includes(busca)
    );
  });

  // Utilitários de Formatação
  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return 'A definir';
    
    // Converte a string ISO 8601 do C# para o formato local
    const date = new Date(dateStr);
    return date.toLocaleDateString('pt-BR', { 
      day: '2-digit', 
      month: '2-digit', 
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Configurações de Paginação
  paginaAtual = signal<number>(1);
  itensPorPagina = signal<number>(15);

  // Computed para fatiar a lista que já passou pelo filtro de busca
  pedidosPaginados = computed(() => {
    const inicio = (this.paginaAtual() - 1) * this.itensPorPagina();
    const fim = inicio + this.itensPorPagina();
    
    return this.pedidosFiltrados().slice(inicio, fim);
  });

  totalPaginas = computed(() => {
    return Math.ceil(this.pedidosFiltrados().length / this.itensPorPagina()) || 1;
  });

  // Método para navegar entre as páginas
  irParaPagina(novaPagina: number): void {
    if (novaPagina >= 1 && novaPagina <= this.totalPaginas()) {
      this.paginaAtual.set(novaPagina);
    }
  }
}