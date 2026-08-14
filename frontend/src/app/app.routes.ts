import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
{
        path: '',
        redirectTo: 'login', 
        pathMatch: 'full'   
    },
    {
        path: 'login',
        canActivate: [guestGuard], 
        loadComponent: () => import('./pages/login/login')
    },
    {
        path: '',
        canActivate: [authGuard], 
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('./pages/dashboard/dashboard')
            },
            {
                path: 'estoque',
                loadComponent: () => import('./pages/estoque/estoque')
            },
            {
                path: 'financeiro',
                loadComponent: () => import('./pages/financeiro/financeiro')
            },
            {
                path: 'pedidos',
                loadComponent: () => import('./pages/pedidos/pedidos')
            },
            {   
                path: 'receitas',
                loadComponent: () => import('./pages/receitas/receitas')
            },
            {
                path: 'configuracoes',
                loadComponent: () => import('./pages/configuracoes/configuracoes')
            }
        ]
    },
    {
        path: '**',
        redirectTo: 'login'
    }
];