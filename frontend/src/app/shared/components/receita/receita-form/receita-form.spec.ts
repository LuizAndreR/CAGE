import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { EstoqueItem } from '../../../../core/models/estoque.interface';
import { ReceitaForm } from './receita-form';
import { ReceitaService } from '../../../../core/services/receita.service';
import { EstoqueService } from '../../../../core/services/estoque.service';

describe('ReceitaForm — alternativas', () => {
  const itens = [1, 2, 3].map(id => ({ id, nome: 'Farinha', marca: `Marca ${id}`, unidadeMedida: 'KG' }));
  const receita = { nome: 'Bolo', modoPreparo: 'Assar', precoVenda: 30, custoExtra: 0, percentualMargemLucro: 0,
    ingredientes: [{ itemId: 1, nome: 'Farinha', quantidade: 1, unidadeMedida: 'KG', alternativasIds: [3, 2] }] };
  const atualizar = vi.fn(() => of(null));

  beforeEach(async () => {
    atualizar.mockClear();
    await TestBed.configureTestingModule({
      imports: [ReceitaForm],
      providers: [
        { provide: EstoqueService, useValue: { getItens: () => of(itens) } },
        { provide: ReceitaService, useValue: { getReceitaById: () => of(receita), atualizarReceita: atualizar } }
      ]
    }).compileComponents();
  });

  it('carrega alternativas na ordem salva, mostra marcas e envia a nova ordem na edição', () => {
    const fixture = TestBed.createComponent(ReceitaForm);
    fixture.componentRef.setInput('receitaId', 4);
    fixture.detectChanges();
    const form = fixture.componentInstance;
    expect(form.ingredientesArray.at(0).value.alternativasIds).toEqual([3, 2]);
    expect(fixture.nativeElement.textContent).toContain('Marca 3');
    expect(form.alternativasDisponiveis(0)).toEqual([]);
    form.moverAlternativa(0, 1, -1);
    form.salvar();
    expect(atualizar).toHaveBeenCalledWith(4, expect.objectContaining({
      ingredientes: [expect.objectContaining({ itemId: 1, alternativasIds: [2, 3] })]
    }));
  });

  it('permite remover alternativas, mas não repetir nem selecionar o principal', () => {
    const fixture = TestBed.createComponent(ReceitaForm);
    fixture.componentRef.setInput('receitaId', 4);
    fixture.detectChanges();
    const form = fixture.componentInstance;
    const select = document.createElement('select');
    for (const id of [1, 2, 3]) select.add(new Option(`${id}`, `${id}`));
    select.value = '1'; form.adicionarAlternativa(0, select);
    select.value = '2'; form.adicionarAlternativa(0, select);
    expect(form.ingredientesArray.at(0).value.alternativasIds).toEqual([3, 2]);
    form.removerAlternativa(0, 0);
    expect(form.alternativasDisponiveis(0).map(i => i.id)).toEqual([3]);
    select.value = '3'; form.adicionarAlternativa(0, select);
    expect(form.ingredientesArray.at(0).value.alternativasIds).toEqual([2, 3]);
  });
  it.each([
    ['G', ['G', 'KG', 'CUP', 'TSP', 'TBS']],
    ['KG', ['G', 'KG', 'CUP', 'TSP', 'TBS']],
    ['ML', ['ML', 'L', 'CUP', 'TSP', 'TBS']],
    ['L', ['ML', 'L', 'CUP', 'TSP', 'TBS']],
    ['CUP', ['G', 'KG', 'ML', 'L', 'CUP', 'TSP', 'TBS']],
    ['TSP', ['G', 'KG', 'ML', 'L', 'CUP', 'TSP', 'TBS']],
    ['TBS', ['G', 'KG', 'ML', 'L', 'CUP', 'TSP', 'TBS']],
    ['UN', ['UN']]
  ])('filtra alternativas para o principal em %s, mesmo com receita em xícaras', (unidade, esperadas) => {
    const fixture = TestBed.createComponent(ReceitaForm);
    fixture.componentRef.setInput('receitaId', 4);
    fixture.detectChanges();
    const form = fixture.componentInstance;
    const unidades = ['G', 'KG', 'ML', 'L', 'CUP', 'TSP', 'TBS', 'UN'];
    const estoque = [
      { ...itens[0], unidadeMedida: unidade },
      ...unidades.map((u, i) => ({ id: i + 2, nome: `Item ${u}`, marca: 'B', unidadeMedida: u }))
    ] as EstoqueItem[];
    form.estoqueItens.set(estoque);
    form.ingredientesArray.at(0).patchValue({ alternativasIds: [], unidadeMedida: 'CUP' });
    fixture.detectChanges();
    expect(form.alternativasDisponiveis(0).map(item => item.unidadeMedida)).toEqual(esperadas);
    const select = fixture.nativeElement.querySelector('.alternativas-editor select') as HTMLSelectElement;
    expect(select.options.length).toBe(esperadas.length + 1);
    if (unidade === 'KG') {
      const forjado = document.createElement('select');
      forjado.add(new Option('Leite', '5')); // L
      forjado.value = '5';
      form.adicionarAlternativa(0, forjado);
      expect(form.ingredientesArray.at(0).value.alternativasIds).toEqual([]);
    }
  });

});
