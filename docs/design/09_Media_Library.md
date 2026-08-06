# 09 --- Media Library Specification

> **Status:** Approved Supporting Feature

This document defines the Media Library for the Portfolio Platform.

It supports both the Public Website and the Admin Portal by providing a
centralized location for images, documents, thumbnails, and downloadable
assets.

------------------------------------------------------------------------

# 1. Purpose

The Media Library is the single source of truth for reusable media
assets.

Goals:

-   Centralized asset management
-   Reusable media references
-   Consistent thumbnails
-   Future cloud storage support
-   Secure file organization

------------------------------------------------------------------------

# 2. Supported Media Types

-   Images (PNG, JPG, WEBP, SVG)
-   PDF documents
-   Icons
-   Cover images
-   Thumbnails

Future:

-   Video
-   Audio
-   ZIP packages

------------------------------------------------------------------------

# 3. Public Usage

Media may be used by:

-   Homepage
-   Projects
-   Visual Handbook
-   Series
-   Reading Paths
-   Profile

Public users never upload media.

------------------------------------------------------------------------

# 4. Admin Pages

Routes:

-   /admin/media
-   /admin/media/upload
-   /admin/media/collections

Capabilities:

-   Browse
-   Search
-   Filter
-   Upload (future)
-   Replace
-   Archive
-   Restore
-   View metadata

------------------------------------------------------------------------

# 5. Metadata

Each media item stores:

-   File name
-   Original name
-   MIME type
-   Size
-   Width
-   Height
-   Upload date
-   Uploader
-   Alt text
-   Collection
-   Usage references

------------------------------------------------------------------------

# 6. Collections

Examples:

-   Project Images
-   Infographics
-   Profile
-   Certifications
-   General Assets

------------------------------------------------------------------------

# 7. User Experience

Support:

-   Grid view
-   List view
-   Preview
-   Search
-   Filters
-   Pagination
-   Empty state

------------------------------------------------------------------------

# 8. Validation

Validate:

-   Supported file type
-   Maximum file size
-   Duplicate detection
-   Required metadata

------------------------------------------------------------------------

# 9. Responsive Design

Desktop: - Multi-column grid

Tablet: - Compact grid

Mobile: - Single-column cards

------------------------------------------------------------------------

# 10. Angular Structure

src/app/features/media/

Contains:

-   pages
-   components
-   data-access
-   models
-   routes

------------------------------------------------------------------------

# 11. API Expectations

Public:

-   GET media by identifier

Admin:

-   List
-   Upload (future)
-   Update metadata
-   Delete
-   Restore

------------------------------------------------------------------------

# 12. Storage Strategy

Development:

-   Local storage

Future:

-   Azure Blob Storage
-   CDN integration

Storage implementation is intentionally deferred from the first Projects
vertical slice.

------------------------------------------------------------------------

# 13. Testing

Unit:

-   Metadata validation
-   Duplicate detection

Integration:

-   Listing
-   Search
-   Filtering

UI:

-   Preview
-   Grid
-   Empty state

------------------------------------------------------------------------

# 14. Deferred

-   Image optimization
-   Automatic thumbnails
-   AI tagging
-   Virus scanning
-   Version history

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Media Library Specification

**Priority:** High
