import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceitaService } from '../../core/service/receita.service';
import { ReceitaListResponse, ReceitaResponse } from '../../core/models/receita.interface';

// Importação dos componentes filhos
import { ReceitaList } from '../../shared/components/receita-list/receita-list';
import { ReceitaDetail } from '../../shared/components/receita-detail/receita-detail'; 
import { ReceitaForm } from '../../shared/components/receita-form/receita-form'; 

@Component({
  selector: 'app-receita-page',
  standalone: true,
  imports: [CommonModule, ReceitaList, ReceitaDetail, ReceitaForm], 
  templateUrl: './receitas.html',
  styleUrl: './receitas.css'
})
export default class Receitas implements OnInit {
  private receitaService = inject(ReceitaService);

  view = signal<'list' | 'detail' | 'form'>('list');
  
  // Estado dos Dados
  receitas = signal<ReceitaListResponse[]>([]);
  activeTab = signal<'todas' | 'ativas' | 'desativadas'>('todas');
  
  selectedId = signal<number | null>(null);
  receitaDetalhe = signal<ReceitaResponse | null>(null);

  ngOnInit(): void {
    this.carregarReceitas();
  }

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

  onMudarAba(aba: 'todas' | 'ativas' | 'desativadas'): void {
    this.activeTab.set(aba);
  }

  onNovaReceita(): void {
    this.selectedId.set(null); // Garante que o ID é nulo (Modo Criação)
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

  onVoltar(): void {
    this.receitaDetalhe.set(null);
    this.selectedId.set(null);
    this.view.set('list');
  }

  // ATUALIZADO: Agora redireciona corretamente para o formulário de edição
  onEditar(id: number): void {
    this.selectedId.set(id); // Guarda o ID para o formulário saber que é edição
    this.view.set('form'); // Troca a tela
  }

  // NOVO: Método para lidar com o sucesso do salvamento
  onSalvoComSucesso(): void {
    this.carregarReceitas(); // Atualiza a lista com o novo dado do C#
    this.view.set('list');   // Volta para a tela principal
  }

  onExcluir(id: number): void {
    this.receitaService.excluirReceita(id).subscribe({
      next: () => {
        // Atualiza o estado removendo a receita excluída da lista
        this.receitas.update(lista => lista.filter(r => r.id !== id));
        
        // Limpa a seleção e volta para a aba de listagem
        this.receitaDetalhe.set(null);
        this.selectedId.set(null);
        this.view.set('list');
      },
      error: (erro) => {
        console.error('Falha ao excluir a receita na API:', erro);
        // Futuramente, podemos adicionar um aviso visual (Toast) aqui
      }
    });
  }
}