import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { BraceletBuilder } from '../bracelet-builder/bracelet-builder';
import { BraceletPreview } from '../bracelet-preview/bracelet-preview';
import { Api } from '../api/api';
import { apiOrdersPost, apiValidateBraceletPost } from '../api/functions';
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

  readonly customerName = signal('');
  readonly customerAddress = signal('');
  readonly bracelets = signal<{ items: BraceletItem[]; cost: number }[]>([]);
  readonly error = signal('');

  readonly validatingBracelet = signal(false);
  readonly braceletError = signal('');
  readonly braceletWarning = signal('');
  readonly pendingBracelet = signal<{ items: BraceletItem[]; cost: number } | null>(null);

  readonly currentBraceletData = signal<BraceletItem[]>([]);

  readonly totalCost = computed(() => {
    return this.bracelets().reduce((sum, b) => sum + b.cost, 0);
  });

  onBraceletChanged(items: BraceletItem[]): void {
    this.currentBraceletData.set(items);
  }

  async addBracelet(): Promise<void> {
    const items = this.currentBraceletData();
    const data = this.braceletDataService.serialize(items);
    this.braceletError.set('');
    this.braceletWarning.set('');
    this.pendingBracelet.set(null);

    this.validatingBracelet.set(true);
    try {
      const result = await this.api.invoke(apiValidateBraceletPost, {
        body: { braceletData: data },
      });

      if (result.error) {
        this.braceletError.set(result.error);
        return;
      }

      if (result.mixedColorsWarning) {
        this.braceletWarning.set('You are mixing spacer colors!');
        this.pendingBracelet.set({ items, cost: result.cost! });
        return;
      }

      this.bracelets.update(list => [...list, { items, cost: result.cost! }]);
    } catch {
      this.braceletError.set('NetworkError');
    } finally {
      this.validatingBracelet.set(false);
    }
  }

  addAnyway(): void {
    const pending = this.pendingBracelet();
    if (!pending) {
      return;
    }

    this.bracelets.update(list => [...list, pending]);
    this.braceletWarning.set('');
    this.pendingBracelet.set(null);
  }

  removeBracelet(index: number): void {
    this.bracelets.update(list => list.filter((_, i) => i !== index));
  }

  async placeOrder(): Promise<void> {
    if (!this.customerName().trim()) {
      this.error.set('Customer name is required.');
      return;
    }
    if (!this.customerAddress().trim()) {
      this.error.set('Customer address is required.');
      return;
    }
    if (this.bracelets().length === 0) {
      this.error.set('Add at least one bracelet.');
      return;
    }
    this.error.set('');

    try {
      await this.api.invoke(apiOrdersPost, {
        body: {
          customerName: this.customerName(),
          customerAddress: this.customerAddress(),
          bracelets: this.bracelets().map(b => this.braceletDataService.serialize(b.items)),
        },
      });
      this.router.navigate(['/orders']);
    } catch (err: any) {
      this.error.set(err.error || 'Failed to place order.');
    }
  }
}
