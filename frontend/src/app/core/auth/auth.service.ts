import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, throwError, interval, Subscription } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import { TokenService, AuthTokens } from './token.service';

export interface LoginCommand {
  email: string;
  senha: string;
}

export interface RefreshTokenCommand {
  RefreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private tokenService = inject(TokenService); // Injeção limpa do TokenService
  
  private apiUrl = `${environment.apiUrl}/auth`;
  private refreshSubscription?: Subscription;

  constructor() {
    if (this.tokenService.hasTokens()) {
      this.iniciarTimerDeRefresh();
    }
  }

  isLoggedIn(): boolean {
    return this.tokenService.hasTokens();
  }

  login(credentials: LoginCommand): Observable<AuthTokens> {
    return this.http.post<AuthTokens>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        this.tokenService.salvarTokens(response);
        this.iniciarTimerDeRefresh();
      })
    );
  }

  refreshToken(): Observable<AuthTokens> {
    const refreshCmd: RefreshTokenCommand = {
      RefreshToken: this.tokenService.getRefreshToken() || ''
    };

    return this.http.post<AuthTokens>(`${this.apiUrl}/refresh`, refreshCmd).pipe(
      tap(response => {
        this.tokenService.salvarTokens(response);
      }),
      catchError((error: HttpErrorResponse) => {
        if (error.status === 400 || error.status === 401) {
          this.logout();
        }
        return throwError(() => error);
      })
    );
  }

  private iniciarTimerDeRefresh(): void {
    this.pararTimerDeRefresh();
    const QUINZE_MINUTOS = 15 * 60 * 1000; 
    
    this.refreshSubscription = interval(QUINZE_MINUTOS).subscribe(() => {
      if (this.tokenService.hasTokens()) {
        this.refreshToken().subscribe();
      }
    });
  }

  private pararTimerDeRefresh(): void {
    if (this.refreshSubscription) {
      this.refreshSubscription.unsubscribe();
    }
  }

  logout(): void {
    this.pararTimerDeRefresh();
    this.tokenService.limparTokens(); // Delega a limpeza para o TokenService
    this.router.navigate(['/login']);
  }
}