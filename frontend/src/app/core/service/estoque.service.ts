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

  criarItem(item: any): Observable<any> {
    return this.http.post(this.apiUrl, item);
  }

  adicionarQuantidade(itemId: number, quantidade: number, valor: number): Observable<any> {
    const payload = {
      quantidadeAdicionar: quantidade,
      valor: valor
    };
    return this.http.patch(`${this.apiUrl}/${itemId}/entrada`, payload);
  }
  
  removerQuantidade(itemId: number, quantidade: number): Observable<any> {
    const payload = {
      quantidadeARemover: quantidade
    };
    return this.http.patch(`${this.apiUrl}/${itemId}/saida`, payload);
  }

  atualizarItem(id: number, payload: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, payload);
  }

  deletarItem(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}