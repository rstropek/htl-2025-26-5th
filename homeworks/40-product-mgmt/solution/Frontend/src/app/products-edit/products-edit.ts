import { Component, inject, signal, ChangeDetectionStrategy, input } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Api } from '../api/api';
import { ProductUpdateDto } from '../api/models';
import { productsIdGet, productsIdPut } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-products-edit',
  imports: [FormsModule],
  templateUrl: './products-edit.html',
  styleUrl: './products-edit.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsEdit {
  protected readonly loading = signal<boolean>(true);
  protected readonly saving = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  protected readonly productCode = signal<string>('');
  protected readonly productName = signal<string>('');
  protected readonly productDescription = signal<string>('');
  protected readonly category = signal<string>('');
  protected readonly pricePerUnit = signal<number>(0);

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);
  public productId = input<number>();

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadData();
  }

  private async loadData() {
    try {
      const product = await this.api.invoke(productsIdGet, { id: this.productId()! });

      if (product) {
        this.productCode.set(product.productCode || '');
        this.productName.set(product.productName || '');
        this.productDescription.set(product.productDescription || '');
        this.category.set(product.category || '');
        this.pricePerUnit.set(product.pricePerUnit || 0);
      } else {
        this.error.set('Product not found');
      }
    } catch (err) {
      this.error.set('Failed to load product');
      console.error(err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSubmit() {
    this.saving.set(true);
    this.error.set(null);

    try {
      const updateDto: ProductUpdateDto = {
        productCode: this.productCode(),
        productName: this.productName(),
        productDescription: this.productDescription() || null,
        category: this.category() || null,
        pricePerUnit: this.pricePerUnit()
      };

      await this.api.invoke(productsIdPut, {
        id: this.productId()!,
        body: updateDto
      });

      this.router.navigate(['/products']);
    } catch (err: any) {
      this.error.set(err.error || 'Failed to save product');
      console.error(err);
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel() {
    this.router.navigate(['/products']);
  }
}
