import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs/operators'; // DICA DE SÊNIOR: Importação do finalize
import { FinanceiroService } from '../../core/services/financeiro.service';
import { ToastService } from '../../core/services/toast.service';

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
      error: (erro) => console.error('Falha ao carregar o resumo:', erro) // Toast removido!
    });

    this.financeiroService.getTransacoes(filtros)
      .pipe(finalize(() => this.isLoading.set(false))) // Loading centralizado!
      .subscribe({
        next: (dados) => {
          this.transacoes.set(dados);
        },
        error: (erro) => {
          // Mantemos apenas a regra visual para 404. O erro na tela fica por conta do interceptor.
          if (erro.status === 404) {
            this.transacoes.set([]); 
          } else {
            console.error('Falha ao buscar transações:', erro);
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

  onCancelarTransacao(evento: { id: number, motivo: string }): void {
    this.isLoading.set(true); 
    
    this.financeiroService.cancelarTransacao(evento.id, evento.motivo)
      .pipe(finalize(() => this.isLoading.set(false))) // Loading centralizado!
      .subscribe({
        next: () => {
          this.toast.showSuccess('Transação cancelada com sucesso!');
          this.view.set('list'); 
          this.selectedId.set(null); 
          this.carregarDados(); 
        },
        error: (erro) => console.error('Erro ao cancelar a transação:', erro) // Toast removido!
      });
  }

  onSalvarNovaTransacao(novaTransacao: { tipo: string, categoria: string, valor: number, data: string, descricao: string }): void {
    this.isLoading.set(true); 
    
    this.financeiroService.criarTransacao(novaTransacao)
      .pipe(finalize(() => this.isLoading.set(false))) // Loading centralizado!
      .subscribe({
        next: () => {
          this.toast.showSuccess('Transação registrada com sucesso!');
          this.view.set('list'); 
          this.selectedId.set(null); 
          this.carregarDados(); 
        },
        error: (erro) => console.error('Erro ao salvar transação:', erro) // Toast removido!
      });
  }
}