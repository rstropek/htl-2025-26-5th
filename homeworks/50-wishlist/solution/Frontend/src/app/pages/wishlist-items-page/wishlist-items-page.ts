import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../../api/api';
import {
  wishlistNameItemsItemIdDelete,
  wishlistNameItemsItemIdMarkAsBoughtPost,
  wishlistNameItemsPost,
} from '../../api/functions';
import { AuthRequestDto, MarkAsBoughtRequestDto, WishlistItemDto } from '../../api/models';
import { SessionService } from '../../services/session.service';

@Component({
  selector: 'app-wishlist-items-page',
  imports: [],
  templateUrl: './wishlist-items-page.html',
  styleUrl: './wishlist-items-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WishlistItemsPage {
  private readonly session = inject(SessionService);
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected readonly loading = signal<boolean>(true);
  protected readonly busyItemId = signal<number | null>(null);
  protected readonly error = signal<string | null>(null);

  protected readonly items = signal<WishlistItemDto[]>([]);

  async ngOnInit() {
    if (!this.session.isAuthenticated()) {
      await this.router.navigate(['/login']);
      return;
    }

    await this.load();
  }

  protected async load() {
    this.loading.set(true);
    this.error.set(null);
    try {
      const name = this.session.wishlistName();
      const body: AuthRequestDto = { pin: this.session.pin() };
      const items = await this.api.invoke(wishlistNameItemsPost, { name, body });
      this.items.set(items);
    } catch (err: any) {
      if (err?.status === 401) {
        this.session.clear();
        await this.router.navigate(['/login']);
        return;
      }

      if (err?.status === 403) {
        this.error.set('Forbidden. This page is for parents.');
      } else {
        this.error.set(`Failed to load wishlist items (${JSON.stringify(err)})`);
      }
    } finally {
      this.loading.set(false);
    }
  }

  protected async toggleBought(item: WishlistItemDto) {
    this.busyItemId.set(item.id);
    this.error.set(null);
    try {
      const name = this.session.wishlistName();
      const body: MarkAsBoughtRequestDto = { pin: this.session.pin(), bought: !item.bought };
      await this.api.invoke(wishlistNameItemsItemIdMarkAsBoughtPost, { name, itemId: item.id, body });
      await this.load();
    } catch (err: any) {
      this.error.set(err?.error ?? 'Failed to update item');
    } finally {
      this.busyItemId.set(null);
    }
  }

  protected async delete(item: WishlistItemDto) {
    if (!confirm(`Delete “${item.itemName}”?`)) {
      return;
    }

    this.busyItemId.set(item.id);
    this.error.set(null);
    try {
      const name = this.session.wishlistName();
      const body: AuthRequestDto = { pin: this.session.pin() };
      await this.api.invoke(wishlistNameItemsItemIdDelete, { name, itemId: item.id, body });
      await this.load();
    } catch (err: any) {
      this.error.set(err?.error ?? 'Failed to delete item');
    } finally {
      this.busyItemId.set(null);
    }
  }
}
