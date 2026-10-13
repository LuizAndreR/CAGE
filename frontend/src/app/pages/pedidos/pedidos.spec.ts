import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import Pedidos from './pedidos';
import { PedidoDetail } from '../../shared/components/pedidos/pedido-detail/pedido-detail';
import { PedidosForm } from '../../shared/components/pedidos/pedidos-form/pedidos-form';
import { ReceitaService } from '../../core/services/receita.service';
import { PedidoResponse, AllPedidosResponse } from '../../core/models/pedido.interface';
import { environment } from '../../../environments/environment';

describe('Pedidos — criação e edição', () => {
  let fixture: ComponentFixture<Pedidos>;
  let http: HttpTestingController;
  const url = `${environment.apiUrl}/pedido`;
  const pedido: PedidoResponse = {
    id: 42, clienteNome: 'Ana', dataCriacao: '2026-01-01T12:00:00Z',
    valorTotal: 90, statusPedido: 'Pendente', pago: true,
    itens: [{ receitaId: 2, nomeReceita: 'Bolo', quantidade: 3, valorUnitario: 30, subTotal: 90 }]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Pedidos],
      providers: [provideHttpClient(), provideHttpClientTesting(), {
        provide: ReceitaService,
        useValue: { getReceitas: () => of([{ id: 2, nome: 'Bolo', precoVenda: 30, status: true }]) }
      }]
    }).compileComponents();
    fixture = TestBed.createComponent(Pedidos);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne({ method: 'GET', url }).flush([]);
    fixture.detectChanges();
  });

  afterEach(() => { http.verify(); vi.restoreAllMocks(); });

  function editar(): PedidosForm {
    fixture.componentInstance.onSelecionarPedido({ id: 42 } as AllPedidosResponse);
    http.expectOne({ method: 'GET', url: `${url}/42` }).flush(pedido);
    fixture.detectChanges();
    fixture.debugElement.query(By.directive(PedidoDetail)).componentInstance.editar.emit();
    fixture.detectChanges();
    return fixture.debugElement.query(By.directive(PedidosForm)).componentInstance;
  }

  function enviar(): void {
    fixture.nativeElement.querySelector('app-pedidos-form form')
      .dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }

  it('envia PUT para o pedido selecionado ao alterar a quantidade, sem criar outro', () => {
    const form = editar();
    expect(form.itens.at(0).value.quantidade).toBe(3);
    form.itens.at(0).get('quantidade')!.setValue(2);
    enviar();
    const request = http.expectOne({ method: 'PUT', url: `${url}/42` });
    expect(request.request.body).toEqual({
      clienteNome: 'Ana', telefoneCliente: null, descricao: null, dataEntrega: null,
      itens: [{ receitaId: 2, quantidade: 2 }]
    });
    http.expectNone(r => r.method === 'POST');
    request.flush(null);
    http.expectOne({ method: 'GET', url }).flush([]);
    expect(fixture.componentInstance.view()).toBe('list');
    expect(fixture.componentInstance.selectedId()).toBeNull();
  });

  it('envia POST ao criar novo pedido mesmo após selecionar um pedido existente', () => {
    editar();
    fixture.componentInstance.onNovoPedido();
    // Recria o formulário após sair da tela de edição.
    fixture.componentInstance.view.set('list');
    fixture.detectChanges();
    fixture.componentInstance.onNovoPedido();
    fixture.detectChanges();
    const form: PedidosForm = fixture.debugElement.query(By.directive(PedidosForm)).componentInstance;
    expect(form.pedidoForm.value.clienteNome).toBe('');
    form.pedidoForm.patchValue({ clienteNome: 'Novo cliente' });
    form.itens.at(0).patchValue({ receitaId: 2, quantidade: 1 });
    enviar();
    const request = http.expectOne({ method: 'POST', url });
    expect(request.request.body.clienteNome).toBe('Novo cliente');
    http.expectNone(r => r.method === 'PUT');
    request.flush(null);
    http.expectOne({ method: 'GET', url }).flush([]);
  });

  it('preserva a edição após falha do PUT e repete a atualização sem POST', () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    const form = editar();
    form.pedidoForm.patchValue({ clienteNome: 'Nome alterado' });
    enviar();
    http.expectOne({ method: 'PUT', url: `${url}/42` })
      .flush({ error: 'Falha' }, { status: 409, statusText: 'Conflict' });
    expect(fixture.componentInstance.selectedId()).toBe(42);
    expect(fixture.componentInstance.view()).toBe('create');
    expect(form.pedidoForm.value.clienteNome).toBe('Nome alterado');
    enviar();
    const retry = http.expectOne({ method: 'PUT', url: `${url}/42` });
    expect(retry.request.body.clienteNome).toBe('Nome alterado');
    http.expectNone(r => r.method === 'POST');
    retry.flush(null);
    http.expectOne({ method: 'GET', url }).flush([]);
  });

  it('cancelar a edição não envia criação nem atualização', () => {
    const form = editar();
    form.cancelar.emit();
    fixture.detectChanges();
    expect(fixture.componentInstance.view()).toBe('list');
    http.expectNone(r => r.method === 'POST' || r.method === 'PUT');
  });
});
