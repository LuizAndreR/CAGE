import { TokenService } from '../../core/auth/token.service';
import { Component, inject, signal } from '@angular/core';
import { ConfigPerfil } from '../../shared/components/config/config-perfil/config-perfil';
import { ConfigUsuarios } from '../../shared/components/config/config-usuarios/config-usuarios';
import { ConfigEmpresa } from '../../shared/components/config/config-empresa/config-empresa';

type AbaConfiguracao = 'perfil' | 'usuarios' | 'empresa';

@Component({
  selector: 'app-configuracoes',
  imports: [ConfigPerfil, ConfigUsuarios, ConfigEmpresa],
  templateUrl: './configuracoes.html',
  styleUrl: './configuracoes.css',
})
export default class Configuracoes {
  readonly podeEditarEmpresa = inject(TokenService).getRole() === 'Dono';
  abaAtiva = signal<AbaConfiguracao>('perfil');
  salvando = signal(false);

  abas: { id: AbaConfiguracao; titulo: string; icone: string }[] = [
    { id: 'perfil', titulo: 'Perfil', icone: 'person' },
    { id: 'usuarios', titulo: 'Usuários', icone: 'group' },
    ...(this.podeEditarEmpresa ? [{ id: 'empresa' as const, titulo: 'Empresa', icone: 'storefront' }] : [])
  ];
}
