import { computed, effect, Injectable, signal } from '@angular/core';

export type PinRole = 'parent' | 'child' | 'none';

type StoredSession = {
  wishlistName: string;
  pin: string;
  role: PinRole;
};

const STORAGE_KEY = 'wishlist.session.v1';

@Injectable({ providedIn: 'root' })
export class SessionService {
  readonly wishlistName = signal<string>('');
  readonly pin = signal<string>('');
  readonly role = signal<PinRole>('none');

  readonly isAuthenticated = computed(() => !!this.wishlistName() && !!this.pin() && this.role() !== 'none');
  readonly isParent = computed(() => this.role() === 'parent');
  readonly isChild = computed(() => this.role() === 'child');

  constructor() {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      try {
        const s = JSON.parse(raw) as StoredSession;
        if (s?.wishlistName && s?.pin && (s.role === 'parent' || s.role === 'child')) {
          this.wishlistName.set(s.wishlistName);
          this.pin.set(s.pin);
          this.role.set(s.role);
        }
      } catch {
        // ignore
      }
    }

    effect(() => {
      const data: StoredSession = {
        wishlistName: this.wishlistName(),
        pin: this.pin(),
        role: this.role(),
      };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
    });
  }

  setSession(wishlistName: string, pin: string, role: PinRole) {
    this.wishlistName.set(wishlistName);
    this.pin.set(pin);
    this.role.set(role);
  }

  clear() {
    this.wishlistName.set('');
    this.pin.set('');
    this.role.set('none');
    localStorage.removeItem(STORAGE_KEY);
  }
}
