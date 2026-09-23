import { Component, DestroyRef, inject, OnInit, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { CadastroFuncionarioRequest, UsuarioResponse } from '../../../../core/models/usuario.interface';
import { UserService } from '../../../../core/services/user.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { ToastService } from '../../../../core/services/toast.service';
import { ConfigUsuarioForm } from './config-usuario-form/config-usuario-form';

@Component({
  selector: 'app-config-usuarios',
  imports: [ConfigUsuarioForm],
  templateUrl: './config-usuarios.html',
  styleUrl: './config-usuarios.css'
})
export class ConfigUsuarios implements OnInit {
  private userService = inject(UserService);
  private destroyRef = inject(DestroyRef);
  private authService = inject(AuthService);
  private toast = inject(ToastService);

  salvandoChange = output<boolean>();
  modalCadastroAberto = signal(false);
  salvandoCadastro = signal(false);
  erroCadastro = signal<string | null>(null);

  usuarios = signal<UsuarioResponse[]>([]);
  carregando = signal(false);
  erro = signal<string | null>(null);

  ngOnInit(): void {
    this.carregarUsuarios();
  }

  abrirCadastro(): void {
    this.erroCadastro.set(null);
    this.modalCadastroAberto.set(true);
  }

  fecharCadastro(): void {
    if (this.salvandoCadastro()) return;
    this.modalCadastroAberto.set(false);
    this.erroCadastro.set(null);
  }

  cadastrarUsuario(payload: CadastroFuncionarioRequest): void {
    if (this.salvandoCadastro()) return;
    this.salvandoCadastro.set(true);
    this.salvandoChange.emit(true);
    this.erroCadastro.set(null);

    this.authService.cadastrarFuncionario(payload).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.salvandoCadastro.set(false);
        this.salvandoChange.emit(false);
      })
    ).subscribe({
      next: () => {
        this.modalCadastroAberto.set(false);
        this.toast.showSuccess('Usuário cadastrado com sucesso!');
        this.carregarUsuarios();
      },
      error: (erro) => {
        const mensagem = erro.error?.errors?.[0] ?? erro.error?.error;
        this.erroCadastro.set(erro.status === 403
          ? 'Você não tem permissão para cadastrar usuários nesta empresa.'
          : typeof mensagem === 'string' ? mensagem : 'Não foi possível cadastrar o usuário. Tente novamente.');
      }
    });
  }

  carregarUsuarios(): void {
    if (this.carregando()) return;
    this.carregando.set(true);
    this.erro.set(null);

    this.userService.getFuncionarios().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.carregando.set(false))
    ).subscribe({
      next: (usuarios) => this.usuarios.set(usuarios),
      error: (erro) => {
        this.usuarios.set([]);
        this.erro.set(erro.status === 403
          ? 'Você não tem permissão para consultar os usuários desta empresa.'
          : 'Não foi possível carregar os usuários da empresa.');
      }
    });
  }

  extrairIniciais(nome: string): string {
    const partes = nome.trim().split(/\s+/).filter(Boolean);
    if (!partes.length) return '?';
    return (partes.length === 1
      ? partes[0].slice(0, 2)
      : partes[0][0] + partes[partes.length - 1][0]).toUpperCase();
  }

  formatarDataInicio(valor: string): string {
    // DateTime.MinValue representa uma data ainda não informada na API.
    if (!valor || valor.startsWith('0001-01-01')) return 'Não informada';
    const data = valor.slice(0, 10);
    if (!/^\d{4}-\d{2}-\d{2}$/.test(data)) return 'Não informada';
    const dataUtc = new Date(`${data}T00:00:00Z`);
    if (Number.isNaN(dataUtc.getTime()) || dataUtc.toISOString().slice(0, 10) !== data) {
      return 'Não informada';
    }
    // Preserva o dia cadastrado, sem deslocá-lo pelo fuso horário do navegador.
    return `${data.slice(8, 10)}/${data.slice(5, 7)}/${data.slice(0, 4)}`;
  }
}
