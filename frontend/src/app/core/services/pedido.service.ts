import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment'; 
// DICA DE SÊNIOR: Não esqueça de importar a interface detalhada!
import { AllPedidosResponse, PedidoResponse } from '../models/pedido.interface'; 

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private http = inject(HttpClient);
  
  private apiUrl = `${environment.apiUrl}/pedido`; 

  getPedidos(): Observable<AllPedidosResponse[]> {
    return this.http.get<AllPedidosResponse[]>(this.apiUrl);
  }
  
  getPedidoById(id: number): Observable<PedidoResponse> {
    return this.http.get<PedidoResponse>(`${this.apiUrl}/${id}`);
  }

  createPedido(command: any): Observable<any> {
    return this.http.post(this.apiUrl, command);
  }

  updatePedido(id: number, command: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, command);
  }

  deletePedido(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  updatePagamento(id: number, pagamento: boolean): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/pagamento`, { pagamento });
  }

  updateStatus(id: number, status: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/status`, { status });
  }
}