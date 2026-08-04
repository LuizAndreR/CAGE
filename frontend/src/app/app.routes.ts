import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard').then(c => c.Dashboard)
    },
    {
        path: 'estoque',
        loadComponent: () => import('./pages/estoque/estoque').then(c => c.Estoque)
    },
    {
        path: 'financeiro',
        loadComponent: () => import('./pages/financeiro/financeiro').then(c => c.Financeiro)
    },
    {
        path: 'pedidos',
        loadComponent: () => import('./pages/pedidos/pedidos').then(c => c.Pedidos)
    },
    {   
        path: 'receitas',
        loadComponent: () => import('./pages/receitas/receitas').then(c => c.Receitas)
    },
    {
        path: 'configuracoes',
        loadComponent: () => import('./pages/configuracoes/configuracoes').then(c => c.Configuracoes)
    }
];
