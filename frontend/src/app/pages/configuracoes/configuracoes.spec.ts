import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { errorInterceptor } from '../../core/interceptor/error.interceptor';
import { ToastService } from '../../core/services/toast.service';
import { ConfigPerfil } from '../../shared/components/config/config-perfil/config-perfil';
import { ConfigEmpresa } from '../../shared/components/config/config-empresa/config-empresa';
import Configuracoes from './configuracoes';

describe('Configurações — Perfil', () => {
  let fixture: ComponentFixture<Configuracoes>;
  let http: HttpTestingController;
  let perfil: ConfigPerfil;
  const url = `${environment.apiUrl}/user/me`;
  const usuario = { id: 1, nome: 'Ana Silva', email: 'ana@example.com', role: 'Dono', dataInicio: '2026-01-01' };
  const toast = { showSuccess: vi.fn(), showError: vi.fn() };

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [Configuracoes],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: ToastService, useValue: toast }
      ]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Configuracoes);
    fixture.detectChanges();
    perfil = fixture.debugElement.query(By.directive(ConfigPerfil)).componentInstance;
  });

  afterEach(() => http.verify());

  function carregarPerfil(): void {
    http.expectOne({ method: 'GET', url }).flush(usuario);
    fixture.detectChanges();
  }

  function preencher(id: string, valor: string): void {
    const input: HTMLInputElement = fixture.nativeElement.querySelector(`#${id}`);
    input.value = valor;
    input.dispatchEvent(new Event('input'));
  }

  function enviarFormulario(indice: number): void {
    fixture.nativeElement.querySelectorAll('form')[indice].dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }

  it('abre em Perfil, carrega os dados reais e não apresenta telefone', () => {
    expect(fixture.componentInstance.abaAtiva()).toBe('perfil');
    expect(fixture.nativeElement.textContent).toContain('Carregando seu perfil');
    carregarPerfil();
    expect(fixture.nativeElement.querySelector('#perfil-nome').value).toBe(usuario.nome);
    expect(fixture.nativeElement.querySelector('#perfil-email').value).toBe(usuario.email);
    expect(fixture.nativeElement.querySelector('input[type="tel"]')).toBeNull();
  });

  it('troca os componentes pela navegação e recarrega o perfil ao retornar', () => {
    carregarPerfil();
    const botoes = fixture.nativeElement.querySelectorAll('.config-nav button');
    botoes[1].click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('app-config-usuarios')).not.toBeNull();
    http.expectOne({ method: 'GET', url: `${environment.apiUrl}/user/funcionarios` }).flush([]);
    expect(fixture.nativeElement.querySelector('app-config-perfil')).toBeNull();
    botoes[2].click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('app-config-empresa')).not.toBeNull();
    botoes[0].click();
    fixture.detectChanges();
    carregarPerfil();
    expect(fixture.nativeElement.querySelector('app-config-perfil')).not.toBeNull();
  });

  it('permite tentar novamente quando o carregamento falha', () => {
    http.expectOne(url).flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Não foi possível carregar seu perfil.');
    perfil.salvarPerfil();
    http.expectNone({ method: 'PUT', url });
    fixture.nativeElement.querySelector('.btn-retry').click();
    carregarPerfil();
    expect(perfil.perfilCarregado()).toBe(true);
  });

  it('bloqueia nome vazio e e-mail inválido sem chamar a API', () => {
    carregarPerfil();
    preencher('perfil-nome', '   ');
    preencher('perfil-email', 'invalido');
    enviarFormulario(0);
    http.expectNone({ method: 'PUT', url });
    expect(fixture.nativeElement.textContent).toContain('Informe seu nome.');
    expect(fixture.nativeElement.textContent).toContain('Informe um e-mail válido.');
  });

  it('envia somente nome e e-mail e impede envio duplicado durante a gravação', () => {
    carregarPerfil();
    preencher('perfil-nome', '  Ana Souza  ');
    preencher('perfil-email', '  ana.souza@example.com  ');
    enviarFormulario(0);
    perfil.salvarPerfil();
    const req = http.expectOne({ method: 'PUT', url });
    expect(req.request.body).toEqual({ nome: 'Ana Souza', email: 'ana.souza@example.com' });
    expect(fixture.nativeElement.querySelector('fieldset').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(true);
    req.flush(null);
    fixture.detectChanges();
    expect(toast.showSuccess).toHaveBeenCalledWith('Perfil atualizado com sucesso!');
    expect(perfil.salvandoPerfil()).toBe(false);
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(false);
  });

  it('exibe o conflito de e-mail e preserva a edição para nova tentativa', () => {
    carregarPerfil();
    preencher('perfil-email', 'existente@example.com');
    enviarFormulario(0);
    http.expectOne({ method: 'PUT', url }).flush({ error: 'Email ja esta em uso por outro usuario' }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();
    expect(toast.showError).toHaveBeenCalledWith('Email ja esta em uso por outro usuario');
    expect(toast.showSuccess).not.toHaveBeenCalled();
    expect(perfil.perfilForm.controls.email.value).toBe('existente@example.com');
    expect(perfil.salvandoPerfil()).toBe(false);
    preencher('perfil-email', 'novo@example.com');
    enviarFormulario(0);
    http.expectOne({ method: 'PUT', url }).flush(null);
    expect(toast.showSuccess).toHaveBeenCalled();
  });

  it('bloqueia senhas curtas e confirmação divergente', () => {
    carregarPerfil();
    preencher('senha-atual', '12345');
    preencher('nova-senha', 'abcde');
    preencher('confirmar-nova-senha', 'abcde');
    enviarFormulario(1);
    http.expectNone(`${url}/senha`);
    expect(fixture.nativeElement.textContent).toContain('A nova senha deve conter pelo menos 6 caracteres.');
    preencher('senha-atual', '123456');
    preencher('nova-senha', 'abcdef');
    enviarFormulario(1);
    http.expectNone(`${url}/senha`);
    expect(fixture.nativeElement.textContent).toContain('A confirmação deve ser igual à nova senha.');
  });

  it('envia somente as duas senhas sem alterar espaços e limpa os campos após sucesso', () => {
    carregarPerfil();
    preencher('senha-atual', ' atual123 ');
    preencher('nova-senha', ' nova123 ');
    preencher('confirmar-nova-senha', ' nova123 ');
    fixture.nativeElement.querySelector('[aria-label="Mostrar nova senha"]').click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#nova-senha').type).toBe('text');
    enviarFormulario(1);
    perfil.alterarSenha();
    const req = http.expectOne({ method: 'PATCH', url: `${url}/senha` });
    expect(req.request.body).toEqual({ senhaAtual: ' atual123 ', novaSenha: ' nova123 ' });
    expect(perfil.salvandoSenha()).toBe(true);
    req.flush(null);
    fixture.detectChanges();
    expect(perfil.senhaForm.getRawValue()).toEqual({ senhaAtual: '', novaSenha: '', confirmarNovaSenha: '' });
    expect(fixture.nativeElement.querySelector('#nova-senha').type).toBe('password');
    expect(perfil.senhaForm.pristine).toBe(true);
    expect(toast.showSuccess).toHaveBeenCalledWith('Senha atualizada com sucesso!');
  });

  it('aceita senhas de seis caracteres, mostra erro da API e permite corrigir a senha atual', () => {
    carregarPerfil();
    preencher('senha-atual', 'errada');
    preencher('nova-senha', 'nova12');
    preencher('confirmar-nova-senha', 'nova12');
    enviarFormulario(1);
    http.expectOne(`${url}/senha`).flush({ errors: ['Senha atual inválida'] }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(toast.showError).toHaveBeenCalledWith('Senha atual inválida');
    expect(perfil.senhaForm.controls.novaSenha.value).toBe('nova12');
    expect(perfil.salvandoSenha()).toBe(false);
    expect(toast.showSuccess).not.toHaveBeenCalled();
    preencher('senha-atual', 'certa1');
    enviarFormulario(1);
    http.expectOne(`${url}/senha`).flush(null);
    expect(toast.showSuccess).toHaveBeenCalledWith('Senha atualizada com sucesso!');
  });

  it('mantém a navegação bloqueada até as duas gravações independentes terminarem', () => {
    carregarPerfil();
    preencher('perfil-nome', 'Ana Souza');
    enviarFormulario(0);
    preencher('senha-atual', 'atual1');
    preencher('nova-senha', 'nova12');
    preencher('confirmar-nova-senha', 'nova12');
    enviarFormulario(1);
    http.expectOne({ method: 'PUT', url }).flush(null);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(true);
    http.expectOne(`${url}/senha`).flush(null);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(false);
  });
});

describe('Configurações — Empresa', () => {
  let fixture: ComponentFixture<Configuracoes>;
  let http: HttpTestingController;
  let empresa: ConfigEmpresa;
  const url = `${environment.apiUrl}/empresa/me`;
  const toast = { showSuccess: vi.fn(), showError: vi.fn() };

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [Configuracoes],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: ToastService, useValue: toast }
      ]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Configuracoes);
    fixture.componentInstance.abaAtiva.set('empresa');
    fixture.detectChanges();
    empresa = fixture.debugElement.query(By.directive(ConfigEmpresa)).componentInstance;
  });

  afterEach(() => http.verify());

  function preencher(nome: string, endereco: string): void {
    for (const [id, valor] of [['empresa-nome', nome], ['empresa-endereco', endereco]]) {
      const input: HTMLInputElement = fixture.nativeElement.querySelector(`#${id}`);
      input.value = valor;
      input.dispatchEvent(new Event('input'));
    }
  }

  function enviar(): void {
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }

  it('exibe somente nome e endereço e valida campos vazios', () => {
    expect(fixture.nativeElement.textContent).not.toContain('CNPJ');
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(2);
    preencher('   ', '   ');
    enviar();
    expect(fixture.nativeElement.textContent).toContain('Informe o nome da empresa.');
    expect(fixture.nativeElement.textContent).toContain('Informe o endereço da empresa.');
    http.expectNone({ method: 'PUT', url });
  });

  it('respeita os limites de 150 caracteres para nome e 250 para endereço', () => {
    preencher('a'.repeat(151), 'b'.repeat(251));
    enviar();
    expect(fixture.nativeElement.textContent).toContain('O nome deve ter no máximo 150 caracteres.');
    expect(fixture.nativeElement.textContent).toContain('O endereço deve ter no máximo 250 caracteres.');
    http.expectNone({ method: 'PUT', url });
  });

  it('envia o contrato exato, bloqueia reenvio e navegação e confirma sucesso', () => {
    preencher('  Confeitaria da Ana  ', '  Rua das Flores, 123  ');
    enviar();
    empresa.salvarEmpresa();
    const req = http.expectOne({ method: 'PUT', url });
    expect(req.request.body).toEqual({ nome: 'Confeitaria da Ana', endereco: 'Rua das Flores, 123' });
    expect(fixture.nativeElement.querySelector('fieldset').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(true);
    req.flush(null);
    fixture.detectChanges();
    expect(toast.showSuccess).toHaveBeenCalledWith('Dados da empresa atualizados com sucesso!');
    expect(empresa.empresaForm.pristine).toBe(true);
    expect(fixture.nativeElement.querySelector('.config-nav button').disabled).toBe(false);
  });

  it('preserva a edição após erro e permite uma nova tentativa', () => {
    preencher('Confeitaria da Ana', 'Rua das Flores, 123');
    enviar();
    http.expectOne({ method: 'PUT', url }).flush({ errors: ['Dados inválidos.'] }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(toast.showError).toHaveBeenCalledWith('Dados inválidos.');
    expect(toast.showSuccess).not.toHaveBeenCalled();
    expect(empresa.empresaForm.getRawValue()).toEqual({ nome: 'Confeitaria da Ana', endereco: 'Rua das Flores, 123' });
    expect(empresa.salvando()).toBe(false);
    expect(fixture.componentInstance.salvando()).toBe(false);
    enviar();
    http.expectOne({ method: 'PUT', url }).flush(null);
    expect(toast.showSuccess).toHaveBeenCalledOnce();
  });
});
