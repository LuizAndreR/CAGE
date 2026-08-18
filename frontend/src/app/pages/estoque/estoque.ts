import { Component, computed, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstoqueItem } from '../../core/models/estoque.interface';
import { EstoqueService } from '../../core/service/estoque.service';
import { FormsModule } from '@angular/forms';

import { EstoqueList } from '../../shared/components/estoque-list/estoque-list';
import { EstoqueDetail } from '../../shared/components/estoque-detail/estoque-detail';
import { EstoqueForm } from '../../shared/components/estoque-form/estoque-form';

@Component({
  selector: 'app-estoque',
  standalone: true, 
  imports: [CommonModule, FormsModule, EstoqueList, EstoqueDetail, EstoqueForm],
  templateUrl: './estoque.html',
  styleUrl: './estoque.css'
})  
export default class Estoque implements OnInit {
  private estoqueService = inject(EstoqueService);

  view = signal<'list' | 'detail' | 'form'>('list');
  activeTab = signal<'todos' | 'baixo'>('todos');
  items = signal<EstoqueItem[]>([]);
  selectedId = signal<number | null>(null);
  search = signal<string>('');
  
  itemEditando = signal<EstoqueItem | null>(null);

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
    const requisicao$ = this.activeTab() === 'todos' 
      ? this.estoqueService.getItens() 
      : this.estoqueService.getItensBaixoEstoque();

    requisicao$.subscribe({
      next: (dados) => this.items.set(dados),
      error: (erro) => console.error('Erro na API:', erro)
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
    const operacao$ = payload.id 
      ? this.estoqueService.atualizarItem(payload.id, payload)
      : this.estoqueService.criarItem((delete payload.id, payload));

    operacao$.subscribe({
      next: () => {
        this.changeView('list');
        this.carregarEstoque();
      },
      error: (erro) => console.error('Erro ao salvar:', erro)
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
        this.closeActionModal();
        this.carregarEstoque(); 
      },
      error: (erro) => console.error(`Erro ao ${modal.type === 'add' ? 'adicionar' : 'remover'} estoque:`, erro)
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
        console.log(`Item ${item.nome} deletado com sucesso.`);
        this.closeDeleteModal();
        this.view.set('list');
        this.carregarEstoque();
      },
      error: (erro) => console.error('Erro ao deletar o item:', erro)
    });
  }
}