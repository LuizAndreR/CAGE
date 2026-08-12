import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReceitaResumo {
  receitaId: number;
  nome: string;
  precoVenda: number;
}

export interface PedidoResumo {
  pedidoId: number;
  clienteNome: string;
  valorTotal: number;
  dataCriacao: string;
  status: string;
}

export interface FinanceiroResumo {
  totalEntradas: number;
  totalSaidas: number;
  saldoAtual: number;
}

export interface DashboardResumo {
  nomeUsuario: string;
  totalItensEstoque: number;
  financeiro: FinanceiroResumo;
  ultimasReceitas: ReceitaResumo[];
  ultimosPedidos: PedidoResumo[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  
  private apiUrl = `${environment.apiUrl}/dashboard/resumo`;

  getResumo(): Observable<DashboardResumo> {
    return this.http.get<DashboardResumo>(this.apiUrl);
  }
}