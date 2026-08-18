type Unit = 'G' | 'KG' | 'ML' | 'L' | 'TSP' | 'TBS' | 'CUP';

export interface EstoqueItem {
  id: number;
  nome: string;
  marca: string;
  quantidadeAtual: number;
  unidadeMedida: Unit; 
  valor?: number;        
  valorMedia?: number;   
  quantidadeMinima: number; 
  quantidadeMinina?: number;  
  unidadeMedidaReferenciaVolume?: string | null; 
  unidadeReferenciaVolume?: string | null; 
  pesoReferenciaEmGramas?: number | null;
}