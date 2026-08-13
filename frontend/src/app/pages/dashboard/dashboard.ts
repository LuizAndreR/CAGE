import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { SummaryCard } from '../../shared/components/summary-card/summary-card';
import { ListPanel } from '../../shared/components/list-panel/list-panel';
import { DashboardService } from './dashboard.service';
import { DashboardResumo } from '../../core/models/dashboard.interface';

export interface ReceitaView {
  name: string;
  price: string;
}

export interface PedidoView {
  id: string;
  cliente: string;
  valor: string;
  date: string;
  status: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, MatIconModule, SummaryCard, ListPanel],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export default class Dashboard implements OnInit {
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef); 

  nomeUsuario: string = '';
  recentOrders: PedidoView[] = [];
  recentRecipes: ReceitaView[] = [];

  dashboardData = {
    estoque: "0",
    receita: "R$ 0,00",
    despesas: "R$ 0,00",
    lucro: "R$ 0,00"
  };

  ngOnInit(): void {
    this.carregarDadosApi();
  }

  private carregarDadosApi(): void {
    this.dashboardService.getResumo().subscribe({
      next: (dadosDaApi: DashboardResumo) => {

        this.nomeUsuario = dadosDaApi.nomeUsuario;

        this.dashboardData = {
          estoque: dadosDaApi.totalItensEstoque.toString(),
          receita: this.formatarMoeda(dadosDaApi.financeiro.totalEntradas),
          despesas: this.formatarMoeda(dadosDaApi.financeiro.totalSaidas),
          lucro: this.formatarMoeda(dadosDaApi.financeiro.saldoAtual)
        };

        this.recentRecipes = dadosDaApi.ultimasReceitas.map(receita => ({
          name: receita.nome,
          price: this.formatarMoeda(receita.precoVenda)
        }));

        this.recentOrders = dadosDaApi.ultimosPedidos.map(pedido => ({
          id: pedido.pedidoId.toString(),
          cliente: pedido.clienteNome,
          valor: this.formatarMoeda(pedido.valorTotal),
          date: new Date(pedido.dataCriacao).toLocaleDateString('pt-BR'),
          status: pedido.status
        }));

        this.cdr.detectChanges();
      },
      error: (erro) => {
        console.error('Falha de conexão com a API do Cake Gestão:', erro);
      }
    });
  }

  private formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
  }
}