import { Component, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { BraceletPreview } from '../bracelet-preview/bracelet-preview';
import { Api } from '../api/api';
import { apiOrdersIdGet } from '../api/functions';
import { OrderDetailDto } from '../api/models';
import { BraceletItem, BraceletDataService } from '../bracelet-data.service';
import { OrderItemDto } from '../api/models';

interface ParsedOrderItemDto extends OrderItemDto {
  items: BraceletItem[];
}

interface ParsedOrderDetailDto extends OrderDetailDto {
  orderItems: ParsedOrderItemDto[];
}

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
  readonly order = signal<ParsedOrderDetailDto | null>(null);

  constructor() {
    effect(async () => {
      const id = this.id();
      if (id) {
        const raw = await this.api.invoke(apiOrdersIdGet, { id: +id });
        this.order.set({
          ...raw,
          orderItems: raw.orderItems.map(item => ({
            ...item,
            items: this.braceletDataService.parse(item.braceletData),
          })),
        });
      }
    });
  }
}
