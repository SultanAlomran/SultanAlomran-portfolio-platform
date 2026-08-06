# 06 --- Projects Feature Specification

> **Status:** Approved Vertical Slice

This document defines the complete Projects feature for the Portfolio
Platform.

It builds on:

-   00_Master_Project_Specification.md
-   01_Design_System.md
-   02_Public_Website.md
-   03_Admin_Portal.md
-   04_Reusable_Components.md
-   05_Homepage.md

------------------------------------------------------------------------

# 1. Purpose

Projects are the primary showcase of engineering work.

The feature provides an end-to-end experience for both public visitors
and administrators.

------------------------------------------------------------------------

# 2. Objectives

-   Showcase professional projects
-   Demonstrate technical capability
-   Support portfolio growth
-   Keep management simple
-   Reuse existing design components

------------------------------------------------------------------------

# 3. Public Pages

## Projects Listing

Route:

`/projects`

Features:

-   Search
-   Technology filters
-   Sort by newest or title
-   Pagination
-   Featured badges
-   Responsive cards

Only published, non-deleted projects are visible.

------------------------------------------------------------------------

## Project Details

Route:

`/projects/:slug`

Display:

-   Hero image
-   Title
-   Short description
-   Full description
-   Technology badges
-   Image gallery
-   External links
-   Live demo (optional)

Future enhancements remain out of scope.

------------------------------------------------------------------------

# 4. Admin Pages

Routes:

-   /admin/projects
-   /admin/projects/new
-   /admin/projects/{id}/edit

Capabilities:

-   Create
-   Edit
-   Publish
-   Archive
-   Soft Delete
-   Restore
-   Search
-   Filter
-   Pagination

------------------------------------------------------------------------

# 5. Form Fields

Supported fields:

-   Title
-   Slug
-   Short Description
-   Description
-   Thumbnail
-   Live URL
-   Technologies
-   Images
-   Links
-   Status

Media upload is deferred. Existing MediaFile references are used.

------------------------------------------------------------------------

# 6. Workflow

Draft

↓

Edit

↓

Publish

↓

Archive

↓

Restore (optional)

------------------------------------------------------------------------

# 7. User Experience

Visitors should reach project content in as few clicks as possible.

Navigation:

Home → Projects → Project Details

------------------------------------------------------------------------

# 8. Components

Reuse:

-   Project Card
-   Section Header
-   Search Box
-   Filter Panel
-   Technology Badge
-   Pagination
-   Empty State
-   Skeleton Loader

No duplicate components.

------------------------------------------------------------------------

# 9. Responsive Design

Desktop: - Multi-column cards

Tablet: - Reduced grid

Mobile: - Single-column cards

------------------------------------------------------------------------

# 10. Validation

Required:

-   Title
-   Slug
-   Short Description

Optional:

-   Description
-   Live URL
-   Gallery
-   Links

------------------------------------------------------------------------

# 11. Loading States

Provide:

-   Skeleton loading
-   Empty state
-   Error state

------------------------------------------------------------------------

# 12. Angular Structure

Suggested feature:

src/app/features/projects/

Contains:

-   pages
-   components
-   data-access
-   models
-   routes

------------------------------------------------------------------------

# 13. API Expectations

Public:

-   GET /api/projects
-   GET /api/projects/{slug}

Admin:

-   GET
-   POST
-   PUT
-   Publish
-   Archive
-   Delete
-   Restore

Authorization is implemented later.

------------------------------------------------------------------------

# 14. Testing

Unit:

-   Create
-   Update
-   Publish
-   Archive
-   Restore

Integration:

-   Public listing
-   Details
-   Filters
-   Pagination

UI:

-   Cards
-   Search
-   Forms
-   Empty state

------------------------------------------------------------------------

# 15. Deferred

-   Media upload
-   Azure Blob Storage
-   Analytics
-   Ratings
-   Comments
-   Related projects
-   Advanced SEO editor

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Projects Feature Specification

**Priority:** Very High
