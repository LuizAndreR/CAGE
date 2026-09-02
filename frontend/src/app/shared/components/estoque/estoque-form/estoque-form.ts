import { Component, inject, input, output, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { EstoqueItem } from '../../../../core/models/estoque.interface';

@Component({
  selector: 'app-estoque-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './estoque-form.html',
  styleUrl: './estoque-form.css'
})
export class EstoqueForm {
  private fb = inject(FormBuilder);

  itemParaEditar = input<EstoqueItem | null>(null);
  salvar = output<any>();
  cancelar = output<void>();

  estoqueForm = this.fb.group({
    id: [null as number | null], 
    nome: ['', Validators.required],
    marca: ['', Validators.required],
    quantidadeAtual: [0, [Validators.required, Validators.min(0)]],
    valor: [0, [Validators.required, Validators.min(0)]], 
    unidadeMedida: ['G', Validators.required],
    quantidadeMinima: [0, Validators.required], 
    pesoReferenciaEmGramas: [null as number | null],
    unidadeMedidaReferenciaVolume: [null as string | null] 
  });

  constructor() {
    effect(() => {
      const item = this.itemParaEditar();
      if (item) {
        this.estoqueForm.patchValue({
          ...item,
          quantidadeMinima: item.quantidadeMinima ?? item.quantidadeMinina,
          valor: item.valorMedia,
          unidadeMedidaReferenciaVolume: item.unidadeMedidaReferenciaVolume ?? item.unidadeReferenciaVolume
        });
        this.estoqueForm.get('valor')?.disable();
      } else {
        this.estoqueForm.reset({ unidadeMedida: 'G' });
        this.estoqueForm.get('valor')?.enable();
      }
    });
  }
  
  onSubmit(): void {
    if (this.estoqueForm.valid) {
      const payload = { ...this.estoqueForm.getRawValue() };
      
      let peso = payload.pesoReferenciaEmGramas;
      let volume = payload.unidadeMedidaReferenciaVolume;

      if (!peso || peso <= 0 || !volume || volume.trim() === '') {
        payload.pesoReferenciaEmGramas = null;
        payload.unidadeMedidaReferenciaVolume = null;
      }
      
      this.salvar.emit(payload);
    } else {
      this.estoqueForm.markAllAsTouched();
    }
  }
}