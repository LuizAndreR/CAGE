type Unit = 'G' | 'KG' | 'ML' | 'L' | 'TSP' | 'TBS' | 'CUP';

export interface EstoqueItem {
  id: number;
  nome: string;
  marca: string;
  quantidadeAtual: number;
  unidade: Unit;
  valorMedio: number;
  quantidadeMinima: number;
  pesoReferenciaEmGramas?: number;
  unidadeMedidaReferenciaVolume?: string;
}