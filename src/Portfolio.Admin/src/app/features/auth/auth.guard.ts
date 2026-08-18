import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthApiService } from './auth-api.service';
import { safeAdminReturnUrl } from './auth.models';

export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthApiService);
  const router = inject(Router);
  return auth.initialize().pipe(map(user => user
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: safeAdminReturnUrl(state.url) } })));
};

export const anonymousOnlyGuard: CanActivateFn = () => {
  const auth = inject(AuthApiService);
  const router = inject(Router);
  return auth.initialize().pipe(map(user => user ? router.createUrlTree(['/dashboard']) : true));
};
