import { computed, Injectable, signal } from '@angular/core';
import { AdminUser } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly currentUser = signal<AdminUser | null>(null);
  readonly user = this.currentUser.asReadonly();
  readonly authenticated = computed(() => this.currentUser() !== null);
  readonly initials = computed(() => this.currentUser()?.fullName.split(/\s+/).slice(0, 2).map(part => part[0]).join('').toUpperCase() ?? 'AD');

  set(user: AdminUser): void {
    this.currentUser.set(user);
  }

  clear(): void {
    this.currentUser.set(null);
  }
}
