import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment'; 
import { GetAllPedidosResponse } from '../models/pedido.interface'; 

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private http = inject(HttpClient);
  
  private apiUrl = `${environment.apiUrl}/pedido`; 

  getPedidos(): Observable<GetAllPedidosResponse[]> {
    return this.http.get<GetAllPedidosResponse[]>(this.apiUrl);
  }
}