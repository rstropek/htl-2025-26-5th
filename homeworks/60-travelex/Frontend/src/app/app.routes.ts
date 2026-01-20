import { Routes } from '@angular/router';
import { TravelDetails } from './travel-details/travel-details';
import { TravelUpload } from './travel-upload/travel-upload';
import { TravelsList } from './travels-list/travels-list';

export const routes: Routes = [
    { path: 'travels', component: TravelsList },
    { path: 'travels/upload', component: TravelUpload },
    { path: 'travels/:id', component: TravelDetails },
    { path: '', redirectTo: '/travels', pathMatch: 'full' },
    { path: '**', redirectTo: '/travels' }
];
