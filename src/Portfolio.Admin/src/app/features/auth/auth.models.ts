export interface AdminUser {
  id: string;
  fullName: string;
  email: string;
  roles: string[];
  permissions: string[];
  provider: 'Local' | 'Google' | string;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface AuthenticationProviders {
  google: boolean;
}

export interface CsrfTokenResponse {
  token: string;
  headerName: string;
}

export function safeAdminReturnUrl(value: string | null | undefined): string {
  if (!value || !value.startsWith('/') || value.startsWith('//')) return '/dashboard';
  try {
    const parsed = new URL(value, globalThis.location.origin);
    return parsed.origin === globalThis.location.origin ? `${parsed.pathname}${parsed.search}${parsed.hash}` : '/dashboard';
  } catch { return '/dashboard'; }
}
