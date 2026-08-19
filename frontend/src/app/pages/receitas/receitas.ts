import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceitaService } from '../../core/service/receita.service';
import { ReceitaListResponse } from '../../core/models/receita.interface';

// Importação do componente de listagem que você acabou de criar
import { ReceitaList } from '../../shared/components/receita-list/receita-list';

@Component({
  selector: 'app-receita-page',
  standalone: true,
  imports: [CommonModule, ReceitaList],
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

  // Signal Computado para o filtro das abas
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
    this.view.set('form'); // Vai para a tela de criação futuramente
  }

  onSelecionarReceita(id: number): void {
    this.selectedId.set(id);
    this.view.set('detail'); // Vai para a tela de detalhes futuramente
  }

  onAlternarStatus(evento: { id: number, statusAtual: boolean }): void {
    // Implementação otimista (muda na tela antes da API confirmar)
    const novoStatus = !evento.statusAtual;
    
    this.receitas.update(lista => 
      lista.map(r => r.id === evento.id ? { ...r, status: novoStatus } : r)
    );

    this.receitaService.mudarStatus(evento.id, novoStatus).subscribe({
      error: (erro) => {
        console.error('Erro ao mudar status:', erro);
        this.carregarReceitas(); // Reverte a lista se der erro
      }
    });
  }
}