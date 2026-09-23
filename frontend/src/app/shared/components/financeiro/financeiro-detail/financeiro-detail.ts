import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransacaoResponse } from '../../../../core/models/financeiro.interface';

@Component({
  selector: 'app-financeiro-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './financeiro-detail.html',
  styleUrl: './financeiro-detail.css'
})
export class FinanceiroDetail {
  // Recebe os dados da transação clicada vindo do componente pai
  transacao = input.required<TransacaoResponse>();

  // Emite eventos de volta para o componente pai
  voltar = output<void>();
  
  // Emite o id e o motivo para o pai chamar a API de cancelamento
  cancelar = output<{ id: number, motivo: string }>();

  // Controle de estado local da Modal de Cancelamento
  showCancelModal = signal(false);
  cancelReason = signal('');

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatDate(dateStr: string | Date): string {
    const data = new Date(dateStr);
    return data.toLocaleDateString('pt-BR', { timeZone: 'UTC' });
  }

  abrirModalCancelamento(): void {
    this.showCancelModal.set(true);
  }

  fecharModalCancelamento(): void {
    this.showCancelModal.set(false);
    this.cancelReason.set('');
  }

  confirmarCancelamento(): void {
    const motivo = this.cancelReason().trim();
    if (!motivo) return;

    this.cancelar.emit({ 
      id: this.transacao().id, 
      motivo: motivo 
    });
    
    this.fecharModalCancelamento();
  }
}