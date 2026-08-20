export interface IngredienteResponse {
  itemId: number;
  nome: string;
  quantidade: number;
  unidadeMedida: string;
}

export interface ReceitaResponse {
  id: number;
  nome: string;
  modoPreparo: string;
  precoVenda: number;
  percentualMargemLucro: number;
  custoTotal: number;
  status: boolean;
  ingredientes: IngredienteResponse[];
  totalIngredientes: number;
}

export interface ReceitaListResponse {
  id: number;
  nome: string;
  precoVenda: number;
  custoTotal: number;
  status: boolean;
} 