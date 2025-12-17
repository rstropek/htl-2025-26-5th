import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../api/api';
import { ProductDto } from '../api/models';
import { productsGet, productsIdDelete, categoriesGet } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-products-list',
  imports: [FormsModule],
  templateUrl: './products-list.html',
  styleUrl: './products-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsList {
  protected readonly products = signal<ProductDto[]>([]);
  protected readonly categories = signal<string[]>([]);

  protected readonly selectedCategory = signal<string>('');
  protected readonly maxUnitPrice = signal<number | null>(null);

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadData();
  }

  private async loadData() {
    let filter = {};
    if (this.selectedCategory()) {
      filter = { ...filter, category: this.selectedCategory() };
    }
    if (this.maxUnitPrice() !== null) {
      filter = { ...filter, maxUnitPrice: this.maxUnitPrice() };
    }
    const products = await this.api.invoke(productsGet, filter);

    const categories = await this.api.invoke(categoriesGet, {});

    this.products.set(products);
    this.categories.set(categories.filter((c) => c !== null) as string[]);
  }

  protected onSearch() {
    this.loadData();
  }

  protected async deleteProduct(id: number) {
    if (confirm('Are you sure you want to delete this product?')) {
      await this.api.invoke(productsIdDelete, { id });
      await this.loadData();
    }
  }

  protected editProduct(id: number) {
    this.router.navigate(['/products/edit', id]);
  }
}
