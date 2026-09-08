export interface TransacaoResumo {
  entrada: number;
  saida: number;
}

export interface TransacaoResponse {
  id: number;
  tipo: string; 
  categoria: string;
  valor: number;
  descricao: string;
  data: string; 
  isCancelado: boolean; 
  motivoCancelamento: string | null;
}