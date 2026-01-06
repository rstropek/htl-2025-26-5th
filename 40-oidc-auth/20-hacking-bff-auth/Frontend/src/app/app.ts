import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ApiConfiguration } from './api/api-configuration';
import { environment } from '../environments/environment.development';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('frontend');
  protected readonly isAuthenticated = signal<boolean | null>(null);

  private http = inject(HttpClient);
  private apiConfiguration = inject(ApiConfiguration);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.checkAuth();
  }

  async checkAuth() {
    try {
      await firstValueFrom(this.http.get(`${environment.apiBaseUrl}/me`, {
        responseType: 'text'
      }));
      this.isAuthenticated.set(true);
    } catch (err: any) {
      // 401 means not authenticated, other errors also treated as not authenticated
      this.isAuthenticated.set(false);
    }
  }

  login() {
    // Redirect to WebApi login endpoint with current URL as redirect
    const currentUrl = window.location.href;
    window.location.href = `${environment.webApiBaseUrl}/login?redirect=${encodeURIComponent(currentUrl)}`;
  }

  logout() {
    // Redirect to WebApi logout endpoint with current URL as redirect
    const currentUrl = window.location.origin;
    window.location.href = `${environment.webApiBaseUrl}/logout?redirect=${encodeURIComponent(currentUrl)}`;
  }
}
