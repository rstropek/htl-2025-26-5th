import { Routes } from '@angular/router';
import { DummiesList } from './dummies-list/dummies-list';
import { GenerateRecords } from './generate-records/generate-records';

export const routes: Routes = [
    { path: 'dummies', component: DummiesList },
    { path: 'generate', component: GenerateRecords },
    { path: '', redirectTo: '/dummies', pathMatch: 'full' }
];
