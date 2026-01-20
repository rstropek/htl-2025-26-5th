import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { TravelListItemDto } from '../api/models';
import { travelsGet } from '../api/functions';

@Component({
  selector: 'app-travels-list',
  imports: [RouterLink],
  templateUrl: './travels-list.html',
  styleUrl: './travels-list.css'
})
export class TravelsList {
  protected readonly travels = signal<TravelListItemDto[] | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  private readonly api = inject(Api);

  async ngOnInit() {
    await this.refresh();
  }

  protected async refresh() {
    this.loading.set(true);
    this.error.set(null);

    try {
      const items = await this.api.invoke(travelsGet, {});
      this.travels.set(items);
    } catch (err: any) {
      this.error.set(err?.message ?? 'Failed to load travels.');
      this.travels.set(null);
    } finally {
      this.loading.set(false);
    }
  }
}
