import { Component, computed, signal } from '@angular/core';
import { EstoqueItem } from '../../core/models/estoque.interface';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-estoque',
  imports: [CommonModule, FormsModule],
  templateUrl: './estoque.html',
  styleUrl: './estoque.css',
})  
export default class Estoque {
  view = signal<'list' | 'detail' | 'form'>('list');
  activeTab = signal<'todos' | 'baixo'>('todos');
  
  items = signal<EstoqueItem[]>([
    { id: 1, nome: "Farinha de Trigo", marca: "Dona Benta", quantidadeAtual: 15, unidade: 'KG', valorMedio: 4.50, quantidadeMinima: 10 },
    { id: 2, nome: "Chocolate em Pó 50%", marca: "Nestlé", quantidadeAtual: 2.5, unidade: 'KG', valorMedio: 28.00, quantidadeMinima: 5 }
  ]);
  
  selectedId = signal<number | null>(null);
  search = signal<string>('');

  actionModal = signal<{ isOpen: boolean; type: 'add' | 'remove'; itemId: number | null }>({
    isOpen: false, type: 'add', itemId: null
  });
  actionAmount = signal<number | null>(null);

  // Estado do Formulário
  formData = signal<Partial<EstoqueItem>>({ unidade: 'G' });

  selectedItem = computed(() => 
    this.items().find(i => i.id === this.selectedId()) || null
  );

  filteredItems = computed(() => {
    const termo = this.search().toLowerCase();
    const tab = this.activeTab();
    
    return this.items().filter(item => {
      const matchesSearch = item.nome.toLowerCase().includes(termo) || item.marca.toLowerCase().includes(termo);
      const matchesTab = tab === 'todos' ? true : item.quantidadeAtual <= item.quantidadeMinima;
      return matchesSearch && matchesTab;
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