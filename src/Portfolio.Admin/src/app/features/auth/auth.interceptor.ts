import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthSessionService } from './auth-session.service';
import { CsrfTokenService } from './csrf-token.service';
import { safeAdminReturnUrl } from './auth.models';

const safeMethods = new Set(['GET', 'HEAD', 'OPTIONS', 'TRACE']);

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const session = inject(AuthSessionService);
  const csrf = inject(CsrfTokenService);
  const apiRoot = new URL(environment.apiUrl, globalThis.location.origin).href;
  if (!new URL(request.url, globalThis.location.origin).href.startsWith(apiRoot)) return next(request);

  const send = (token?: string) => next(request.clone({
    withCredentials: true,
    setHeaders: token ? { 'X-CSRF-TOKEN': token } : {},
  })).pipe(catchError((error: unknown) => {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 400 && `${error.error?.title ?? ''}`.includes('security token')) csrf.clear();
      const authRequest = request.url.includes('/auth/');
      if (error.status === 401 && !authRequest) {
        session.clear();
        csrf.clear();
        const returnUrl = safeAdminReturnUrl(router.url);
        void router.navigate(['/login'], { queryParams: { returnUrl, reason: 'expired' } });
      } else if (error.status === 403 && !authRequest) {
        void router.navigate(['/permission-denied']);
      }
    }
    return throwError(() => error);
  }));

  return safeMethods.has(request.method.toUpperCase())
    ? send()
    : csrf.ensure().pipe(switchMap(token => send(token)));
};
