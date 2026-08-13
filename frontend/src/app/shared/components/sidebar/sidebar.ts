import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UserService } from '../../../pages/configuracoes/user.service'; 
import { UsuarioResponse } from '../../../core/models/usuario.interface';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar implements OnInit {
  private userService = inject(UserService);

  usuario = signal<UsuarioResponse | null>(null);
  iniciais = signal<string>('..'); 

  navItems = [
    { path: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
    { path: '/estoque', label: 'Estoque', icon: 'inventory_2' },
    { path: '/financeiro', label: 'Financeiro', icon: 'payments' },
    { path: '/receitas', label: 'Receitas', icon: 'cake' },
    { path: '/pedidos', label: 'Pedidos', icon: 'shopping_bag' },
    { path: '/configuracoes', label: 'Configurações', icon: 'settings' }
  ];

  ngOnInit(): void {
    this.carregarPerfil();
  }

  private carregarPerfil(): void {
    this.userService.getPerfil().subscribe({
      next: (dados) => {
        this.usuario.set(dados);
        this.iniciais.set(this.extrairIniciais(dados.nome));
      },
      error: (err) => {
        console.error('Erro ao carregar dados do usuário', err);
      }
    });
  }

  private extrairIniciais(nome: string): string {
    if (!nome) return '';
    const partes = nome.trim().split(' ');
    if (partes.length >= 2) {
      return (partes[0][0] + partes[partes.length - 1][0]).toUpperCase();
    }
    return nome.substring(0, 2).toUpperCase();
  }
}