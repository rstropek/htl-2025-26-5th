import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { SessionService } from './services/session.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);

  protected readonly isAuthenticated = this.session.isAuthenticated;
  protected readonly isParent = this.session.isParent;
  protected readonly wishlistName = this.session.wishlistName;
  protected readonly roleLabel = computed(() => {
    const role = this.session.role();
    return role === 'parent' ? 'Parent' : role === 'child' ? 'Child' : '';
  });

  protected async logout() {
    this.session.clear();
    await this.router.navigateByUrl('/login');
  }
}
