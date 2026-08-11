import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-summary-card',
  imports: [MatIconModule, CommonModule],
  templateUrl: './summary-card.html',
  styleUrl: './summary-card.css',
})
export class SummaryCard {
  label = input.required<string>();
  value = input.required<string | number>();
  icon = input.required<string>();
  colorClass = input.required<string>();
}
