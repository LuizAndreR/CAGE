import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { SummaryCard } from '../../shared/components/summary-card/summary-card';
import { ListPanel } from '../../shared/components/list-panel/list-panel';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, MatIconModule, SummaryCard, ListPanel],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})

export class Dashboard {
  nomeUsuario: string = 'Ana Maria';
  
  cardsConfig = [
    { label: "Estoque Total", icon: "inventory_2", colorClass: "card-blue", id: "estoque" },
    { label: "Receita (Mês)", icon: "trending_up", colorClass: "card-emerald", id: "receita" },
    { label: "Despesas (Mês)", icon: "trending_down", colorClass: "card-rose", id: "despesas" },
    { label: "Lucro Líquido", icon: "account_balance_wallet", colorClass: "card-indigo", id: "lucro" }
  ];

  recentOrders = [
    { id: "1042", cliente: "Maria Oliveira", valor: "R$ 450,00", status: "Em Preparo", date: "Hoje, 14:30" },
    { id: "1041", cliente: "João Pedro", valor: "R$ 120,00", status: "Entregue", date: "Hoje, 10:15" },
    { id: "1040", cliente: "Casamento Ana", valor: "R$ 2.400,00", status: "Aguardando", date: "Ontem, 16:40" },
    { id: "1039", cliente: "Empresa XPTO", valor: "R$ 300,00", status: "Entregue", date: "Ontem, 09:20" }
  ];

  recentRecipes = [
    { name: "Bolo de Cenoura c/ Chocolate", type: "Bolo Inteiro", cost: "R$ 15,20", price: "R$ 45,00" },
    { name: "Brigadeiro Gourmet (100un)", type: "Docinhos", cost: "R$ 32,00", price: "R$ 120,00" },
    { name: "Torta de Limão", type: "Sobremesa", cost: "R$ 18,50", price: "R$ 60,00" }
  ];

  dashboardData = {
    estoque: "0",
    receita: "R$ 0,00",
    despesas: "R$ 0,00",
    lucro: "R$ 0,00"
  };

}