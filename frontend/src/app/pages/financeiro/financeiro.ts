import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FinanceiroService } from '../../core/service/financeiro.service';
import { ToastService } from '../../core/service/toast.service';
import { TransacaoResponse, TransacaoResumo } from '../../core/models/financeiro.interface';
import { FinanceiroList } from '../../shared/components/financeiro/financeiro-list/financeiro-list';

@Component({
  selector: 'app-financeiro-page',
  standalone: true,
  imports: [CommonModule, FinanceiroList], 
  templateUrl: './financeiro.html',
  styleUrl: './financeiro.css'
})
export default class Financeiro implements OnInit {
  private financeiroService = inject(FinanceiroService);
  private toast = inject(ToastService);

  view = signal<'list' | 'detail' | 'form'>('list');
  isLoading = signal<boolean>(true);
  
  transacoes = signal<TransacaoResponse[]>([]);
  resumo = signal<TransacaoResumo>({ entrada: 0, saida: 0 });
  selectedId = signal<number | null>(null);

  ngOnInit(): void {
    this.carregarDados();
  }

  // Tipagem forte para garantir que strings vazias não cheguem aqui
  carregarDados(filtros?: { tipo?: string, categoria?: string, ano?: number, mes?: number }): void {
    this.isLoading.set(true);

    this.financeiroService.getResumo().subscribe({
      next: (dados) => this.resumo.set(dados),
      error: () => this.toast.showError('Não foi possível carregar o resumo financeiro.')
    });

    this.financeiroService.getTransacoes(filtros).subscribe({
      next: (dados) => {
        this.transacoes.set(dados);
        this.isLoading.set(false);
      },
      error: (erro) => {
        // Interceptação pacífica do 404
        if (erro.status === 404) {
          this.transacoes.set([]); 
          this.isLoading.set(false);
        } else {
          console.error('Falha ao buscar transações:', erro);
          this.toast.showError('Erro ao carregar o extrato de transações.');
          this.isLoading.set(false);
        }
      }
    });
  }

  onFiltrar(filtros: { tipo?: string, categoria?: string, ano?: number, mes?: number }): void {
    this.carregarDados(filtros);
  }

  onNovaTransacao(): void {
    this.selectedId.set(null);
    this.view.set('form');
  }

  onSelecionarTransacao(id: number): void {
    this.selectedId.set(id);
    this.view.set('detail');
  }
}