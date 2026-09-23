import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AlterarSenhaRequest, AtualizarPerfilRequest, UsuarioResponse } from '../models/usuario.interface';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/user`;
  private perfilAtualizado = new Subject<AtualizarPerfilRequest>();

  readonly perfilAtualizado$ = this.perfilAtualizado.asObservable();

  getPerfil(): Observable<UsuarioResponse> {
    return this.http.get<UsuarioResponse>(`${this.apiUrl}/me`);
  }

  getFuncionarios(): Observable<UsuarioResponse[]> {
    return this.http.get<UsuarioResponse[]>(`${this.apiUrl}/funcionarios`);
  }

  atualizarPerfil(payload: AtualizarPerfilRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/me`, payload).pipe(
      tap(() => this.perfilAtualizado.next(payload))
    );
  }

  alterarSenha(payload: AlterarSenhaRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/me/senha`, payload);
  }
}
