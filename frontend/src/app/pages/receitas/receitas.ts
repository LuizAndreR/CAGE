import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs/operators'; // DICA DE SÊNIOR: Importação adicionada

import { ReceitaService } from '../../core/services/receita.service';
import { ToastService } from '../../core/services/toast.service'; 
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
  private toast = inject(ToastService);

  view = signal<'list' | 'detail' | 'form'>('list');
  isLoading = signal<boolean>(true);
  
  receitas = signal<ReceitaListResponse[]>([]);
  activeTab = signal<'todas' | 'ativas' | 'desativadas'>('todas');
  
  selectedId = signal<number | null>(null);
  receitaDetalhe = signal<ReceitaResponse | null>(null);

  ngOnInit(): void {
    this.carregarReceitas();
  }

  carregarReceitas(): void {
    this.isLoading.set(true);

    this.receitaService.getReceitas()
      .pipe(finalize(() => this.isLoading.set(false))) // Loading centralizado
      .subscribe({
        next: (dados) => {
          this.receitas.set(dados);
        },
        error: (erro) => console.error('Falha ao carregar as receitas da API:', erro)
        // Toast de erro removido: o Interceptor já cuida disso.
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
      error: (erro) => console.error(`Falha ao carregar a receita ${id}:`, erro)
      // Toast de erro removido
    });
  }

  onAlternarStatus(evento: { id: number, statusAtual: boolean }): void {
    const novoStatus = !evento.statusAtual;
    
    // Atualização otimista local
    this.receitas.update(lista => 
      lista.map(r => r.id === evento.id ? { ...r, status: novoStatus } : r)
    );

    this.receitaService.mudarStatus(evento.id, novoStatus).subscribe({
      next: () => this.toast.showSuccess('Status atualizado com sucesso!'),
      error: (erro) => {
        console.error('Erro ao mudar status:', erro);
        this.carregarReceitas(); // Reverte a lista buscando do banco novamente, sem duplicar o Toast
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
    this.toast.showSuccess('Receita salva com sucesso!');
    this.view.set('list');
  }

  onExcluir(id: number): void {
    this.receitaService.excluirReceita(id).subscribe({
      next: () => {
        this.receitas.update(lista => lista.filter(r => r.id !== id));
        this.receitaDetalhe.set(null);
        this.selectedId.set(null);
        this.view.set('list');
        
        this.toast.showSuccess('Receita excluída permanentemente.');
      },
      error: (erro) => console.error('Falha ao excluir a receita na API:', erro)
      // Toast de erro removido
    });
  }
}