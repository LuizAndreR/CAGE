import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service'; 

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // O guard agora pergunta ao serviço se o token existe no navegador[cite: 3]
  if (authService.isLoggedIn()) {
    return true; 
  }

  // Se não estiver logado, bloqueia e manda para a tela inicial[cite: 1]
  router.navigate(['/login']);
  return false; 
};