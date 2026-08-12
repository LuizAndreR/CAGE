import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'login', 
        pathMatch: 'full'   
    },
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login').then(c => c.Login)
    },
    {
        path: 'dashboard',
        canActivate: [authGuard], 
        loadComponent: () => import('./pages/dashboard/dashboard').then(c => c.Dashboard)
    },
    {
        path: 'estoque',
        canActivate: [authGuard],
        loadComponent: () => import('./pages/estoque/estoque').then(c => c.Estoque)
    },
    {
        path: 'financeiro',
        canActivate: [authGuard],
        loadComponent: () => import('./pages/financeiro/financeiro').then(c => c.Financeiro)
    },
    {
        path: 'pedidos',
        canActivate: [authGuard],
        loadComponent: () => import('./pages/pedidos/pedidos').then(c => c.Pedidos)
    },
    {   
        path: 'receitas',
        canActivate: [authGuard],
        loadComponent: () => import('./pages/receitas/receitas').then(c => c.Receitas)
    },
    {
        path: 'configuracoes',
        canActivate: [authGuard],
        loadComponent: () => import('./pages/configuracoes/configuracoes').then(c => c.Configuracoes)
    },
    {
        path: '**',
        redirectTo: 'login'
    }
];