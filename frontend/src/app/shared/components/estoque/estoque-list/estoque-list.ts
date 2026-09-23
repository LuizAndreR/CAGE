import { Component, input, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstoqueItem } from '../../../../core/models/estoque.interface';

@Component({
  selector: 'app-estoque-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './estoque-list.html',
  styleUrl: './estoque-list.css'
})
export class EstoqueList {
  items = input.required<EstoqueItem[]>();
  activeTab = input<'todos' | 'baixo'>('todos');

  mudarAba = output<'todos' | 'baixo'>();
  novoItem = output<void>();
  selecionarItem = output<number>();
  acaoRapida = output<{ type: 'add' | 'remove', id: number }>();

  // Configurações de Paginação (Padrão 15 itens por página igual ao financeiro)
  paginaAtual = signal<number>(1);
  itensPorPagina = signal<number>(15);

  itemsPaginados = computed(() => {
    const inicio = (this.paginaAtual() - 1) * this.itensPorPagina();
    const fim = inicio + this.itensPorPagina();
    return this.items().slice(inicio, fim);
  });

  totalPaginas = computed(() => {
    return Math.ceil(this.items().length / this.itensPorPagina()) || 1;
  });

  irParaPagina(novaPagina: number): void {
    if (novaPagina >= 1 && novaPagina <= this.totalPaginas()) {
      this.paginaAtual.set(novaPagina);
    }
  }

  formatCurrency(value: number | undefined): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value || 0);
  }
}