import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FinanceiroService } from '../../core/service/financeiro.service';
import { ToastService } from '../../core/service/toast.service';

import { TransacaoResponse, TransacaoResumo } from '../../core/models/financeiro.interface';

import { FinanceiroList } from '../../shared/components/financeiro/financeiro-list/financeiro-list';
import { FinanceiroDetail } from '../../shared/components/financeiro/financeiro-detail/financeiro-detail';
import { FinanceiroForm } from '../../shared/components/financeiro/financeiro-form/financeiro-form';

@Component({
  selector: 'app-financeiro-page',
  standalone: true,
  imports: [CommonModule, FinanceiroList, FinanceiroDetail, FinanceiroForm], 
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

  transacaoSelecionada = computed(() => {
    const id = this.selectedId();
    if (!id) return null;
    return this.transacoes().find(t => t.id === id) || null;
  });

  ngOnInit(): void {
    this.carregarDados();
  }

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

  // IMPLEMENTAÇÃO REAL DO CANCELAMENTO
  onCancelarTransacao(evento: { id: number, motivo: string }): void {
    this.isLoading.set(true); // Ativa o loading na tela inteira
    
    this.financeiroService.cancelarTransacao(evento.id, evento.motivo).subscribe({
      next: () => {
        this.toast.showSuccess('Transação cancelada com sucesso!');
        this.view.set('list'); // Volta para a tela de lista
        this.selectedId.set(null); // Limpa a seleção
        this.carregarDados(); // Dispara nova busca para atualizar lista e resumo
      },
      error: (erro) => {
        console.error('Erro ao cancelar a transação:', erro);
        this.toast.showError('Falha ao tentar cancelar a transação.');
        this.isLoading.set(false); // Desliga o loading se falhar, mantendo o usuário na tela de detalhes
      }
    });
  }

  // IMPLEMENTAÇÃO REAL DO SALVAMENTO
  onSalvarNovaTransacao(novaTransacao: { tipo: string, categoria: string, valor: number, data: string, descricao: string }): void {
    this.isLoading.set(true); // Levanta a tela de carregamento global
    
    // Chama o serviço para enviar o POST para a API
    this.financeiroService.criarTransacao(novaTransacao).subscribe({
      next: () => {
        this.toast.showSuccess('Transação registrada com sucesso!');
        this.view.set('list'); // Retorna para a tela de lista
        this.selectedId.set(null); // Limpa a seleção
        this.carregarDados(); // Recarrega os dados do banco para atualizar tabela e saldo
      },
      error: (erro) => {
        console.error('Erro ao salvar transação:', erro);
        this.toast.showError('Não foi possível registrar a transação. Verifique os dados.');
        this.isLoading.set(false);
      }
    });
  }
} 