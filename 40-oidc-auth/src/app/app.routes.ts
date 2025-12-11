import { Routes } from '@angular/router';
import { Protected } from './protected/protected';
import { autoLoginPartialRoutesGuard } from 'angular-auth-oidc-client';

export const routes: Routes = [
  {
    path: 'protected',
    component: Protected,
    canActivate: [autoLoginPartialRoutesGuard],
  }
];
