import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransacaoResponse, TransacaoResumo } from '../../../../core/models/financeiro.interface';

@Component({
  selector: 'app-financeiro-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './financeiro-list.html',
  styleUrl: './financeiro-list.css'
})

export class FinanceiroList {
  transacoes = input.required<TransacaoResponse[]>();
  resumo = input.required<TransacaoResumo>();

  novaTransacao = output<void>();
  selecionarTransacao = output<number>();
  
  // O filtro de mês agora envia a string no formato "YYYY-MM" (ex: "2026-05")
  filtrar = output<{ tipo: string, categoria: string, mes: string }>();

  filtroTipo = '';
  filtroCategoria = '';
  filtroMes = '';

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatDate(dateStr: string): string {
    const data = new Date(dateStr);
    return data.toLocaleDateString('pt-BR', { timeZone: 'UTC' });
  }

  get saldoTotal(): number {
    return this.resumo().entrada - this.resumo().saida;
  }

  onFilterChange(): void {
    // Quando o usuário seleciona "Todos os tipos", filtroTipo fica "" (vazio),
    // fazendo com que o parâmetro vá limpo para a API.
    this.filtrar.emit({
      tipo: this.filtroTipo,
      categoria: this.filtroCategoria,
      mes: this.filtroMes
    });
  }
}