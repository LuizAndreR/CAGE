import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EstoqueItem } from '../../core/models/estoque.interface';

@Injectable({
  providedIn: 'root'
})
export class EstoqueService {
  private http = inject(HttpClient);
  
  private apiUrl = `${environment.apiUrl}/estoque`; 

  getItens(): Observable<EstoqueItem[]> {
    return this.http.get<EstoqueItem[]>(this.apiUrl);
  }
  
  getItensBaixoEstoque(): Observable<EstoqueItem[]> {
    return this.http.get<EstoqueItem[]>(`${this.apiUrl}/alert`);
  }
}