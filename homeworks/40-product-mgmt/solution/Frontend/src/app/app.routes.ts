import { Routes } from '@angular/router';
import { ProductsList } from './products-list/products-list';
import { ProductsEdit } from './products-edit/products-edit';

export const routes: Routes = [
    { path: 'products', component: ProductsList },
    { path: 'products/edit/:productId', component: ProductsEdit },
    { path: '', redirectTo: '/products', pathMatch: 'full' }
];
