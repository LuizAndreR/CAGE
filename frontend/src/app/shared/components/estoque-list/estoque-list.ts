import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstoqueItem } from '../../../core/models/estoque.interface';

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

  formatCurrency(value: number | undefined): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value || 0);
  }
}