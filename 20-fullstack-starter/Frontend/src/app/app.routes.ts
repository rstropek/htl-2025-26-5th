import { Routes } from '@angular/router';
import { DummiesList } from './dummies-list/dummies-list';

export const routes: Routes = [
    { path: 'dummies', component: DummiesList },
    { path: '', redirectTo: '/dummies', pathMatch: 'full' }
];
