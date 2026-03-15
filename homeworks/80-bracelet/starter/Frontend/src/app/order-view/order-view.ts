import { Component, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { BraceletPreview } from '../bracelet-preview/bracelet-preview';
import { Api } from '../api/api';
import { BraceletItem, BraceletDataService } from '../bracelet-data.service';

@Component({
  selector: 'app-order-view',
  imports: [CurrencyPipe, DatePipe, BraceletPreview],
  templateUrl: './order-view.html',
  styleUrl: './order-view.css',
})
export class OrderView {
  private api = inject(Api);
  private braceletDataService = inject(BraceletDataService);

  readonly id = input<string>();
}
