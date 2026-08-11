import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
    // Truque temporário: Cole aqui o token JWT gerado pelo seu backend
    // Quando a tela de Login for criada, substituiremos isso pela leitura do LocalStorage
    const tokenTemporario = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdGUgMDIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ0ZXN0ZTAyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkRvbm8iLCJFbXByZXNhSWQiOiIxIiwiZXhwIjoxNzg2NDg3OTI4LCJpc3MiOiJDYWtlR2VzdGFvIiwiYXVkIjoiQ2FrZUdlc3Rhb1VzZXJzIn0.IUvEO8TJYJRseojZYsn9Zq4s73taXThLgkLha3-b1Y0'; 
    
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${tokenTemporario}`
    });

    return this.http.get<DashboardResumo>(this.apiUrl, { headers });
  }
}