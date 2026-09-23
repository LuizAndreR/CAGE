import { AfterViewInit, Component, ElementRef, inject, input, OnDestroy, output, signal, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CadastroFuncionarioRequest, FUNCOES_FUNCIONARIO } from '../../../../../core/models/usuario.interface';

@Component({
  selector: 'app-config-usuario-form',
  imports: [ReactiveFormsModule],
  templateUrl: './config-usuario-form.html',
  styleUrl: './config-usuario-form.css'
})
export class ConfigUsuarioForm implements AfterViewInit, OnDestroy {
  private fb = inject(FormBuilder);
  private dialog = viewChild.required<ElementRef<HTMLDialogElement>>('modal');

  salvando = input(false);
  erro = input<string | null>(null);
  cancelar = output<void>();
  cadastrar = output<CadastroFuncionarioRequest>();
  mostrarSenha = signal(false);
  readonly funcoes = FUNCOES_FUNCIONARIO;

  usuarioForm = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.pattern(/\S/)]],
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.pattern(/\S/), Validators.minLength(6)]],
    role: this.fb.nonNullable.control<CadastroFuncionarioRequest['role']>('Atendente', [
      Validators.required, Validators.pattern(`^(${FUNCOES_FUNCIONARIO.join('|')})$`)
    ])
  });

  ngAfterViewInit(): void {
    // O dialog nativo mantém o foco no modal e torna o restante da página inativo.
    this.dialog().nativeElement.showModal();
  }

  ngOnDestroy(): void {
    this.dialog().nativeElement.close();
  }

  fechar(): void {
    if (!this.salvando()) this.cancelar.emit();
  }

  onEscape(event: Event): void {
    event.preventDefault();
    this.fechar();
  }

  salvar(): void {
    if (this.salvando()) return;
    const { nome, email } = this.usuarioForm.getRawValue();
    this.usuarioForm.patchValue({ nome: nome.trim(), email: email.trim() });

    if (this.usuarioForm.invalid) {
      this.usuarioForm.markAllAsTouched();
      return;
    }

    this.cadastrar.emit({ adminRole: false, ...this.usuarioForm.getRawValue() });
  }
}
