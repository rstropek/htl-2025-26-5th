import { Routes } from '@angular/router';
import { LaufbewerbList } from './laufbewerb-list/laufbewerb-list';
import { LaufbewerbEdit } from './laufbewerb-edit/laufbewerb-edit';
import { LaufAuswertung } from './lauf-auswertung/lauf-auswertung';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'laufbewerbe' },
  { path: 'laufbewerbe', component: LaufbewerbList },
  { path: 'laufbewerbe/new', component: LaufbewerbEdit },
  { path: 'laufbewerbe/:id/edit', component: LaufbewerbEdit },
  { path: 'laufbewerbe/auswertung', component: LaufAuswertung },
];
