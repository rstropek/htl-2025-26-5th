import { Routes } from '@angular/router';
import { CustomersList } from './customers-list/customers-list';
import { SecretsList } from './secrets-list/secrets-list';

export const routes: Routes = [
    { path: 'customers', component: CustomersList },
    { path: 'secrets', component: SecretsList },
    { path: '', redirectTo: '/customers', pathMatch: 'full' }
];
