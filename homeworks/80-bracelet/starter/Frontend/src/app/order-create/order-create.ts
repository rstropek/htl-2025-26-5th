import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { BraceletBuilder } from '../bracelet-builder/bracelet-builder';
import { BraceletPreview } from '../bracelet-preview/bracelet-preview';
import { Api } from '../api/api';
import { BraceletItem, BraceletDataService } from '../bracelet-data.service';

@Component({
  selector: 'app-order-create',
  imports: [FormsModule, DecimalPipe, BraceletBuilder, BraceletPreview],
  templateUrl: './order-create.html',
  styleUrl: './order-create.css',
})
export class OrderCreate {
  private api = inject(Api);
  private router = inject(Router);
  private braceletDataService = inject(BraceletDataService);

  readonly demoBracelet: BraceletItem[] = [
    { type: 'letter', value: 'A' },
    { type: 'spacer', value: 'pink', hex: '#f4a0a0' },
    { type: 'letter', value: 'B' },
    { type: 'spacer', value: 'blue', hex: '#a0a0f4' },
    { type: 'letter', value: 'C' },
  ];

  onBraceletChanged(items: BraceletItem[]): void {
    // This method will be called whenever the user makes a change in the bracelet builder.
    // The `items` parameter contains the current state of the bracelet as an array of 
    // `BraceletItem`s.
    // TODO: Implement any logic needed to handle changes to the bracelet design
  }
}
