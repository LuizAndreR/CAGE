import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstoqueItem } from '../../../../core/models/estoque.interface';

@Component({
  selector: 'app-estoque-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './estoque-detail.html',
  styleUrl: './estoque-detail.css'
})
export class EstoqueDetail {
  item = input.required<EstoqueItem>();
  
  voltar = output<void>();
  editar = output<EstoqueItem>();
  deletar = output<void>();

  formatCurrency(value: number | undefined): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value || 0);
  }
}