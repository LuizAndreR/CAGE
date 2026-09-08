import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error';

export interface Toast {
  id: string;
  type: ToastType;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  // Signal que guarda a lista de notificações ativas
  toasts = signal<Toast[]>([]);

  showSuccess(message: string): void {
    this.addToast('success', message);
  }

  showError(message: string): void {
    this.addToast('error', message);
  }

  remove(id: string): void {
    this.toasts.update(currentToasts => currentToasts.filter(t => t.id !== id));
  }

  private addToast(type: ToastType, message: string): void {
    const id = Math.random().toString(36).substring(2, 9);
    
    this.toasts.update(currentToasts => {
      // 1. Criamos a nova lista adicionando o aviso atual no final
      const novaLista = [...currentToasts, { id, type, message }];

      // 2. Se a lista passar de 5, nós "cortamos" e pegamos apenas os 5 últimos
      if (novaLista.length > 5) {
        return novaLista.slice(-5);
      }

      return novaLista;
    });

    // O setTimeout continua normal. Se o ID do aviso já tiver sido removido 
    // pela regra do limite de 5 acima, a função remove(id) simplesmente não fará nada.
    setTimeout(() => {
      this.remove(id);
    }, 3500);
  }
}