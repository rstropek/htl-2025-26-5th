import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { TravelDetailsDto } from '../api/models';
import { travelsIdGet } from '../api/functions';
import { DatePipe, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-travel-details',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './travel-details.html',
  styleUrl: './travel-details.css'
})
export class TravelDetails implements OnInit {
  protected readonly travel = signal<TravelDetailsDto | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  private readonly api = inject(Api);

  protected readonly id = input.required<string>();

  public ngOnInit(): void {
    const idText = this.id();
    const id = Number(idText);

    if (!idText || Number.isNaN(id)) {
      this.error.set('Invalid travel id.');
      this.travel.set(null);
      return;
    }

    void this.load(id);
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
