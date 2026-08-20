# Contact Messages, Notifications & SignalR Real-Time Stream

## Purpose and Scope

This feature implements the complete public-to-admin communication pipeline:
1. Public visitors can submit structured inquiries via `/contact` or the persistent floating contact drawer across `Portfolio.Web`.
2. Messages are durably persisted to SQL Server before any external side effects occur.
3. Connected Admin sessions receive instantaneous real-time awareness via SignalR (`/hubs/notifications`) without polling.
4. Background notification channels (Email and WhatsApp) are dispatched asynchronously through an isolated in-memory channel worker.
5. Administrators can manage messages in `Portfolio.Admin` (`/messages`) with full status transitions (New, Read, Archived), search, filtering, and direct email reply.
6. Administrators can review durable engagement insights via `/analytics/messages` and manage dispatch preferences via `/settings/notifications`.

## Architecture and Data Flow

```text
Public Visitor (Page or Floating Drawer)
      ↓
[Portfolio.Web (ContactFormComponent)]
      ↓ (POST /api/contact-messages)
[Portfolio.Api (ContactEndpoints)]
      ↓
[Portfolio.Infrastructure (ContactService)]
      ↓ (Transactional Boundary)
[SQL Server: ContactMessages table (Durable Commit)]
      ↓
 ┌────┴────────────────────────────────┐
 │                                     │
 ▼                                     ▼
[SignalR Hub (/hubs/notifications)]  [System.Threading.Channels (InMemoryNotificationQueue)]
      ↓ (WebSocket / Push)                  ↓ (Asynchronous Consumer)
[Portfolio.Admin (Live Toast + Badge)] [NotificationBackgroundWorker]
                                            ↓
                                  ┌─────────┴─────────┐
                                  ▼                   ▼
                         [Email Service]     [WhatsApp Service]
                         (Azure CS / Deter)  (Meta Cloud / Deter)
```

## Security & Reliability Boundaries

1. **Transactional Integrity:** The `ContactMessage` record is persisted and committed to SQL Server before any notification dispatch is attempted. If external notification channels fail, the visitor's message is never lost.
2. **Non-Blocking Public Endpoint:** Network calls to external Email, WhatsApp, or SignalR clients never block the public API response.
3. **Admin Authorization & CSRF:**
   - Admin management endpoints (`/api/admin/contact-messages/*`, `/api/admin/contact-messages/analytics`, `/api/admin/settings/notifications`) require the `AdminAuthorization.Policy` cookie session.
   - Mutating operations (`PATCH /read`, `PATCH /unread`, `PATCH /archive`, `PUT /settings/notifications`) require valid CSRF tokens via `AntiforgeryEndpointFilter`.
   - SignalR Hub (`/hubs/notifications`) requires `[Authorize(Policy = AdminAuthorization.Policy)]`.
4. **Rate Limiting:** Public contact submission is protected with a dedicated rate-limiter partition (`contact-submission`, 5 requests/minute per IP) returning HTTP 429 upon excess.
5. **Channel Isolation:** Email and WhatsApp dispatch operate under independent try/catch blocks within `NotificationBackgroundWorker`; failure of one channel does not impact or delay the other.

## Configuration & Providers

Configuration is located under the `Notifications` section in `appsettings.json` / environment variables:

```json
{
  "Notifications": {
    "AdminBaseUrl": "http://localhost:4300",
    "AdminToastEnabled": true,
    "Email": {
      "Enabled": true,
      "Provider": "Deterministic",
      "FromAddress": "DoNotReply@sultanomran.com",
      "FromName": "Sultan Portfolio",
      "RecipientEmail": "sultan.alomran.9@gmail.com",
      "ConnectionString": ""
    },
    "WhatsApp": {
      "Enabled": true,
      "Provider": "Deterministic",
      "RecipientPhoneNumber": "+966508334411",
      "ApiUrl": "https://graph.facebook.com/v21.0",
      "PhoneNumberId": "",
      "AccessToken": ""
    }
  }
}
```

### Provider Options:
- **Email:**
  - `Deterministic` (default for CI/automated testing and local development): logs dispatched emails without making external network calls.
  - `AzureCommunicationServices`: uses the official `Azure.Communication.Email` SDK to send responsive HTML notifications.
- **WhatsApp:**
  - `Deterministic` (default for CI/automated testing and local development): logs dispatched WhatsApp alerts.
  - `MetaCloud`: calls the official Meta WhatsApp Business Cloud API (`/messages` endpoint) with Bearer token authentication.

## User Experience

### Public Frontend (`Portfolio.Web`):
- **Design System & Contrast:** High contrast WCAG AA/AAA compliant styling (dark slate text `#0f172a`, crisp white input fields `#ffffff` with slate borders `#cbd5e1`, distinct violet accents `#7c3aed`).
- **Reusable Contact Form:** Single shared `ContactFormComponent` used on `/contact` and within modal drawers.
- **Persistent Floating Action:** Bottom-left anchored "Contact Sultan" button (non-overlapping with bottom-right "Ask Portfolio").
- **Direct Drawer / Modal:** Opened via navbar "Contact Me" or floating action, with complete focus management and Escape key dismissal.
- **Accessible Validation:** Real-time form validation with aria-invalid and aria-describedby attributes.
- **Success View:** Animated checkmark (respecting `prefers-reduced-motion`) and delivery status indicators.

### Admin Portal (`Portfolio.Admin`):
- **SignalR Real-Time Stream:** Live unread counter and non-intrusive floating toast alerts for incoming messages.
- **Messages Inbox (`/messages`):** Master-detail split-pane inbox with tabs (`All`, `New`, `Read`, `Archived`), instant search, and email reply.
- **Message Analytics (`/analytics/messages`):** Real-time aggregate metrics (Total Inquiries, New/Unread, Read, Archived, This Month, Avg Response Time), 30-day activity volume trend, and top inquired subjects.
- **Notification Settings (`/settings/notifications`):** Interactive channel toggles for Email, WhatsApp, and Admin Toasts persisted to `SiteSettings`.
