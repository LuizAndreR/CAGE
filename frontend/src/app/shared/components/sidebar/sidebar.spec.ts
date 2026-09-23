import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { UserService } from '../../../core/services/user.service';
import { Sidebar } from './sidebar';

describe('Sidebar', () => {
  it('atualiza nome e iniciais após salvar o perfil e mantém a função do usuário', async () => {
    await TestBed.configureTestingModule({
      imports: [Sidebar],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    }).compileComponents();

    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(Sidebar);
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/user/me`).flush({
      id: 1, nome: 'Ana Silva', email: 'ana@example.com', role: 'Dono', dataInicio: '2026-01-01'
    });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.user-name').textContent).toBe('Ana Silva');

    TestBed.inject(UserService).atualizarPerfil({ nome: 'Beatriz  Souza', email: 'bia@example.com' }).subscribe();
    http.expectOne({ method: 'PUT', url: `${environment.apiUrl}/user/me` }).flush(null);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.user-name').textContent).toBe('Beatriz  Souza');
    expect(fixture.nativeElement.querySelector('.user-avatar').textContent).toBe('BS');
    expect(fixture.nativeElement.querySelector('.user-role').textContent).toBe('Dono');
    http.verify();
  });
});
