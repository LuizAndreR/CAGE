export interface UsuarioResponse {
  id: number;
  nome: string;
  email: string;
  role: string;
  dataInicio: string;
}

export interface AtualizarPerfilRequest {
  nome: string;
  email: string;
}

export interface AlterarSenhaRequest {
  senhaAtual: string;
  novaSenha: string;
}

export const FUNCOES_FUNCIONARIO = ['Dono', 'Confeiteiro', 'Auxiliar', 'Decorador', 'Atendente', 'Caixa'] as const;

export interface CadastroFuncionarioRequest {
  adminRole: false;
  nome: string;
  email: string;
  senha: string;
  role: typeof FUNCOES_FUNCIONARIO[number];
}
