import { Component, computed, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EstoqueItem } from '../../core/models/estoque.interface';
import { EstoqueService } from '../../core/service/estoque.service';
import { ToastService } from '../../core/service/toast.service'; // IMPORTAÇÃO DO TOAST

import { EstoqueList } from '../../shared/components/estoque/estoque-list/estoque-list';
import { EstoqueDetail } from '../../shared/components/estoque/estoque-detail/estoque-detail';
import { EstoqueForm } from '../../shared/components/estoque/estoque-form/estoque-form';

@Component({
  selector: 'app-estoque',
  standalone: true,
  imports: [CommonModule, FormsModule, EstoqueList, EstoqueDetail, EstoqueForm],
  templateUrl: './estoque.html',
  styleUrl: './estoque.css'
})  
export default class Estoque implements OnInit {
  private estoqueService = inject(EstoqueService);
  private toast = inject(ToastService); // INJEÇÃO DO SERVIÇO DE TOAST

  view = signal<'list' | 'detail' | 'form'>('list');
  activeTab = signal<'todos' | 'baixo'>('todos');
  items = signal<EstoqueItem[]>([]);
  selectedId = signal<number | null>(null);
  search = signal<string>('');
  
  itemEditando = signal<EstoqueItem | null>(null);
  
  // Controle de carregamento (UX)
  isLoading = signal<boolean>(true);

  // Modais State
  actionModal = signal<{ isOpen: boolean; type: 'add' | 'remove'; itemId: number | null }>({ isOpen: false, type: 'add', itemId: null });
  actionAmount = signal<number | null>(null);
  actionValor = signal<number | null>(null);
  isDeleteModalOpen = signal<boolean>(false);

  ngOnInit(): void {
    this.carregarEstoque();
  }

  // --- LÓGICA DE DADOS ---
  carregarEstoque(): void {
    this.isLoading.set(true); // Bloqueia a tela
    
    const requisicao$ = this.activeTab() === 'todos' 
      ? this.estoqueService.getItens() 
      : this.estoqueService.getItensBaixoEstoque();

    requisicao$.subscribe({
      next: (dados) => {
        this.items.set(dados);
        this.isLoading.set(false); // Libera a tela
      },
      error: (erro) => {
        console.error('Erro na API:', erro);
        this.toast.showError('Falha ao carregar o estoque.'); // AVISO
        this.isLoading.set(false);
      }
    });
  }

  selectedItem = computed(() => this.items().find(i => i.id === this.selectedId()) || null);
  
  filteredItems = computed(() => {
    const termo = this.search().toLowerCase();
    return this.items().filter(item => 
      item.nome.toLowerCase().includes(termo) || (item.marca?.toLowerCase() || '').includes(termo)
    );
  });

  // --- EVENTOS DOS FILHOS ---
  onMudarAba(aba: 'todos' | 'baixo'): void {
    this.activeTab.set(aba);
    this.carregarEstoque();
  }

  onSelecionarItem(id: number): void {
    this.selectedId.set(id);
    this.changeView('detail');
  }

  onEditItem(): void {
    this.itemEditando.set(this.selectedItem());
    this.view.set('form');
  }

  changeView(newView: 'list' | 'detail' | 'form'): void {
    this.view.set(newView);
    if (newView === 'list') {
      this.selectedId.set(null);
      this.itemEditando.set(null);
    }
  }

  onSalvarItem(payload: any): void {
    // RESOLUÇÃO CLEAN CODE: Extraímos o ID e criamos um novo objeto sem mutar o payload original
    const { id, ...dadosEnvio } = payload;

    const operacao$ = id 
      ? this.estoqueService.atualizarItem(id, payload)
      : this.estoqueService.criarItem(dadosEnvio);

    operacao$.subscribe({
      next: () => {
        this.toast.showSuccess(id ? 'Item atualizado com sucesso!' : 'Novo item adicionado ao estoque!'); // AVISO
        this.changeView('list');
        this.carregarEstoque();
      },
      error: (erro) => {
        console.error('Erro ao salvar:', erro);
        this.toast.showError('Não foi possível salvar as informações do item.'); // AVISO
      }
    });
  }

  // --- MÉTODOS DOS MODAIS ---
  openActionModal(type: 'add' | 'remove', id: number): void {
    this.actionModal.set({ isOpen: true, type, itemId: id });
    this.actionAmount.set(null);
    this.actionValor.set(null);
  }

  closeActionModal(): void {
    this.actionModal.set({ isOpen: false, type: 'add', itemId: null });
    this.actionValor.set(null);
  }

  obterUnidadeItemModal(): string {
    const id = this.actionModal().itemId;
    if (!id) return '';
    const item = this.items().find(i => i.id === id);
    return item ? item.unidadeMedida : '';
  }

  confirmAction(): void {
    const modal = this.actionModal();
    const amount = this.actionAmount();
    const valor = this.actionValor() || 0; 
    
    if (!modal.itemId || amount === null || amount <= 0) return;

    const operacao$ = modal.type === 'add'
      ? this.estoqueService.adicionarQuantidade(modal.itemId, amount, valor)
      : this.estoqueService.removerQuantidade(modal.itemId, amount);

    operacao$.subscribe({
      next: () => {
        this.toast.showSuccess(`Quantidade ${modal.type === 'add' ? 'adicionada' : 'retirada'} com sucesso!`); // AVISO
        this.closeActionModal();
        this.carregarEstoque(); 
      },
      error: (erro) => {
        console.error(`Erro ao ${modal.type === 'add' ? 'adicionar' : 'remover'} estoque:`, erro);
        this.toast.showError('Falha ao processar a movimentação no estoque.'); // AVISO
      }
    });
  }

  openDeleteModal(): void {
    this.isDeleteModalOpen.set(true);
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen.set(false);
  }

  confirmDelete(): void {
    const item = this.selectedItem();
    if (!item) return;

    this.estoqueService.deletarItem(item.id).subscribe({
      next: () => {
        this.toast.showSuccess(`${item.nome} foi excluído permanentemente.`); // AVISO
        this.closeDeleteModal();
        this.changeView('list');
        this.carregarEstoque();
      },
      error: (erro) => {
        console.error('Erro ao deletar o item:', erro);
        this.toast.showError('Não foi possível excluir o item selecionado.'); // AVISO
      }
    });
  }
}