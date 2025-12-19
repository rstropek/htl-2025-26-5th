import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Field, form, maxLength, required } from '@angular/forms/signals';
import { HttpClient } from '@angular/common/http';
import { Api } from '../../api/api';
import { giftCategoriesGet, wishlistNameItemsAddPost, wishlistNameItemsPost } from '../../api/functions';
import { AddItemRequestDto } from '../../api/models';
import { SessionService } from '../../services/session.service';

type AddItemModel = {
  itemName: string;
  category: string;
};

@Component({
  selector: 'app-add-item-page',
  imports: [Field],
  templateUrl: './add-item-page.html',
  styleUrl: './add-item-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddItemPage implements OnInit {
  private readonly session = inject(SessionService);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  protected readonly categories = signal<string[]>([]);

  protected readonly model = signal<AddItemModel>({
    itemName: '',
    category: '',
  });

  protected readonly addForm = form(this.model, (schema) => {
    required(schema.itemName, { message: 'Item name is required' });
    maxLength(schema.itemName, 100, { message: 'Max length is 100 characters' });

    required(schema.category, { message: 'Category is required' });
    maxLength(schema.category, 50, { message: 'Max length is 50 characters' });
  });

  async ngOnInit(): Promise<void> {
    if (!this.session.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.categories.set(await this.api.invoke(giftCategoriesGet));
  }

  protected async onSubmit() {
    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    try {
      const name = this.session.wishlistName();
      const body: AddItemRequestDto = {
        pin: this.session.pin(),
        itemName: this.addForm.itemName().value().trim(),
        category: this.addForm.category().value().trim(),
      };
      const resp = await this.api.invoke(wishlistNameItemsAddPost, { name, body });

      this.success.set(`You whish for ${resp.itemName} has been sent to Santa! 🎅🎄`);
      this.addForm.itemName().value.set('');
      this.addForm.category().value.set('');
      this.error.set(null);
      this.addForm.itemName().reset();
      this.addForm.category().reset();
    } catch (err: any) {
      if (err?.status === 401) {
        this.session.clear();
        await this.router.navigate(['/login']);
        return;
      }
      this.error.set(err?.error ?? 'Failed to add item');
    } finally {
      this.saving.set(false);
    }
  }
}
