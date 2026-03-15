import { Component, inject, model, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { Api } from '../api/api';

@Component({
  selector: 'app-order-list',
  imports: [RouterLink, FormsModule, DatePipe, CurrencyPipe],
  templateUrl: './order-list.html',
  styleUrl: './order-list.css',
})
export class OrderList {
  private api = inject(Api);

  // TODO: Add logic for component here
}
