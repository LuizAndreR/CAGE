import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AtualizarEmpresaRequest } from '../models/empresa.interface';

@Injectable({ providedIn: 'root' })
export class EmpresaService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/empresa`;

  atualizarEmpresa(payload: AtualizarEmpresaRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/me`, payload);
  }
}
