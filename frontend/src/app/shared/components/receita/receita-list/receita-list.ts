import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceitaListResponse } from '../../../../core/models/receita.interface';

@Component({
  selector: 'app-receita-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './receita-list.html',
  styleUrl: './receita-list.css'
})
export class ReceitaList {
  // Angular 22 Signal Inputs
  receitas = input.required<ReceitaListResponse[]>();
  activeTab = input<'todas' | 'ativas' | 'desativadas'>('todas');

  // Angular 22 Signal Outputs
  mudarAba = output<'todas' | 'ativas' | 'desativadas'>();
  novaReceita = output<void>();
  selecionarReceita = output<number>();
  alternarStatus = output<{ id: number, statusAtual: boolean }>();

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }
}