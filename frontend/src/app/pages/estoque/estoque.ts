import { Component, computed, signal, inject, OnInit } from '@angular/core';
import { EstoqueItem } from '../../core/models/estoque.interface';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EstoqueService } from '../../core/service/estoque.service';

@Component({
  selector: 'app-estoque',
  standalone: true, 
  imports: [CommonModule, FormsModule],
  templateUrl: './estoque.html',
  styleUrl: './estoque.css',
})  
export default class Estoque implements OnInit {
  
  private estoqueService = inject(EstoqueService);

  view = signal<'list' | 'detail' | 'form'>('list');
  activeTab = signal<'todos' | 'baixo'>('todos');
  
  items = signal<EstoqueItem[]>([]);
  selectedId = signal<number | null>(null);
  search = signal<string>('');

  actionModal = signal<{ isOpen: boolean; type: 'add' | 'remove'; itemId: number | null }>({
    isOpen: false, type: 'add', itemId: null
  });
  actionAmount = signal<number | null>(null);
  formData = signal<Partial<EstoqueItem>>({ unidade: 'G' });

  ngOnInit(): void {
    this.carregarEstoque();
  }

  mudarAba(aba: 'todos' | 'baixo'): void {
    this.activeTab.set(aba);
    this.carregarEstoque();
  }

  carregarEstoque(): void {
    const requisicao$ = this.activeTab() === 'todos' 
      ? this.estoqueService.getItens() 
      : this.estoqueService.getItensBaixoEstoque();

    requisicao$.subscribe({
      next: (dadosDaApi) => {
        this.items.set(dadosDaApi);
      },
      error: (erro) => {
        console.error('Falha ao comunicar com a API:', erro);
      }
    });
  }

  selectedItem = computed(() => 
    this.items().find(i => i.id === this.selectedId()) || null
  );

  
  filteredItems = computed(() => {
    const termo = this.search().toLowerCase();
    
    return this.items().filter(item => {
      const marcaTexto = item.marca?.toLowerCase() || '';
      return item.nome.toLowerCase().includes(termo) || marcaTexto.includes(termo);
    });
  });


  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  openActionModal(type: 'add' | 'remove', id: number): void {
    this.actionModal.set({ isOpen: true, type, itemId: id });
    this.actionAmount.set(null);
  }

  closeActionModal(): void {
    this.actionModal.set({ isOpen: false, type: 'add', itemId: null });
  }

  confirmAction(): void {
    const modal = this.actionModal();
    const amount = this.actionAmount();
    
    if (!modal.itemId || amount === null || amount <= 0) return;

    this.items.update(currentItems => 
      currentItems.map(item => {
        if (item.id === modal.itemId) {
          return {
            ...item,
            quantidadeAtual: modal.type === 'add' 
              ? item.quantidadeAtual + amount 
              : Math.max(0, item.quantidadeAtual - amount)
          };
        }
        return item;
      })
    );
    this.closeActionModal();
  }

  changeView(newView: 'list' | 'detail' | 'form'): void {
    this.view.set(newView);
    if (newView === 'list') {
      this.selectedId.set(null);
      this.formData.set({ unidade: 'G' });
    }
  }

  editItem(item: EstoqueItem): void {
    this.formData.set({ ...item });
    this.changeView('form');
  }
}