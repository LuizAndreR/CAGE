import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service'; 

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      
      if (error.status === 400 && error.error?.errors?.length > 0) {
        const mensagemApi = error.error.errors[0]; 
        toast.showError(mensagemApi);
      } 
      else if (error.status === 409 && error.error?.error) {
        const mensagemApi = error.error.error;
        toast.showError(Array.isArray(mensagemApi) ? mensagemApi[0] : mensagemApi);
      }
      else if (error.status === 500 && error.error?.error) {
        toast.showError(error.error.error);
      } 
      else {
        toast.showError('Não foi possível se comunicar com o servidor.');
      }

      return throwError(() => error);
    })
  );
};
