import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Api } from '../api/api';
import { CustomerDto } from '../api/models';
import { customersGet, customersPost, customersIdDelete } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-customers-list',
  imports: [FormsModule],
  templateUrl: './customers-list.html',
  styleUrl: './customers-list.css'
})
export class CustomersList {
  protected customers = signal<CustomerDto[] | null>(null);
  protected newCustomerName = signal<string>('');
  protected error = signal<string | null>(null);
  protected loading = signal<boolean>(false);

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private sanitizer = inject(DomSanitizer);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadCustomers();
  }

  async loadCustomers() {
    this.loading.set(true);
    this.error.set(null);
    try {
      const customers = await this.api.invoke(customersGet, {});
      this.customers.set(customers);
    } catch (err: any) {
      this.error.set(err.error || 'Failed to load customers');
    } finally {
      this.loading.set(false);
    }
  }

  async addCustomer() {
    const name = this.newCustomerName();
    if (!name.trim()) return;

    this.loading.set(true);
    this.error.set(null);
    try {
      await this.api.invoke(customersPost, { body: { name } });
      this.newCustomerName.set('');
      await this.loadCustomers();
    } catch (err: any) {
      this.error.set(err.error || 'Failed to add customer');
      this.loading.set(false);
    }
  }

  async deleteCustomer(id: number) {
    this.loading.set(true);
    this.error.set(null);
    try {
      await this.api.invoke(customersIdDelete, { id });
      await this.loadCustomers();
    } catch (err: any) {
      this.error.set(err.error || 'Failed to delete customer');
      this.loading.set(false);
    }
  }

  getUnsafeHtml(name: string): SafeHtml {
    // We want to allow that the user can enter HTML in the name field (e.g. for bold text with `<b>`)
    return this.sanitizer.bypassSecurityTrustHtml(name);
  }
}
