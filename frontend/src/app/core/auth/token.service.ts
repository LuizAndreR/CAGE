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

  hasTokens(): boolean {
    return !!this.getAccessToken();
  }

  limparTokens(): void {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
  }
}