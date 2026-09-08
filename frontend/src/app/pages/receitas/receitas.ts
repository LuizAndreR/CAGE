import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceitaService } from '../../core/service/receita.service';
import { ToastService } from '../../core/service/toast.service'; 
import { ReceitaListResponse, ReceitaResponse } from '../../core/models/receita.interface';

import { ReceitaList } from '../../shared/components/receita/receita-list/receita-list';
import { ReceitaDetail } from '../../shared/components/receita/receita-detail/receita-detail'; 
import { ReceitaForm } from '../../shared/components/receita/receita-form/receita-form'; 

@Component({
  selector: 'app-receita-page',
  standalone: true,
  imports: [CommonModule, ReceitaList, ReceitaDetail, ReceitaForm], 
  templateUrl: './receitas.html',
  styleUrl: './receitas.css'
})
export default class Receitas implements OnInit {
  private receitaService = inject(ReceitaService);
  private toast = inject(ToastService); // INJEÇÃO DO SERVIÇO

  view = signal<'list' | 'detail' | 'form'>('list');
  
  // NOVO: Controle de estado de carregamento da tela
  isLoading = signal<boolean>(true);
  
  receitas = signal<ReceitaListResponse[]>([]);
  activeTab = signal<'todas' | 'ativas' | 'desativadas'>('todas');
  
  selectedId = signal<number | null>(null);
  receitaDetalhe = signal<ReceitaResponse | null>(null);

  ngOnInit(): void {
    this.carregarReceitas();
  }

  carregarReceitas(): void {
    this.isLoading.set(true); // Inicia o bloqueio da tela

    this.receitaService.getReceitas().subscribe({
      next: (dados) => {
        this.receitas.set(dados);
        this.isLoading.set(false); // Libera a tela
      },
      error: (erro) => {
        console.error('Falha ao carregar as receitas da API:', erro);
        this.toast.showError('Erro ao buscar as receitas no servidor.'); // FEEDBACK VISUAL
        this.isLoading.set(false);
      }
    });
  }

  filteredReceitas = computed(() => {
    const lista = this.receitas();
    const filtro = this.activeTab();

    if (filtro === 'todas') return lista;
    if (filtro === 'ativas') return lista.filter(r => r.status === true);
    return lista.filter(r => r.status === false);
  });

  onMudarAba(aba: 'todas' | 'ativas' | 'desativadas'): void {
    this.activeTab.set(aba);
  }

  onNovaReceita(): void {
    this.selectedId.set(null); 
    this.receitaDetalhe.set(null);
    this.view.set('form'); 
  }

  onSelecionarReceita(id: number): void {
    this.selectedId.set(id);
    
    this.receitaService.getReceitaById(id).subscribe({
      next: (dadosCompletos) => {
        this.receitaDetalhe.set(dadosCompletos);
        this.view.set('detail'); 
      },
      error: (erro) => {
        console.error(`Falha ao carregar a receita ${id}:`, erro);
        this.toast.showError('Não foi possível carregar os detalhes.'); // FEEDBACK VISUAL
      }
    });
  }

  onAlternarStatus(evento: { id: number, statusAtual: boolean }): void {
    const novoStatus = !evento.statusAtual;
    
    // Atualização otimista local
    this.receitas.update(lista => 
      lista.map(r => r.id === evento.id ? { ...r, status: novoStatus } : r)
    );

    this.receitaService.mudarStatus(evento.id, novoStatus).subscribe({
      next: () => this.toast.showSuccess('Status atualizado com sucesso!'), // FEEDBACK VISUAL
      error: (erro) => {
        console.error('Erro ao mudar status:', erro);
        this.toast.showError('Falha ao alterar o status da receita.'); // FEEDBACK VISUAL
        this.carregarReceitas(); // Reverte a lista buscando do banco novamente
      }
    });
  }

  onVoltar(): void {
    this.receitaDetalhe.set(null);
    this.selectedId.set(null);
    this.view.set('list');
  }

  onEditar(id: number): void {
    this.selectedId.set(id);
    this.view.set('form');
  }

  onSalvoComSucesso(): void {
    this.carregarReceitas();
    this.toast.showSuccess('Receita salva com sucesso!'); // FEEDBACK VISUAL
    this.view.set('list');
  }

  onExcluir(id: number): void {
    this.receitaService.excluirReceita(id).subscribe({
      next: () => {
        this.receitas.update(lista => lista.filter(r => r.id !== id));
        this.receitaDetalhe.set(null);
        this.selectedId.set(null);
        this.view.set('list');
        
        this.toast.showSuccess('Receita excluída permanentemente.'); // FEEDBACK VISUAL
      },
      error: (erro) => {
        console.error('Falha ao excluir a receita na API:', erro);
        this.toast.showError('Não foi possível excluir a receita.'); // FEEDBACK VISUAL
      }
    });
  }
}