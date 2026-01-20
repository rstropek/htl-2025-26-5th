import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { TravelDetailsDto } from '../api/models';
import { travelsIdGet } from '../api/functions';

@Component({
  selector: 'app-travel-details',
  imports: [RouterLink],
  templateUrl: './travel-details.html',
  styleUrl: './travel-details.css'
})
export class TravelDetails {
  protected readonly travel = signal<TravelDetailsDto | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  private readonly api = inject(Api);
  private readonly route = inject(ActivatedRoute);

  async ngOnInit() {
    const idText = this.route.snapshot.paramMap.get('id');
    const id = Number(idText);

    if (!idText || Number.isNaN(id)) {
      this.error.set('Invalid travel id.');
      return;
    }

    await this.load(id);
  }

  protected formatDate(value: string): string {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat(undefined, {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  }

  protected formatMoney(value: number): string {
    return new Intl.NumberFormat(undefined, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  protected total(t: TravelDetailsDto): number {
    return (t.mileage ?? 0) + (t.perDiem ?? 0) + (t.expenses ?? 0);
  }

  private async load(id: number) {
    this.loading.set(true);
    this.error.set(null);

    try {
      const details = await this.api.invoke(travelsIdGet, { id });
      this.travel.set(details);
    } catch (err: any) {
      if (err?.status === 404) {
        this.error.set('Travel not found.');
      } else {
        this.error.set(err?.message ?? 'Failed to load travel details.');
      }
      this.travel.set(null);
    } finally {
      this.loading.set(false);
    }
  }
}
