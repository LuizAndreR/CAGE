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

  getTransacoes(filtros?: { tipo?: string, categoria?: string, mes?: string }): Observable<TransacaoResponse[]> {
    let params = new HttpParams();
    
    // Tratamento de Clean Code: Só anexa na URL se existir E não for vazio
    if (filtros?.tipo && filtros.tipo !== '') {
      params = params.set('tipo', filtros.tipo);
    }
    
    if (filtros?.categoria && filtros.categoria !== '') {
      params = params.set('categoria', filtros.categoria);
    }
    
    // Quebra a string "2026-05" de forma segura para mandar ano=2026 e mes=5 pro C#
    if (filtros?.mes && filtros.mes !== '') {
      const mesParts = filtros.mes.split('-');
      if (mesParts.length === 2) {
        params = params.set('ano', mesParts[0]);
        params = params.set('mes', mesParts[1]);
      }
    }

    return this.http.get<TransacaoResponse[]>(this.apiUrl, { params });
  }

  getResumo(): Observable<TransacaoResumo> {
    return this.http.get<TransacaoResumo>(`${this.apiUrl}/resumo`);
  }
}