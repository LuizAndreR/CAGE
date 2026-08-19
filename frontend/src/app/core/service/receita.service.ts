import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReceitaListResponse } from '../models/receita.interface';

@Injectable({
  providedIn: 'root'
})
export class ReceitaService {
  private http = inject(HttpClient);
  
  private apiUrl = `${environment.apiUrl}/receita`;

  getReceitas(): Observable<ReceitaListResponse[]> {
    return this.http.get<ReceitaListResponse[]>(this.apiUrl);
  }

  mudarStatus(id: number, novoStatus: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, { status: novoStatus });
  }
}