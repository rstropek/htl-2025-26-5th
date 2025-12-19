import { Routes } from '@angular/router';

import { LoginPage } from './pages/login-page/login-page';
import { WishlistItemsPage } from './pages/wishlist-items-page/wishlist-items-page';
import { AddItemPage } from './pages/add-item-page/add-item-page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginPage },
  { path: 'parent', component: WishlistItemsPage },
  { path: 'add-item', component: AddItemPage },
  { path: '**', redirectTo: 'login' }
];
