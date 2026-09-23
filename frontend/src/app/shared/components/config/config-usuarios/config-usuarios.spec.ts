import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { errorInterceptor } from '../../../../core/interceptor/error.interceptor';
import { ToastService } from '../../../../core/services/toast.service';
import Configuracoes from '../../../../pages/configuracoes/configuracoes';

describe('Configurações — Listagem de usuários', () => {
  let fixture: ComponentFixture<Configuracoes>;
  let http: HttpTestingController;
  const url = `${environment.apiUrl}/user/funcionarios`;
  const usuario = { id: 2, nome: 'Teste 02', email: 'teste02@gmail.com', role: 'Dono', dataInicio: '0001-01-01T00:00:00' };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Configuracoes],
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: ToastService, useValue: { showSuccess: vi.fn(), showError: vi.fn() } }
      ]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Configuracoes);
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/user/me`).flush(usuario);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  function abrirUsuarios(): void {
    fixture.nativeElement.querySelectorAll('.config-nav button')[1].click();
    fixture.detectChanges();
  }

  it('busca os funcionários apenas ao entrar na aba e não abre formulário ou modal', () => {
    http.expectNone(url);
    abrirUsuarios();
    expect(fixture.nativeElement.textContent).toContain('Carregando usuários da empresa...');
    expect(fixture.nativeElement.textContent).not.toContain('0 colaboradores cadastrados');
    const req = http.expectOne({ method: 'GET', url });
    expect(req.request.body).toBeNull();
    expect(req.request.params.keys()).toEqual([]);
    req.flush([usuario]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(fixture.nativeElement.querySelector('dialog, [role="dialog"], app-config-usuario-form')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('1 colaborador cadastrado');
    expect(fixture.nativeElement.querySelector('.btn-new-user').textContent).toContain('Novo usuário');
  });

  it('mostra os dados da API, iniciais e datas sem deslocar o dia pelo fuso', () => {
    abrirUsuarios();
    http.expectOne(url).flush([
      usuario,
      { id: 3, nome: '  Ana   Souza  ', email: 'ana@example.com', role: 'Confeiteiro', dataInicio: '2026-09-23T00:00:00Z' }
    ]);
    fixture.detectChanges();
    const linhas = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(linhas.length).toBe(2);
    expect(linhas[0].textContent).toContain('Teste 02');
    expect(linhas[0].textContent).toContain('teste02@gmail.com');
    expect(linhas[0].textContent).toContain('Dono');
    expect(linhas[0].textContent).toContain('Não informada');
    expect(linhas[0].textContent).not.toContain('0001');
    expect(linhas[1].querySelector('.user-avatar').textContent).toBe('AS');
    expect(linhas[1].querySelector('.role-badge').textContent).toBe('Confeiteiro');
    expect(linhas[1].textContent).toContain('23/09/2026');
    expect(fixture.nativeElement.textContent).toContain('2 colaboradores cadastrados');
  });

  it('apresenta lista vazia somente após uma resposta bem-sucedida sem usuários', () => {
    abrirUsuarios();
    http.expectOne(url).flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('0 colaboradores cadastrados');
    expect(fixture.nativeElement.textContent).toContain('Nenhum usuário cadastrado nesta empresa.');
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });

  it('diferencia falha de lista vazia e permite tentar novamente', () => {
    abrirUsuarios();
    http.expectOne(url).flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Não foi possível carregar os usuários da empresa.');
    expect(fixture.nativeElement.textContent).not.toContain('Nenhum usuário cadastrado');
    expect(fixture.nativeElement.textContent).not.toContain('0 colaboradores cadastrados');
    fixture.nativeElement.querySelector('.btn-retry').click();
    fixture.detectChanges();
    http.expectOne(url).flush([usuario]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain(usuario.email);
  });

  it('informa quando a API nega permissão de acesso', () => {
    abrirUsuarios();
    http.expectOne(url).flush(null, { status: 403, statusText: 'Forbidden' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Você não tem permissão para consultar os usuários desta empresa.');
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
  });

  it('cancela a consulta ao sair e faz uma nova requisição ao retornar à aba', () => {
    abrirUsuarios();
    const req = http.expectOne(url);
    fixture.nativeElement.querySelectorAll('.config-nav button')[2].click();
    fixture.detectChanges();
    expect(req.cancelled).toBe(true);
    abrirUsuarios();
    http.expectOne(url).flush([usuario]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain(usuario.nome);
  });
});
