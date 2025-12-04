import { Routes } from '@angular/router';
import { CustomerList } from './customer-list/customer-list';
import { CustomerEdit } from './customer-edit/customer-edit';

export const routes: Routes = [
  { path: '', redirectTo: '/customers', pathMatch: 'full' },
  { path: 'customers', component: CustomerList },
  { path: 'customers/:id/edit', component: CustomerEdit }
];
