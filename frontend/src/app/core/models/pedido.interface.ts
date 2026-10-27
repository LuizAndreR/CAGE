export type StatusPedido = 'Pendente' | 'EmProducao' | 'ProntoEntregue' | 'Concluido';

export interface AllPedidosResponse {
  id: number;
  clienteNome: string;
  dataCriacao: string; 
  dataEntrega: string | null; 
  valorTotal: number;
  statusPedido: StatusPedido; 
  pago: boolean;
}

export interface ItemPedidoDetailDto {
  receitaId: number;
  nomeReceita: string;
  quantidade: number;
  valorUnitario: number;
  subTotal: number; 
}

export interface PedidoResponse {
  id: number;
  clienteNome: string;
  telefoneCliente?: string;
  descricao?: string;      
  dataCriacao: string;      
  dataEntrega?: string;     
  dataPagamento?: string;   
  valorTotal: number;       
  statusPedido: StatusPedido;
  pago: boolean;
  itens: ItemPedidoDetailDto[]; 
}
export interface EstoqueNegativoAviso {
  itemId: number;
  nome: string;
  marca: string;
  saldo: number;
  unidadeMedida: string;
  mensagem: string;
}

export interface UpdateStatusResponse {
  avisos: EstoqueNegativoAviso[];
}
