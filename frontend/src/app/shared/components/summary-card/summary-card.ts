import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-summary-card',
  imports: [MatIconModule, CommonModule],
  templateUrl: './summary-card.html',
  styleUrl: './summary-card.css',
})
export class SummaryCard {
  @Input() label!: string;
  @Input() value!: string | number;
  @Input() icon!: string;
  @Input() colorClass!: string;
}
