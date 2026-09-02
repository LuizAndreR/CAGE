import { Component, input, output, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransacaoResponse, TransacaoResumo } from '../../../../core/models/financeiro.interface';

@Component({
  selector: 'app-financeiro-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './financeiro-list.html',
  styleUrl: './financeiro-list.css'
})
export class FinanceiroList {
  transacoes = input.required<TransacaoResponse[]>();
  resumo = input.required<TransacaoResumo>();

  novaTransacao = output<void>();
  selecionarTransacao = output<number>();
  filtrar = output<{ tipo?: string, categoria?: string, ano?: number, mes?: number }>();

  filtroTipo = '';
  filtroCategoria = '';
  filtroMes = '';

  // 1. Variáveis para gerenciar as datas dinâmicas
  mesesDisponiveis: { valor: string, rotulo: string }[] = [];
  private mesesVistos = new Set<string>();

  constructor() {
    // 2. O 'effect' reage automaticamente toda vez que o sinal transacoes() mudar
    effect(() => {
      const transacoesAtuais = this.transacoes();
      let encontrouNovoMes = false;

      transacoesAtuais.forEach(t => {
        // Extrai apenas o "YYYY-MM" da data que vem do banco (ex: "2026-09-02T19:17:50" -> "2026-09")
        const mesAno = t.data.substring(0, 7);
        
        // Se é um mês que ainda não tínhamos visto, nós aprendemos ele
        if (!this.mesesVistos.has(mesAno)) {
          this.mesesVistos.add(mesAno);
          encontrouNovoMes = true;
        }
      });

      // 3. Se achamos um mês novo, recriamos a lista para o <select> ordenando do mais novo pro mais antigo
      if (encontrouNovoMes) {
        this.mesesDisponiveis = Array.from(this.mesesVistos)
          .sort().reverse() // Garante que 09/2026 fique acima de 08/2026
          .map(valor => {
            const [ano, mes] = valor.split('-');
            return { 
              valor: valor, 
              rotulo: `${mes}/${ano}` // Transforma "2026-09" em "09/2026" para a usuária ler
            };
          });
      }
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatDate(dateStr: string): string {
    const data = new Date(dateStr);
    return data.toLocaleDateString('pt-BR', { timeZone: 'UTC' });
  }

  get saldoTotal(): number {
    return this.resumo().entrada - this.resumo().saida;
  }

  onFilterChange(): void {
    // 1. Tratamento para quebrar a data selecionada (Mês/Ano)
    let anoFormatado: number | undefined = undefined;
    let mesFormatado: number | undefined = undefined;

    if (this.filtroMes && this.filtroMes !== '') {
      const partes = this.filtroMes.split('-');
      if (partes.length === 2) {
        anoFormatado = parseInt(partes[0], 10);
        mesFormatado = parseInt(partes[1], 10);
      }
    }

    this.filtrar.emit({
      tipo: this.filtroTipo || undefined,
      categoria: this.filtroCategoria || undefined,
      ano: anoFormatado,
      mes: mesFormatado
    });
  }
}