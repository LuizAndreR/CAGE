import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FinanceiroService } from '../../core/service/financeiro.service';
import { ToastService } from '../../core/service/toast.service';
import { TransacaoResponse, TransacaoResumo } from '../../core/models/financeiro.interface';

// Importação do nosso componente de listagem (Dumb Component)
import { FinanceiroList } from '../../shared/components/financeiro/financeiro-list/financeiro-list';

@Component({
  selector: 'app-financeiro-page',
  standalone: true,
  imports: [CommonModule, FinanceiroList], 
  templateUrl: './financeiro.html',
  styleUrl: './financeiro.css' // Pode usar o mesmo CSS base de layout (page-wrapper)
})
export default class Financeiro implements OnInit {
  private financeiroService = inject(FinanceiroService);
  private toast = inject(ToastService);

  // Controle de Telas
  view = signal<'list' | 'detail' | 'form'>('list');
  isLoading = signal<boolean>(true);
  
  // Estado dos Dados
  transacoes = signal<TransacaoResponse[]>([]);
  resumo = signal<TransacaoResumo>({ entrada: 0, saida: 0 });
  selectedId = signal<number | null>(null);

  ngOnInit(): void {
    this.carregarDados();
  }

  // Carrega a lista e o resumo simultaneamente
  carregarDados(filtros?: { tipo?: string, categoria?: string, mes?: string }): void {
    this.isLoading.set(true);

    // Dispara a busca do resumo
    this.financeiroService.getResumo().subscribe({
      next: (dados) => this.resumo.set(dados),
      error: () => this.toast.showError('Não foi possível carregar o resumo financeiro.')
    });

    // Dispara a busca da lista passando os filtros se existirem
    this.financeiroService.getTransacoes(filtros).subscribe({
      next: (dados) => {
        this.transacoes.set(dados);
        this.isLoading.set(false);
      },
      error: (erro) => {
        console.error('Falha ao buscar transações:', erro);
        this.toast.showError('Erro ao carregar o extrato de transações.');
        this.isLoading.set(false);
      }
    });
  }

  // Evento emitido pelo componente filho quando o usuário muda um select
  onFiltrar(filtros: { tipo: string, categoria: string, mes: string }): void {
    this.carregarDados(filtros);
  }

  // Navegação
  onNovaTransacao(): void {
    this.selectedId.set(null);
    this.view.set('form');
  }

  onSelecionarTransacao(id: number): void {
    this.selectedId.set(id);
    this.view.set('detail');
  }
}