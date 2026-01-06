import { getLocaleFirstDayOfWeek, JsonPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, JsonPipe, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  isAuthenticated = this.oidcSecurityService.authenticated;
  userData = this.oidcSecurityService.userData;
  
  login(): void {
    this.oidcSecurityService.authorize();
  }

  async checkAuthWithSilentRenew(): Promise<void> {
    const result = await firstValueFrom(this.oidcSecurityService.checkAuthIncludingServer());
    console.log(result);
  }

  async logout(): Promise<void> {
    await firstValueFrom(this.oidcSecurityService.logoff());
  }
}
