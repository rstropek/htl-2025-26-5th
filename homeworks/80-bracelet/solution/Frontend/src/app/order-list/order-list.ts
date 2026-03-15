import { Component, inject, model, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { Api } from '../api/api';
import { apiOrdersGet } from '../api/functions';
import { OrderListDto } from '../api/models';

@Component({
  selector: 'app-order-list',
  imports: [RouterLink, FormsModule, DatePipe, CurrencyPipe],
  templateUrl: './order-list.html',
  styleUrl: './order-list.css',
})
export class OrderList {
  private api = inject(Api);
  readonly orders = signal<OrderListDto[]>([]);
  readonly minTotalCosts = model<number | null>(null);

  ngOnInit() {
    this.loadOrders();
  }

  async loadOrders() {
    const min = this.minTotalCosts();
    const orders = await this.api.invoke(apiOrdersGet, {
      minTotalCosts: min !== null && min > 0 ? min : undefined,
    });
    this.orders.set(orders);
  }
}
