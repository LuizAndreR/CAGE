import { Component, DestroyRef, inject, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { EmpresaService } from '../../../../core/services/empresa.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-config-empresa',
  imports: [ReactiveFormsModule],
  templateUrl: './config-empresa.html',
  styleUrl: './config-empresa.css'
})
export class ConfigEmpresa {
  private fb = inject(FormBuilder);
  private empresaService = inject(EmpresaService);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  salvandoChange = output<boolean>();
  salvando = signal(false);

  empresaForm = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(150)]],
    endereco: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(250)]]
  });

  salvarEmpresa(): void {
    if (this.salvando()) return;

    const { nome, endereco } = this.empresaForm.getRawValue();
    this.empresaForm.patchValue({ nome: nome.trim(), endereco: endereco.trim() });

    if (this.empresaForm.invalid) {
      this.empresaForm.markAllAsTouched();
      return;
    }

    const payload = this.empresaForm.getRawValue();
    this.salvando.set(true);
    this.salvandoChange.emit(true);

    this.empresaService.atualizarEmpresa(payload).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.salvando.set(false);
        this.salvandoChange.emit(false);
      })
    ).subscribe({
      next: () => {
        this.empresaForm.reset(payload);
        this.toast.showSuccess('Dados da empresa atualizados com sucesso!');
      },
      error: () => { /* O interceptor exibe o erro; a edição é preservada para nova tentativa. */ }
    });
  }
}
