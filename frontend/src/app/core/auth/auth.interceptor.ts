import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { TokenService } from './token.service'; 
import { AuthService } from './auth.service';   

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const authService = inject(AuthService);

  // 1. Pega o token atual salvo no LocalStorage
  const token = tokenService.getAccessToken();

  // 2. Não enviamos o token nas rotas de login ou refresh para evitar conflitos com a API
  const isAuthRoute = req.url.includes('/auth/login') || req.url.includes('/auth/refresh');

  let authReq = req;
  
  // 3. Se temos um token e não é uma rota de autenticação, clonamos a requisição e injetamos o JWT
  if (token && !isAuthRoute) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  // 4. Envia a requisição para a API Layer em ASP.NET Core 8
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      
      // 5. Se a API barrar (401), tentamos o Refresh Token silenciosamente
      if (error.status === 401) {
        if (tokenService.hasTokens()) {
          return authService.refreshToken().pipe(
            switchMap((newTokens) => {
              // Deu certo! Refaz a requisição original com o novo token
              const newReq = req.clone({
                setHeaders: { Authorization: `Bearer ${newTokens.accessToken}` }
              });
              return next(newReq);
            }),
            catchError((refreshError) => {
              // Se o refresh falhar, derruba a sessão por segurança
              authService.logout();
              return throwError(() => refreshError);
            })
          );
        } else {
           // Se tomou 401 e nem token tem, desloga na hora
           authService.logout();
        }
      }
      
      // Repassa qualquer outro erro (500, 404, 400) para o componente tratar
      return throwError(() => error);
    })
  );
};