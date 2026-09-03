import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TransacaoResponse, TransacaoResumo } from '../models/financeiro.interface';

@Injectable({
  providedIn: 'root'
})
export class FinanceiroService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/financeiro`;

  getTransacoes(filtros?: { tipo?: string, categoria?: string, ano?: number, mes?: number }): Observable<TransacaoResponse[]> {
    let params = new HttpParams();
    
    if (filtros?.tipo) params = params.set('tipo', filtros.tipo);
    if (filtros?.categoria) params = params.set('categoria', filtros.categoria);
    
    if (filtros?.ano) params = params.set('ano', filtros.ano.toString());
    if (filtros?.mes) params = params.set('mes', filtros.mes.toString());

    return this.http.get<TransacaoResponse[]>(this.apiUrl, { params });
  }

  getResumo(): Observable<TransacaoResumo> {
    return this.http.get<TransacaoResumo>(`${this.apiUrl}/resumo`);
  }

  // NOVA IMPLEMENTAÇÃO: Rota de cancelamento
  cancelarTransacao(id: number, motivo: string): Observable<void> {
    // Enviamos o motivo no corpo da requisição para o Model Binding do .NET ler o CancelTransacaoCommand
    return this.http.post<void>(`${this.apiUrl}/${id}/cancelamento`, { motivo });
  }
}