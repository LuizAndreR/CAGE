import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceitaService } from '../../core/service/receita.service';
import { ReceitaListResponse, ReceitaResponse } from '../../core/models/receita.interface';

// Importação dos componentes filhos
import { ReceitaList } from '../../shared/components/receita-list/receita-list';
import { ReceitaDetail } from '../../shared/components/receita-detail/receita-detail'; // Ajuste o caminho se necessário

@Component({
  selector: 'app-receita-page',
  standalone: true,
  imports: [CommonModule, ReceitaList, ReceitaDetail], 
  templateUrl: './receitas.html',
  styleUrl: './receitas.css'
})
export default class Receitas implements OnInit {
  private receitaService = inject(ReceitaService);

  // Controle de Navegação da Tela
  view = signal<'list' | 'detail' | 'form'>('list');
  
  // Estado dos Dados
  receitas = signal<ReceitaListResponse[]>([]);
  activeTab = signal<'todas' | 'ativas' | 'desativadas'>('todas');
  
  selectedId = signal<number | null>(null);
  
  // NOVO: Signal para guardar os dados completos da receita selecionada
  receitaDetalhe = signal<ReceitaResponse | null>(null);

  ngOnInit(): void {
    this.carregarReceitas();
  }

  // --- LÓGICA DE DADOS ---
  carregarReceitas(): void {
    this.receitaService.getReceitas().subscribe({
      next: (dados) => this.receitas.set(dados),
      error: (erro) => console.error('Falha ao carregar as receitas da API:', erro)
    });
  }

  filteredReceitas = computed(() => {
    const lista = this.receitas();
    const filtro = this.activeTab();

    if (filtro === 'todas') return lista;
    if (filtro === 'ativas') return lista.filter(r => r.status === true);
    return lista.filter(r => r.status === false);
  });

  // --- ORQUESTRAÇÃO DE EVENTOS DOS FILHOS ---
  onMudarAba(aba: 'todas' | 'ativas' | 'desativadas'): void {
    this.activeTab.set(aba);
  }

  onNovaReceita(): void {
    this.selectedId.set(null);
    this.receitaDetalhe.set(null);
    this.view.set('form'); 
  }

  // ATUALIZADO: Agora ele busca os detalhes na API antes de trocar de tela
  onSelecionarReceita(id: number): void {
    this.selectedId.set(id);
    
    this.receitaService.getReceitaById(id).subscribe({
      next: (dadosCompletos) => {
        this.receitaDetalhe.set(dadosCompletos);
        this.view.set('detail'); // Só muda a tela quando os dados chegarem com sucesso
      },
      error: (erro) => console.error(`Falha ao carregar a receita ${id}:`, erro)
    });
  }

  onAlternarStatus(evento: { id: number, statusAtual: boolean }): void {
    const novoStatus = !evento.statusAtual;
    this.receitas.update(lista => 
      lista.map(r => r.id === evento.id ? { ...r, status: novoStatus } : r)
    );

    this.receitaService.mudarStatus(evento.id, novoStatus).subscribe({
      error: (erro) => {
        console.error('Erro ao mudar status:', erro);
        this.carregarReceitas();
      }
    });
  }

  // NOVOS MÉTODOS: Ações da tela de detalhes
  onVoltar(): void {
    this.receitaDetalhe.set(null);
    this.selectedId.set(null);
    this.view.set('list');
  }

  onEditar(id: number): void {
    console.log('Navegar para edição da receita:', id);
    // Será implementado quando fizermos o formulário
  }

  onExcluir(id: number): void {
    console.log('Disparar exclusão da receita:', id);
    // Será implementado futuramente
  }
}