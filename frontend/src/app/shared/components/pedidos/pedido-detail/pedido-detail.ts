import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PedidoResponse, StatusPedido } from '../../../../core/models/pedido.interface';

@Component({
  selector: 'app-pedido-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pedido-detail.html',
  styleUrl: './pedido-detail.css'
})
export class PedidoDetail {
  pedido = input.required<PedidoResponse>();

  voltar = output<void>();
  editar = output<void>(); 
  
  alterarStatus = output<{ id: number, status: StatusPedido }>();
  alterarPagamento = output<{ id: number, pago: boolean }>();
  excluir = output<number>();

  onStatusChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.alterarStatus.emit({ 
      id: this.pedido().id, 
      status: select.value as StatusPedido 
    });
  }

  onPagamentoChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.alterarPagamento.emit({ 
      id: this.pedido().id, 
      pago: select.value === 'true' 
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatDate(dateStr: string | undefined): string {
    if (!dateStr) return 'Não definida';
    return new Date(dateStr).toLocaleString('pt-BR', { 
      day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' 
    });
  }
}