type Unit = 'G' | 'KG' | 'ML' | 'L' | 'TSP' | 'TBS' | 'CUP';

export interface EstoqueItem {
  id: number;
  nome: string;
  marca: string;
  quantidadeAtual: number;
  unidadeMedida: Unit; 
  valor?: number;        // Usado pelo Formulário (Write)
  valorMedia?: number;   // Devolvido pela API na Listagem (Read)
  quantidadeMinima: number; 
  pesoReferenciaEmGramas?: number | null;
  unidadeMedidaReferenciaVolume?: string | null; 
}