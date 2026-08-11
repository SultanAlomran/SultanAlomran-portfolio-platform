# Media Library

## Purpose and ownership

The Media Library is the canonical file-and-file-metadata capability. `MediaFile` is reused by Projects, Infographics, collections, and future content; those entities continue to own their business data. SQL Server contains metadata and references only. Binary bytes are never placed in SQL Server.

## Storage

Development uses the `IMediaStorage` abstraction's local provider. Files are written below the configured `Media:LocalPath`, served at `/media/*`, survive application restarts, and the runtime directory is ignored by Git. Physical names are generated GUID keys partitioned by year/month; original filenames are metadata only. Application code does not use filesystem APIs.

Azure Container Apps local filesystems are **not durable storage** and must not be used for deployed media. No Azure resource is provisioned by this slice. The proposed durable implementation is one UAE North StorageV2 account (Standard LRS), one private `media` blob container, Container App system-assigned identity with **Storage Blob Data Contributor**, and application-generated read URLs (or an API read endpoint). PR blobs should use a `pr-{number}/` prefix and be removed with preview cleanup; Production uses an isolated account or container. Shared keys and public container access remain disabled. Consumption is expected to be well below USD 1/month at portfolio scale, but current Azure pricing must be confirmed before approval/provisioning.

## Validation and lifecycle

Supported files are PNG, JPEG/JPG, WebP (10 MB maximum), and PDF (20 MB maximum). Server validation checks the allow-listed extension, byte size, and magic-byte signature rather than trusting browser MIME metadata. Uploads use `multipart/form-data`. A failed metadata write triggers best-effort binary cleanup. Storage keys cannot traverse directories or overwrite another object.

The Admin list exposes referenced/unreferenced filtering. Delete checks Project thumbnails/images, Infographic cover/main/PDF references, and Media Collection items, returning HTTP 409 for referenced media. Orphans are never automatically deleted. The existing collections model is preserved without adding a separate collection-management surface.

## Surfaces

- Admin route: `/media` (`/media-library` redirects for compatibility).
- API: `GET/POST /api/admin/media`, `GET/DELETE /api/admin/media/{id}`.
- Local binary reads: `/media/{generated-key}`.
- OpenAPI and Scalar use the existing Microsoft ASP.NET Core stack and document multipart upload; no Swagger tooling is introduced.

The Admin screen provides summary metrics, search/type/usage filters, responsive previews, multiple-file selection, feedback, and deletion protection. `MediaPickerComponent` is content-agnostic and supports image, PDF, or unrestricted single selection. The Infographic Media & Files step uses it for cover, main-image, and PDF selection, as well as direct upload, preview/open, replacement, and reference removal. Existing Infographic foreign keys and Project `MediaFile` relationships remain authoritative. Infographics without media retain their public fallback visuals; the CV remains a static public document.

## Security, testing, and deferred work

Uploads are untrusted. Filename metadata is sanitized with `Path.GetFileName`; only generated keys reach storage. Server validation is authoritative and internal filesystem paths are never returned. Admin authentication remains an existing platform limitation and is not simulated here.

Focused integration and Playwright coverage exercises image/PDF upload, signature validation, filters, previews, picker selection, reference conflicts, orphan deletion, Infographic selection, and public rendering. Visual and full-recording specifications live under the existing `visual/media` and `recording/media` conventions, so normalized telemetry remains compatible with `feature=media` and Full Recording mode without changing Test Analytics.

Deferred: Azure Blob provider and Bicep after explicit approval, richer usage labels, image dimensions, Project wizard picker adoption, automatic orphan cleanup, collections administration, and Series/Reading Paths.
