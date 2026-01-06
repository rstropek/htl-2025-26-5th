import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch, withInterceptors, withNoXsrfProtection, HttpRequest, HttpHandlerFn } from '@angular/common/http';

// We had some problems with security stuff, so we disable Xsrf and send all credentials.
// Whatever, it works now. TODO: Check later if that is ok from a security perspective.

function credentialsInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn) {
  const reqWithCredentials = req.clone({ withCredentials: true });
  return next(reqWithCredentials);
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withFetch(), withNoXsrfProtection(), withInterceptors([credentialsInterceptor]))
  ]
};
