import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../../environments/environment';
import { TokenService } from '../../../../../core/auth/token.service';
import { authInterceptor } from '../../../../../core/interceptor/auth.interceptor';
import { errorInterceptor } from '../../../../../core/interceptor/error.interceptor';
import { ToastService } from '../../../../../core/services/toast.service';
import Configuracoes from '../../../../../pages/configuracoes/configuracoes';

describe('Configurações — Cadastro de funcionário', () => {
  let fixture: ComponentFixture<Configuracoes>;
  let http: HttpTestingController;
  const cadastroUrl = `${environment.apiUrl}/auth/cadastrofunc`;
  const listaUrl = `${environment.apiUrl}/user/funcionarios`;
  const toast = { showSuccess: vi.fn(), showError: vi.fn() };
  const tokens = { hasTokens: () => false, getAccessToken: () => 'token-do-dono', salvarTokens: vi.fn() };
  const showModalOriginal = Object.getOwnPropertyDescriptor(HTMLDialogElement.prototype, 'showModal');
  const closeOriginal = Object.getOwnPropertyDescriptor(HTMLDialogElement.prototype, 'close');

  beforeAll(() => {
    // O jsdom não implementa os métodos nativos de abertura e fechamento de dialog.
    Object.defineProperties(HTMLDialogElement.prototype, {
      showModal: { configurable: true, value: function (this: HTMLDialogElement) { this.open = true; } },
      close: { configurable: true, value: function (this: HTMLDialogElement) { this.open = false; } }
    });
  });

  afterAll(() => {
    for (const [nome, descriptor] of [['showModal', showModalOriginal], ['close', closeOriginal]] as const) {
      if (descriptor) Object.defineProperty(HTMLDialogElement.prototype, nome, descriptor);
      else Reflect.deleteProperty(HTMLDialogElement.prototype, nome);
    }
  });

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [Configuracoes],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
        provideHttpClientTesting(),
        { provide: TokenService, useValue: tokens },
        { provide: ToastService, useValue: toast }
      ]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Configuracoes);
    fixture.componentInstance.abaAtiva.set('usuarios');
    fixture.detectChanges();
    http.expectOne(listaUrl).flush([]);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  function abrir(): void {
    fixture.nativeElement.querySelector('.btn-new-user').click();
    fixture.detectChanges();
  }

  function preencher(nome = '  Ana Souza  ', email = '  ana@example.com  ', senha = ' senha123 '): void {
    for (const [id, valor] of [['cadastro-nome', nome], ['cadastro-email', email], ['cadastro-senha', senha]]) {
      const input: HTMLInputElement = fixture.nativeElement.querySelector(`#${id}`);
      input.value = valor;
      input.dispatchEvent(new Event('input'));
    }
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('#cadastro-funcao');
    select.value = 'Confeiteiro';
    select.dispatchEvent(new Event('change'));
  }

  function enviar(): void {
    fixture.nativeElement.querySelector('dialog form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }

  it('abre somente pelo botão e oferece funções compatíveis com adminRole false', () => {
    expect(fixture.nativeElement.querySelector('dialog')).toBeNull();
    abrir();
    expect(fixture.nativeElement.querySelector('dialog').open).toBe(true);
    const funcoes = Array.from(fixture.nativeElement.querySelectorAll('option'), (opcao: any) => opcao.value);
    expect(funcoes).toEqual(['Dono', 'Confeiteiro', 'Auxiliar', 'Decorador', 'Atendente', 'Caixa']);
    expect(fixture.nativeElement.querySelector('#cadastro-funcao').value).toBe('Atendente');
    http.expectNone(cadastroUrl);
  });

  it('valida nome, e-mail e senha antes de enviar', () => {
    abrir();
    preencher('   ', 'invalido', '12345');
    enviar();
    expect(fixture.nativeElement.textContent).toContain('Informe o nome do usuário.');
    expect(fixture.nativeElement.textContent).toContain('Informe um e-mail válido.');
    expect(fixture.nativeElement.textContent).toContain('A senha deve conter pelo menos 6 caracteres.');
    http.expectNone(cadastroUrl);
  });

  it('mostra a senha e descarta os dados ao cancelar, inclusive ao reabrir', () => {
    abrir();
    preencher();
    fixture.nativeElement.querySelector('[aria-label="Mostrar senha"]').click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#cadastro-senha').type).toBe('text');
    fixture.nativeElement.querySelector('.btn-cancel').click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog')).toBeNull();
    abrir();
    expect(fixture.nativeElement.querySelector('#cadastro-nome').value).toBe('');
    expect(fixture.nativeElement.querySelector('#cadastro-senha').value).toBe('');
    expect(fixture.nativeElement.querySelector('#cadastro-senha').type).toBe('password');
    http.expectNone(cadastroUrl);
  });

  it('fecha por Escape e pelo botão de fechar sem enviar requisição', () => {
    abrir();
    fixture.nativeElement.querySelector('dialog').dispatchEvent(new Event('cancel', { cancelable: true }));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog')).toBeNull();
    abrir();
    fixture.nativeElement.querySelector('.btn-close').click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog')).toBeNull();
    http.expectNone(cadastroUrl);
  });

  it('envia o contrato autenticado, impede duplicação e recarrega a lista sem trocar a sessão', () => {
    abrir();
    preencher();
    enviar();
    enviar();
    const req = http.expectOne({ method: 'POST', url: cadastroUrl });
    expect(req.request.body).toEqual({ adminRole: false, nome: 'Ana Souza', email: 'ana@example.com', senha: ' senha123 ', role: 'Confeiteiro' });
    expect(req.request.headers.get('Authorization')).toBe('Bearer token-do-dono');
    expect(fixture.nativeElement.querySelector('dialog fieldset').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('.btn-close').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(true);
    fixture.nativeElement.querySelector('dialog').dispatchEvent(new Event('cancel', { cancelable: true }));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog').open).toBe(true);
    req.flush(null);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog')).toBeNull();
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(false);
    expect(toast.showSuccess).toHaveBeenCalledWith('Usuário cadastrado com sucesso!');
    expect(tokens.salvarTokens).not.toHaveBeenCalled();
    http.expectOne(listaUrl).flush([{ id: 3, nome: 'Ana Souza', email: 'ana@example.com', role: 'Confeiteiro', dataInicio: '2026-09-23T00:00:00' }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('tbody').textContent).toContain('Ana Souza');
  });

  it('preserva o formulário após conflito de e-mail e permite corrigir e reenviar', () => {
    abrir();
    preencher();
    enviar();
    http.expectOne(cadastroUrl).flush({ error: 'Usuário com mesmo email já existe' }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog [role="alert"]').textContent).toContain('Usuário com mesmo email já existe');
    expect(fixture.nativeElement.querySelector('#cadastro-senha').value).toBe(' senha123 ');
    expect(fixture.nativeElement.querySelector('dialog fieldset').disabled).toBe(false);
    expect(toast.showSuccess).not.toHaveBeenCalled();
    http.expectNone(listaUrl);
    preencher('Ana Souza', 'ana.nova@example.com', '123456');
    enviar();
    const req = http.expectOne(cadastroUrl);
    expect(req.request.body.email).toBe('ana.nova@example.com');
    req.flush(null);
    http.expectOne(listaUrl).flush([]);
  });

  it('fecha o cadastro após sucesso mesmo se a atualização da lista falhar', () => {
    abrir();
    preencher();
    enviar();
    http.expectOne(cadastroUrl).flush(null);
    http.expectOne(listaUrl).flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Não foi possível carregar os usuários da empresa.');
    fixture.nativeElement.querySelector('.btn-retry').click();
    http.expectOne(listaUrl).flush([]);
    http.expectNone(cadastroUrl);
  });
});
