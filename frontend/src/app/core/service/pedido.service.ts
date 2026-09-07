import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetAllPedidosResponse } from '../models/pedido.interface'; // Ajuste o caminho

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private http = inject(HttpClient);
  
  // A rota da API do CAGE
  private apiUrl = '/api/pedido'; 

  /**
   * Busca a listagem completa de pedidos (Read do CRUD)
   */
  getPedidos(): Observable<GetAllPedidosResponse[]> {
    return this.http.get<GetAllPedidosResponse[]>(this.apiUrl);
  }
}