import { Injectable } from '@angular/core';

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly ACCESS_KEY = 'cage_access_token';
  private readonly REFRESH_KEY = 'cage_refresh_token';

  salvarTokens(tokens: AuthTokens): void {
    localStorage.setItem(this.ACCESS_KEY, tokens.accessToken);
    localStorage.setItem(this.REFRESH_KEY, tokens.refreshToken);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  getRole(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;
    try {
      const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const claims = JSON.parse(atob(payload.padEnd(Math.ceil(payload.length / 4) * 4, '=')));
      const role = claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? claims.role;
      return typeof role === 'string' ? role : null;
    } catch {
      return null;
    }
  }

  hasTokens(): boolean {
    return !!this.getAccessToken();
  }

  limparTokens(): void {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
  }
}