import { Routes } from '@angular/router';
import { OrderList } from './order-list/order-list';
import { OrderCreate } from './order-create/order-create';
import { OrderView } from './order-view/order-view';

export const routes: Routes = [
    { path: 'orders', component: OrderList },
    { path: 'orders/new', component: OrderCreate },
    { path: 'orders/:id', component: OrderView },
    { path: '', redirectTo: '/orders', pathMatch: 'full' }
];
