# Admin authentication and Google sign-in

## Architecture decision

Portfolio Admin uses server-controlled, encrypted ASP.NET Core cookies. The browser never stores access or refresh tokens in `localStorage` or `sessionStorage`. The existing `User`, `Role`, `Permission`, `UserRole`, and `RolePermission` model remains authoritative; ASP.NET Identity tables are not introduced. `Microsoft.Extensions.Identity.Core` is used only for the supported `PasswordHasher<User>` implementation.

The application cookie is HttpOnly, essential, scoped to `/`, non-sliding, and secure in HTTPS environments. An unchecked **Remember me** produces a browser-session cookie containing an eight-hour authentication ticket. A checked value produces a persistent cookie capped at fourteen days. Logout signs out both application and temporary external cookies. The active user and Administrator role are revalidated against SQL Server at most every five minutes, so a disabled or de-authorized user loses access without waiting for the original ticket expiry.

`Session` remains visitor/analytics telemetry. `RefreshToken` is an unused earlier JWT-oriented domain concept. Neither is used for browser authentication, and neither table is removed by this slice.

## Reused and added persistence

The existing configured/migrated tables are reused: `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions`, `RefreshTokens`, `PasswordResetTokens`, `EmailVerificationTokens`, `Sessions`, and `AuditLogs`. Existing unique indexes cover user email/user name, role name, permission name, and both join pairs. The existing seed creates the `Administrator` role and the bounded `content.manage`, `settings.manage`, and `analytics.read` permissions; it deliberately creates no credential.

Google required one focused `UserExternalLogins` table because no safe provider-identity mapping existed. It stores only `UserId`, provider name, stable provider subject, an optional email snapshot, and creation time. `(Provider, ProviderSubject)` and `(UserId, Provider)` are unique. Provider access tokens and refresh tokens are never stored.

## Local flow and bootstrap

The public auth surface is:

