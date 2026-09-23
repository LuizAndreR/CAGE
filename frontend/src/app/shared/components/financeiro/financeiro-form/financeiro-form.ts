import { Component, inject, OnInit, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';

@Component({
  selector: 'app-financeiro-form',
  standalone: true,
  // IMPORTANTE: Importar o ReactiveFormsModule para usar formGroup no HTML
  imports: [CommonModule, ReactiveFormsModule], 
  templateUrl: './financeiro-form.html',
  styleUrl: './financeiro-form.css'
})
export class FinanceiroForm implements OnInit {
  private fb = inject(FormBuilder);

  // Emissores de eventos para o componente orquestrador
  cancelar = output<void>();
  salvar = output<{ tipo: string, categoria: string, valor: number, data: string, descricao: string }>();

  transacaoForm!: FormGroup;

  // Espelho exato dos Enums C# (Sem acentos nos valores para o backend aceitar)
  tiposDisponiveis = [
    { valor: 'Entrada', rotulo: 'Entrada' },
    { valor: 'Saida', rotulo: 'Saída' } 
  ];

  categoriasDisponiveis = [
    { valor: 'Vendas', rotulo: 'Vendas' },
    { valor: 'Compras', rotulo: 'Compras' },
    { valor: 'Fixos', rotulo: 'Fixos' },
    { valor: 'Manutencao', rotulo: 'Manutenção' },
    { valor: 'Salarios', rotulo: 'Salários' },
    { valor: 'Marketing', rotulo: 'Marketing' },
    { valor: 'Impostos', rotulo: 'Impostos' },
    { valor: 'Outros', rotulo: 'Outros' }
  ];

  ngOnInit(): void {
    // Configuração do Formulário Reativo baseada no FluentValidation do C#
    this.transacaoForm = this.fb.group({
      tipo: ['Entrada', Validators.required],
      data: [this.getHojeFormatado(), [Validators.required, this.dataValidator]],
      valor: [null, [Validators.required, Validators.min(0.01)]], // Valor > 0
      categoria: ['', Validators.required],
      descricao: ['', [Validators.required, Validators.maxLength(1000)]] // Max 1000 caracteres
    });
  }

  // Validador customizado replicando a regra do backend: Must(data <= DateTime.UtcNow) e >= 1 ano
  private dataValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    // Como a validação ocorre no navegador do usuário, usamos datas locais
    const dataSelecionada = new Date(control.value + 'T00:00:00'); // Força meia-noite local
    const hoje = new Date();
    hoje.setHours(23, 59, 59, 999); // Fim do dia de hoje
    
    const umAnoAtras = new Date();
    umAnoAtras.setFullYear(hoje.getFullYear() - 1);
    umAnoAtras.setHours(0, 0, 0, 0);

    if (dataSelecionada > hoje) {
      return { dataFutura: true };
    }
    if (dataSelecionada < umAnoAtras) {
      return { dataMuitoAntiga: true };
    }
    return null;
  }

  private getHojeFormatado(): string {
    const hoje = new Date();
    // Retorna YYYY-MM-DD exigido pelo input type="date"
    return hoje.toISOString().split('T')[0]; 
  }

  onSubmit(): void {
    if (this.transacaoForm.invalid) {
      this.transacaoForm.markAllAsTouched(); // Exibe os erros se a usuária clicar em salvar vazio
      return;
    }

    const payload = this.transacaoForm.getRawValue();
    
    // Tratamento de segurança: Converter valor para número
    // O Angular pode retornar string se o input type="text" for usado com máscara no futuro
    payload.valor = parseFloat(payload.valor.toString().replace(',', '.'));

    this.salvar.emit(payload);
  }
}