export type StatusPedido = 'Pendente' | 'EmProducao' | 'ProntoEntregue' | 'Concluido';

export interface GetAllPedidosResponse {
  id: number;
  clienteNome: string;
  dataCriacao: string; 
  dataEntrega: string | null; 
  valorTotal: number;
  statusPedido: StatusPedido; 
  pago: boolean;
}