- `GET /api/auth/csrf`
- `GET /api/auth/providers`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/auth/google`
- `GET /api/auth/google/callback`

All `/api/admin/**` endpoints require the `AdminOnly` policy, which requires an authenticated `Administrator` role. Unauthenticated requests return 401; authenticated requests lacking authorization return 403. Cookie middleware never redirects API requests to HTML.

The bootstrap is explicit, idempotent, and does not reset an existing password. Configure a password with user-secrets or environment variables, then run the command once:

```powershell
dotnet user-secrets set --project src/Portfolio.Api "AdminBootstrap:Email" "owner@example.com"
dotnet user-secrets set --project src/Portfolio.Api "AdminBootstrap:UserName" "portfolio.owner"
dotnet user-secrets set --project src/Portfolio.Api "AdminBootstrap:FullName" "Portfolio Owner"
dotnet user-secrets set --project src/Portfolio.Api "AdminBootstrap:Password" "<generate-a-strong-unique-password>"
dotnet run --project src/Portfolio.Api --launch-profile http -- --bootstrap-admin
```

Do not paste the real password into source, documentation, chat, or shell history on shared machines. Production provisioning should supply the same keys from the deployment secret store. The bootstrap requires at least fourteen password characters, hashes the password before persistence, assigns the existing role, and records a bounded audit event.

Local login uses one generic `Invalid email or password.` response for unknown email, incorrect password, inactive account, and missing Administrator authorization. Known-account failures, successful logins, disabled-account denials, logout, Google success/denial, and bootstrap are audited without passwords, cookies, Google tokens, or message bodies. The password endpoint is fixed-window limited to five attempts per source IP per minute.

Password reset and email-verification entities are modeled but no complete delivery workflow exists, so Login intentionally renders no dead **Forgot password** or registration controls. Public registration and MFA remain separate future security work.

## CSRF and CORS

Cookie-authenticated mutations use ASP.NET Core antiforgery. Angular obtains a request token from `/api/auth/csrf`; the complementary cookie is HttpOnly, and the interceptor sends the token in `X-CSRF-TOKEN`. The token cache is cleared after login/logout because antiforgery binds tokens to the current identity. Every mutating Admin endpoint group validates it. Tests exercise missing-token rejection.

CORS uses only configured Web/Admin origins, explicit methods/headers, and credentials. `AllowAnyOrigin` is never combined with credentials. Local ports are configured in `appsettings.json`; Preview origins are injected by Bicep. Preview uses `SameSite=None` plus HTTPS-only Secure cookies because API and Admin have different Container App hostnames. Production must explicitly configure its exact origins, Admin URL, HTTPS cookie policy, and durable data-protection key storage.

## Angular Admin

`features/auth` owns the small auth API/state layer, CSRF service, functional interceptor, guards, and lazy Login page. State is memory-only and is restored from `/api/auth/me`. Anonymous Admin routes redirect to `/login` with a validated local return path; authenticated visits to Login redirect to Dashboard. A 401 clears state and returns to Login, while a 403 opens the existing permission-denied page without a loop.

The Admin header shows the authenticated full name, role, email, initials, and working logout action. The Login layout selectively adapts Metronic v9.5 branded/classic sign-in spacing, input hierarchy, Google action, divider, and password visibility pattern from:

- `C:\Users\HP\Desktop\themeforest-JM1c1kIc-metronic-responsive-admin-dashboard-template\metronic-v9.5.0\metronic-tailwind-html-demos\dist\html\demo1\authentication\branded\sign-in.html`
- `C:\Users\HP\Desktop\themeforest-JM1c1kIc-metronic-responsive-admin-dashboard-template\metronic-v9.5.0\metronic-tailwind-html-demos\dist\html\demo1\authentication\classic\sign-in.html`
- `C:\Users\HP\Desktop\themeforest-JM1c1kIc-metronic-responsive-admin-dashboard-template\metronic-v9.5.0\metronic-tailwind-html-starter-kit\dist\assets\media\brand-logos\google.svg`

No template JavaScript, jQuery validation, unused bundle, or complete Metronic application is copied. The final Angular implementation uses Reactive Forms and the portfolio navy/violet tokens. Pure CSS reveals the exact phrases **Build with clarity.**, **Engineer for scale.**, and **Deliver with confidence.** once; `prefers-reduced-motion` renders all phrases statically and disables decorative motion.

## Google sign-in

Google uses the official ASP.NET Core Google handler. Google proves identity only. Authorization still requires a linked, active Portfolio `User` with the local `Administrator` role. Unknown, inactive, or non-Administrator Google identities are denied and no user or role is created automatically.

Create/configure Google manually:

1. In Google Cloud, create or select a project and configure the OAuth consent screen for the intended owner account.
2. Create an **OAuth client ID** of type **Web application**.
3. Add `http://localhost:5100` as a local authorized JavaScript origin if the Google console requests an origin.
4. Add the exact local authorized redirect URI `http://localhost:5100/signin-google`. `/signin-google` is the Google handler callback; `/api/auth/google/callback` is the application's post-handler completion endpoint and is not the Google Console redirect URI.
5. Store credentials server-side:

```powershell
dotnet user-secrets set --project src/Portfolio.Api "Authentication:Google:Enabled" "true"
dotnet user-secrets set --project src/Portfolio.Api "Authentication:Google:ClientId" "<client-id>"
dotnet user-secrets set --project src/Portfolio.Api "Authentication:Google:ClientSecret" "<client-secret>"
```

6. Obtain the approved account's stable Google OpenID Connect `sub` locally through the configured OAuth client/provider tooling. Do not share the ID token or client secret. Link that subject only through secure bootstrap configuration:

```powershell
dotnet user-secrets set --project src/Portfolio.Api "AdminBootstrap:GoogleSubject" "<stable-google-sub>"
dotnet user-secrets set --project src/Portfolio.Api "AdminBootstrap:GoogleEmail" "owner@example.com"
dotnet run --project src/Portfolio.Api --launch-profile http -- --bootstrap-admin
```

`GoogleEmail` is only a snapshot; authorization keys on the stable subject and local role. The server sets `SaveTokens = false`. Return URLs must be local Admin paths; absolute, protocol-relative, malformed, and backslash-based destinations fall back to `/dashboard`.

Real Google validation through a Dev Tunnel is not enabled by the current relative Admin `/api` proxy topology. Local email/password works through the private tunnel. For Google, use localhost unless a stable API callback has been registered and the Admin is explicitly configured to call that API origin with credentials. The Google Console callback would be `https://<stable-api-tunnel-host>/signin-google`, and `Authentication:AdminBaseUrl` must be the exact private Admin tunnel origin. Never make the tunnel anonymous merely to make OAuth work.

For Azure Preview, Google remains disabled unless that environment is deliberately given its own OAuth client and secrets. Preview currently bootstraps an isolated test Administrator from protected GitHub environment values `AZURE_PREVIEW_ADMIN_EMAIL` and `AZURE_PREVIEW_ADMIN_PASSWORD`; forks receive no credentials without the protected environment gate. Register a Preview callback only if Google validation is explicitly approved: `https://<preview-api-host>/signin-google`.

## Tests and evidence

Integration tests seed only deterministic test credentials and authenticate through `/csrf` plus `/login`. They prove local success/failure, Remember Me ticket behavior, logout, `/me`, 401, CSRF, linked/unknown/inactive Google behavior, protected Admin APIs, Quality/Test Analytics, and artifact previews.

Playwright keeps authenticated cookies in memory per worker; no storage-state file or real secret is committed. Standard auth coverage lives in `tests/playwright/admin/auth`, deterministic reduced-motion visual evidence in `tests/playwright/visual/auth`, and the optional local login/dashboard/logout recording in `tests/playwright/recording/auth`. Real Google UI is never automated in CI; Development-only TestMode uses a fixed server-configured subject and cannot run outside Development.

Quality telemetry and artifact rules are unchanged: SQL stores normalized metadata only, while screenshots, video, trace, and HTML reports remain external artifacts.

## SignalR readiness for issue #33

A future Admin SignalR hub should require the same `AdminOnly` policy. The browser will send the HttpOnly application cookie during negotiate/connect when the approved Admin origin and credentials configuration are used. The hub must not accept client-selected privileged groups and should publish only minimal ContactMessage notification DTOs. No hub or Contact implementation is added by this authentication slice; resume #33 only after this branch is reviewed, merged to `dev`, and its paused feature branch is recreated/rebased safely from updated `dev`.
