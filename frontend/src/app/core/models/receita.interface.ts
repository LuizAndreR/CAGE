export interface IngredienteResponse {
  itemId: number;
  marca: string;
  alternativasIds: number[];
  alternativas?: { itemId: number; nome: string; marca: string; unidadeMedida: string }[];
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
  custoExtra: number;
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