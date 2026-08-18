import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, finalize, Observable, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminUser, AuthenticationProviders, LoginRequest, safeAdminReturnUrl } from './auth.models';
import { AuthSessionService } from './auth-session.service';
import { CsrfTokenService } from './csrf-token.service';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly csrf = inject(CsrfTokenService);
  private initialization: Observable<AdminUser | null> | null = null;
  readonly session = inject(AuthSessionService);
  private readonly base = `${environment.apiUrl}/auth`;

  initialize(): Observable<AdminUser | null> {
    if (this.session.user()) return of(this.session.user());
    if (this.initialization) return this.initialization;
    this.initialization = this.http.get<AdminUser>(`${this.base}/me`).pipe(
      tap(user => this.session.set(user)),
      catchError(() => { this.session.clear(); return of(null); }),
      finalize(() => this.initialization = null),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.initialization;
  }

  login(request: LoginRequest): Observable<AdminUser> {
    return this.http.post<AdminUser>(`${this.base}/login`, request).pipe(tap(user => {
      this.session.set(user); this.csrf.clear();
    }));
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.base}/logout`, {}).pipe(
      tap(() => { this.session.clear(); this.csrf.clear(); }),
    );
  }

  providers(): Observable<AuthenticationProviders> {
    return this.http.get<AuthenticationProviders>(`${this.base}/providers`);
  }

  googleUrl(returnUrl: string): string {
    const safe = safeAdminReturnUrl(returnUrl);
    return `${this.base}/google?returnUrl=${encodeURIComponent(safe)}`;
  }

  hasPermission(permission: string): boolean {
    return this.session.user()?.permissions.includes(permission) ?? false;
  }
}
