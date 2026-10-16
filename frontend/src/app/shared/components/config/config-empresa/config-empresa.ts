import { Component, DestroyRef, inject, OnInit, output, signal } from '@angular/core';
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
export class ConfigEmpresa implements OnInit {
  private fb = inject(FormBuilder);
  private empresaService = inject(EmpresaService);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  salvandoChange = output<boolean>();
  salvando = signal(false);
  carregando = signal(false);
  empresaCarregada = signal(false);

  empresaForm = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(150)]],
    endereco: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(250)]]
  });

  ngOnInit(): void {
    this.carregarEmpresa();
  }

  carregarEmpresa(): void {
    if (this.carregando() || this.salvando()) return;
    this.carregando.set(true);
    this.empresaCarregada.set(false);
    this.empresaService.getMinhaEmpresa().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.carregando.set(false))
    ).subscribe({
      next: empresa => {
        this.empresaForm.reset({ nome: empresa.nome, endereco: empresa.endereco });
        this.empresaCarregada.set(true);
      },
      error: () => { /* O interceptor exibe o erro; a tela permite tentar novamente. */ }
    });
  }

  salvarEmpresa(): void {
    if (this.salvando() || !this.empresaCarregada()) return;

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
