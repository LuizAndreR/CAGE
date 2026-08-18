import { Component, computed, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EstoqueItem } from '../../core/models/estoque.interface';
import { EstoqueService } from '../../core/service/estoque.service';

@Component({
  selector: 'app-estoque',
  standalone: true, 
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './estoque.html',
  styleUrl: './estoque.css',
})  
export default class Estoque implements OnInit {
  
  private estoqueService = inject(EstoqueService);
  private fb = inject(FormBuilder);

  view = signal<'list' | 'detail' | 'form'>('list');
  activeTab = signal<'todos' | 'baixo'>('todos');
  
  items = signal<EstoqueItem[]>([]);
  selectedId = signal<number | null>(null);
  search = signal<string>('');

  actionModal = signal<{ isOpen: boolean; type: 'add' | 'remove'; itemId: number | null }>({
    isOpen: false, type: 'add', itemId: null
  });
  actionAmount = signal<number | null>(null);
  actionValor = signal<number | null>(null);

  estoqueForm = this.fb.group({
    id: [null as number | null], 
    nome: ['', Validators.required],
    marca: ['', Validators.required],
    quantidadeAtual: [0, [Validators.required, Validators.min(0)]],
    valor: [0, [Validators.required, Validators.min(0)]], // Corrigido
    unidadeMedida: ['G', Validators.required],
    quantidadeMinima: [0, Validators.required], // Corrigido
    pesoReferenciaEmGramas: [null as number | null],
    unidadeMedidaReferenciaVolume: [null as string | null] 
  });

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

  formatCurrency(value: number | undefined): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value || 0);
  }

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

    if (modal.type === 'add') {
      this.estoqueService.adicionarQuantidade(modal.itemId, amount, valor).subscribe({
        next: () => {
          this.closeActionModal();
          this.carregarEstoque(); 
        },
        error: (erro) => console.error('Erro ao adicionar estoque:', erro)
      });
    } else {
      this.estoqueService.removerQuantidade(modal.itemId, amount).subscribe({
        next: () => {
          this.closeActionModal();
          this.carregarEstoque(); 
        },
        error: (erro) => console.error('Erro ao remover estoque:', erro)
      });
    }
  }

  changeView(newView: 'list' | 'detail' | 'form'): void {
    this.view.set(newView);
    if (newView === 'list') {
      this.selectedId.set(null);
      this.estoqueForm.reset({ unidadeMedida: 'G' });
      this.estoqueForm.get('valor')?.enable();
    }
  }

  editItem(item: EstoqueItem): void {
    this.estoqueForm.patchValue({
      id: item.id,
      nome: item.nome,
      marca: item.marca,
      quantidadeAtual: item.quantidadeAtual,
      valor: item.valor, // Corrigido
      unidadeMedida: item.unidadeMedida,
      quantidadeMinima: item.quantidadeMinima, // Corrigido
      pesoReferenciaEmGramas: item.pesoReferenciaEmGramas,
      unidadeMedidaReferenciaVolume: item.unidadeMedidaReferenciaVolume 
    });
    
    // REGRA DE NEGÓCIO: API não aceita alterar o valor na edição
    this.estoqueForm.get('valor')?.disable();
    
    this.view.set('form');
  }

  // 3. CORREÇÃO NO SALVAMENTO: Uso do getRawValue()
  salvarItem(): void {
    if (this.estoqueForm.valid) {
      
      const formValues = { ...this.estoqueForm.getRawValue() };
      
      let peso = formValues.pesoReferenciaEmGramas;
      let volume = formValues.unidadeMedidaReferenciaVolume;

      if (!peso || peso <= 0 || !volume || volume.trim() === '') {
        formValues.pesoReferenciaEmGramas = null;
        formValues.unidadeMedidaReferenciaVolume = null;
      }

      if (formValues.id) {
        // MODO EDIÇÃO -> PUT (Envia com ID)
        this.estoqueService.atualizarItem(formValues.id, formValues).subscribe({
          next: () => {
            console.log('Item atualizado com sucesso!');
            this.estoqueForm.reset({ unidadeMedida: 'G' }); 
            this.estoqueForm.get('valor')?.enable();
            this.view.set('list'); 
            this.carregarEstoque(); 
          },
          error: (erro) => console.error('Erro ao atualizar item:', erro)
        });
      } else {
        const { id, ...payloadCriacao } = formValues;

        this.estoqueService.criarItem(payloadCriacao).subscribe({
          next: () => {
            console.log('Novo item criado com sucesso!');
            this.estoqueForm.reset({ unidadeMedida: 'G' }); 
            this.estoqueForm.get('valor')?.enable();
            this.view.set('list'); 
            this.carregarEstoque(); 
          },
          error: (erro) => console.error('Erro ao salvar item:', erro)
        });
      }
    } else {
      this.estoqueForm.markAllAsTouched();
    }
  }

  // 4. GARANTIA: Liberar o campo caso a usuária cancele a edição
  cancelarFormulario(): void {
    this.estoqueForm.reset({ unidadeMedida: 'G' });
    this.estoqueForm.get('valor')?.enable();
    this.view.set('list');
  }
  
  

  isDeleteModalOpen = signal<boolean>(false);

  // 2. Métodos para abrir e fechar o modal
  openDeleteModal(): void {
    this.isDeleteModalOpen.set(true);
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen.set(false);
  }

  // 3. Método que integra com a API
  confirmDelete(): void {
    const item = this.selectedItem();
    if (!item) return;

    this.estoqueService.deletarItem(item.id).subscribe({
      next: () => {
        console.log(`Item ${item.nome} deletado com sucesso.`);
        this.closeDeleteModal();
        this.view.set('list'); // Volta para a tabela principal
        this.carregarEstoque(); // Atualiza a lista com o banco de dados
      },
      error: (erro) => {
        console.error('Erro ao deletar o item:', erro);
      }
    });
  }
}