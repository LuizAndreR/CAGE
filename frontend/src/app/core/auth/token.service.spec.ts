import { TokenService } from './token.service';

describe('TokenService — papel do usuário', () => {
  const service = new TokenService();
  afterEach(() => vi.restoreAllMocks());

  it.each(['role', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])('lê o papel da claim %s', claim => {
    const payload = btoa(JSON.stringify({ [claim]: 'Dono' })).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
    vi.spyOn(service, 'getAccessToken').mockReturnValue(`header.${payload}.signature`);
    expect(service.getRole()).toBe('Dono');
  });

  it.each([null, 'invalido', 'header.e30.signature'])('não concede papel para token ausente ou inválido: %s', token => {
    vi.spyOn(service, 'getAccessToken').mockReturnValue(token);
    expect(service.getRole()).toBeNull();
  });
});
