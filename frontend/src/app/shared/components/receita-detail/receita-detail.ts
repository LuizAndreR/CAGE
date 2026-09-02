import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceitaResponse } from '../../../core/models/receita.interface';

@Component({
  selector: 'app-receita-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './receita-detail.html',
  styleUrl: './receita-detail.css'
})
export class ReceitaDetail {
  receita = input.required<ReceitaResponse>();

  voltar = output<void>();
  editar = output<number>();
  excluir = output<number>();

  isModalAberto = signal<boolean>(false);

  abrirModal(): void {
    this.isModalAberto.set(true);
  }

  fecharModal(): void {
    this.isModalAberto.set(false);
  }

  confirmarExclusao(): void {
    this.excluir.emit(this.receita().id);
    this.fecharModal();
  }
}