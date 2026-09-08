import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-list-panel',
  imports: [RouterModule, MatIconModule, CommonModule],
  templateUrl: './list-panel.html',
  styleUrl: './list-panel.css',
})

export class ListPanel {
  @Input() title!: string;
  @Input() iconName!: string;
  @Input() iconClass!: string;
  @Input() linkPath!: string;
  @Input() linkText!: string;
}
