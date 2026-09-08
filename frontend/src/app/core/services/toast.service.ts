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

  showSuccess(message: string, duration: number = 7000): void {
    this.addToast('success', message, duration);
  }

  showError(message: string, duration: number = 7000): void {
    this.addToast('error', message, duration);
  }

  remove(id: string): void {
    this.toasts.update(currentToasts => currentToasts.filter(t => t.id !== id));
  }

  private addToast(type: ToastType, message: string, duration: number): void {
    const id = Math.random().toString(36).substring(2, 9);
    
    this.toasts.update(currentToasts => {
    if (currentToasts.some(t => t.message === message)) {
      return currentToasts;
    }

    const novaLista = [...currentToasts, { id, type, message }];

    if (novaLista.length > 5) {
      return novaLista.slice(-5);
    }

    return novaLista;
  });

  setTimeout(() => {
    this.remove(id);
  }, duration);
}
}