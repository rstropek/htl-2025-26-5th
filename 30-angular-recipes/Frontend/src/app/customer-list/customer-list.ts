import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Customer } from '../api/models/customer';
import { customersGet, customersIdDelete } from '../api/functions';
import { ApiConfiguration } from '../api/api-configuration';

// TODO: If unfamiliar, research about Angular standalone components (no NgModule needed)
@Component({
  selector: 'app-customer-list',
  imports: [DatePipe, RouterLink, CurrencyPipe],
  templateUrl: './customer-list.html',
  styleUrl: './customer-list.css',
})
export class CustomerList implements OnInit {
  // TODO: If unfamiliar, research about Angular inject (dependency injection)
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  // TODO: If unfamiliar, research about Angular signals (in this case writeable signals)
  protected readonly customers = signal<Customer[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly selectedCustomerIds = signal<number[]>([]);
  
  // TODO: If unfamiliar, research about Angular computed signals
  protected readonly selectedCount = computed(() => this.selectedCustomerIds().length);
  protected readonly hasSelection = computed(() => this.selectedCount() > 0);

  ngOnInit(): void {
    this.loadCustomers();
  }

  private async loadCustomers() {
    this.loading.set(true);
    this.error.set(null);

    try {
      // TODO: If unfamiliar, research about Angular firstValueFrom (to convert an Observable to a Promise)
      const response = await firstValueFrom(customersGet(this.http, this.apiConfig.rootUrl));
      this.customers.set(response.body);
    } catch (error) {
      this.error.set('Error loading customers: ' + JSON.stringify(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected async deleteCustomer(id: number): Promise<void> {
    const customer = this.customers().find(c => c.id === id);
    if (!customer) return;

    const confirmed = confirm(
      `Do you really want to delete customer "${customer.name}"?\n\nThis action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      await firstValueFrom(customersIdDelete(this.http, this.apiConfig.rootUrl, { id }));
      // TODO: If unfamiliar, research about signal update function (for immutable state updates)
      // Remove customer from list
      this.customers.update(currentCustomers => currentCustomers.filter(c => c.id !== id));
    } catch (err: any) {
      alert('Error deleting customer: ' + err.message);
    }
  }

  protected toggleCustomerSelection(id: number): void {
    this.selectedCustomerIds.update(currentSelection => {
      if (currentSelection.includes(id)) {
        // Remove from selection
        return currentSelection.filter(customerId => customerId !== id);
      } else {
        // Add to selection
        return [...currentSelection, id];
      }
    });
  }

  protected isCustomerSelected(id: number): boolean {
    return this.selectedCustomerIds().includes(id);
  }

  protected async deleteSelectedCustomers(): Promise<void> {
    const selectedIds = this.selectedCustomerIds();
    if (selectedIds.length === 0) return;

    const customerNames = this.customers()
      .filter(c => selectedIds.includes(c.id!))
      .map(c => c.name)
      .join(', ');

    const confirmed = confirm(
      `Do you really want to delete ${selectedIds.length} customer(s)?\n\n${customerNames}\n\nThis action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      // Delete all selected customers
      await Promise.all(
        selectedIds.map(id => 
          firstValueFrom(customersIdDelete(this.http, this.apiConfig.rootUrl, { id }))
        )
      );
      
      // Remove deleted customers from list
      this.customers.update(currentCustomers => 
        currentCustomers.filter(c => !selectedIds.includes(c.id!))
      );
      
      // Clear selection
      this.selectedCustomerIds.update(() => []);
    } catch (err: any) {
      alert('Error deleting customers: ' + err.message);
    }
  }
}
