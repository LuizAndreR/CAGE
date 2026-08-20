import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceitaResponse } from '../../../core/models/receita.interface';

@Component({
  selector: 'app-receita-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './receita-detail.html',
  styleUrl: './receita-detail.css'
})
export class ReceitaDetail {
  receita = input.required<ReceitaResponse>();

  // Eventos de navegação e ação
  voltar = output<void>();
  editar = output<number>();
  excluir = output<number>();

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatPercent(value: number): string {
    return `${value.toFixed(2)}%`;
  }
}