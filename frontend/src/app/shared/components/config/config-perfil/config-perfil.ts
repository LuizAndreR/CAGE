import { Component, DestroyRef, inject, OnInit, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { UserService } from '../../../../core/services/user.service';
import { ToastService } from '../../../../core/services/toast.service';

function confirmarSenha(control: AbstractControl): ValidationErrors | null {
  return control.get('novaSenha')?.value === control.get('confirmarNovaSenha')?.value
    ? null
    : { senhasDiferentes: true };
}

@Component({
  selector: 'app-config-perfil',
  imports: [ReactiveFormsModule],
  templateUrl: './config-perfil.html',
  styleUrl: './config-perfil.css'
})
export class ConfigPerfil implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  salvandoChange = output<boolean>();
  carregando = signal(false);
  perfilCarregado = signal(false);
  salvandoPerfil = signal(false);
  salvandoSenha = signal(false);
  mostrarSenhaAtual = signal(false);
  mostrarNovaSenha = signal(false);
  mostrarConfirmacao = signal(false);

  perfilForm = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.pattern(/\S/)]],
    email: ['', [Validators.required, Validators.email]]
  });

  senhaForm = this.fb.nonNullable.group({
    senhaAtual: ['', [Validators.required, Validators.pattern(/\S/), Validators.minLength(6)]],
    novaSenha: ['', [Validators.required, Validators.pattern(/\S/), Validators.minLength(6)]],
    confirmarNovaSenha: ['', Validators.required]
  }, { validators: confirmarSenha });

  ngOnInit(): void {
    this.carregarPerfil();
  }

  carregarPerfil(): void {
    if (this.carregando()) return;
    this.carregando.set(true);
    this.perfilCarregado.set(false);

    this.userService.getPerfil().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.carregando.set(false))
    ).subscribe({
      next: (usuario) => {
        this.perfilForm.reset({ nome: usuario.nome, email: usuario.email });
        this.perfilCarregado.set(true);
      },
      // As mensagens da API são exibidas pelo interceptor global.
      error: () => this.perfilCarregado.set(false)
    });
  }

  salvarPerfil(): void {
    if (!this.perfilCarregado() || this.carregando() || this.salvandoPerfil()) return;

    const { nome, email } = this.perfilForm.getRawValue();
    this.perfilForm.patchValue({ nome: nome.trim(), email: email.trim() });

    if (this.perfilForm.invalid) {
      this.perfilForm.markAllAsTouched();
      return;
    }

    const payload = this.perfilForm.getRawValue();
    this.salvandoPerfil.set(true);
    this.notificarSalvamento();

    this.userService.atualizarPerfil(payload).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.salvandoPerfil.set(false);
        this.notificarSalvamento();
      })
    ).subscribe({
      next: () => {
        this.perfilForm.reset(payload);
        this.toast.showSuccess('Perfil atualizado com sucesso!');
      },
      error: () => { /* O interceptor exibe o erro; os dados são preservados para nova tentativa. */ }
    });
  }

  alterarSenha(): void {
    if (this.salvandoSenha()) return;

    if (this.senhaForm.invalid) {
      this.senhaForm.markAllAsTouched();
      return;
    }

    // A confirmação é uma validação local e não faz parte do contrato da API.
    const { senhaAtual, novaSenha } = this.senhaForm.getRawValue();
    this.salvandoSenha.set(true);
    this.notificarSalvamento();

    this.userService.alterarSenha({ senhaAtual, novaSenha }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.salvandoSenha.set(false);
        this.notificarSalvamento();
      })
    ).subscribe({
      next: () => {
        this.senhaForm.reset();
        this.mostrarSenhaAtual.set(false);
        this.mostrarNovaSenha.set(false);
        this.mostrarConfirmacao.set(false);
        this.toast.showSuccess('Senha atualizada com sucesso!');
      },
      error: () => { /* O interceptor global trata falhas, incluindo senha atual incorreta. */ }
    });
  }

  private notificarSalvamento(): void {
    this.salvandoChange.emit(this.salvandoPerfil() || this.salvandoSenha());
  }
}
