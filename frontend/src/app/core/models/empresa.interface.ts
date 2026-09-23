export interface EmpresaResponse {
  id: number;
  nome: string;
  endereco: string;
  dataCadastro: string;
  status: string;
}

export interface AtualizarEmpresaRequest {
  nome: string;
  endereco: string;
}
