import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms'; 
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule], 
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  showSenha = signal(false);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(6)]]
  });

  toggleSenha(): void {
    this.showSenha.update(v => !v);
  }

  handleEntrar(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const { email, senha } = this.loginForm.getRawValue();

    this.authService.login({ email, senha }).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('E-mail ou senha incorretos. Tente novamente.');
        console.error('Erro de Autenticação:', err);
      }
    });
  }
}