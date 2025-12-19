import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Field, form, maxLength, minLength, required } from '@angular/forms/signals';
import { Api } from '../../api/api';
import { verifyPinNamePost } from '../../api/functions';
import { VerifyPinRequestDto } from '../../api/models';
import { PinRole, SessionService } from '../../services/session.service';

@Component({
  selector: 'app-login-page',
  imports: [Field],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage {
  private readonly api = inject(Api);
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly model = signal({
    wishlistName: '',
    pin: '',
  });

  protected readonly loginForm = form(this.model, (schema) => {
    required(schema.wishlistName, { message: 'Wishlist name is required' });
    maxLength(schema.wishlistName, 100, { message: 'Max length is 100 characters' });

    required(schema.pin, { message: 'PIN is required' });
    minLength(schema.pin, 6, { message: 'PIN must be 6 characters' });
    maxLength(schema.pin, 6, { message: 'PIN must be 6 characters' });
  });

  protected async onSubmit() {
    this.submitting.set(true);
    this.error.set(null);

    const name = this.loginForm.wishlistName().value().trim();
    const pin = this.loginForm.pin().value().trim();

    try {
      const body: VerifyPinRequestDto = { pin };
      const resp = await this.api.invoke(verifyPinNamePost, { name, body });
      
      const role = (resp.role?.toLowerCase() as PinRole) ?? 'none';
      if (role !== 'parent' && role !== 'child') {
        this.error.set('Invalid PIN');
        return;
      }

      this.session.setSession(name, pin, role);
      await this.router.navigate([role === 'parent' ? '/parent' : '/add-item']);
    } catch (err: any) {
      if (err?.status === 401) {
        this.error.set('Invalid wishlist name or PIN');
      } else {
        this.error.set(`Login failed (${JSON.stringify(err)})`);
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
