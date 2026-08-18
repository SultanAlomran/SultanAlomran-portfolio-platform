import { HttpBackend, HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { finalize, map, Observable, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CsrfTokenResponse } from './auth.models';

@Injectable({ providedIn: 'root' })
export class CsrfTokenService {
  private readonly http = new HttpClient(inject(HttpBackend));
  private token: string | null = null;
  private pending: Observable<string> | null = null;

  ensure(): Observable<string> {
    if (this.token) return of(this.token);
    if (this.pending) return this.pending;
    this.pending = this.http.get<CsrfTokenResponse>(`${environment.apiUrl}/auth/csrf`, { withCredentials: true }).pipe(
      map(response => response.token),
      tap(token => this.token = token),
      finalize(() => this.pending = null),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.pending;
  }

  clear(): void {
    this.token = null;
    this.pending = null;
  }
}
