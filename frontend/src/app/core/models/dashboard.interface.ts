export interface ReceitaResumo {
  receitaId: number;
  nome: string;
  precoVenda: number;
}

export interface PedidoResumo {
  pedidoId: number;
  clienteNome: string;
  valorTotal: number;
  dataCriacao: string;
  status: string;
}

export interface FinanceiroResumo {
  totalEntradas: number;
  totalSaidas: number;
  saldoAtual: number;
}

export interface DashboardResumo {
  nomeUsuario: string;
  totalItensEstoque: number;
  financeiro: FinanceiroResumo;
  ultimasReceitas: ReceitaResumo[];
  ultimosPedidos: PedidoResumo[];
}