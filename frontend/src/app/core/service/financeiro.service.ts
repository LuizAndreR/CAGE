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

  // CORREÇÃO AQUI: Atualizamos a tipagem de 'mes' para number e adicionamos 'ano?: number'
  getTransacoes(filtros?: { tipo?: string, categoria?: string, ano?: number, mes?: number }): Observable<TransacaoResponse[]> {
    let params = new HttpParams();
    
    // Adicionamos os parâmetros apenas se eles existirem
    if (filtros?.tipo) params = params.set('tipo', filtros.tipo);
    if (filtros?.categoria) params = params.set('categoria', filtros.categoria);
    
    // O HttpParams exige que os valores da URL sejam strings, então usamos toString()
    if (filtros?.ano) params = params.set('ano', filtros.ano.toString());
    if (filtros?.mes) params = params.set('mes', filtros.mes.toString());

    return this.http.get<TransacaoResponse[]>(this.apiUrl, { params });
  }

  getResumo(): Observable<TransacaoResumo> {
    return this.http.get<TransacaoResumo>(`${this.apiUrl}/resumo`);
  }
}