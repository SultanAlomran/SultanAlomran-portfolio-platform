# 08 --- Series & Reading Paths Specification

> Status: Approved Supporting Feature

## Purpose

Series and Reading Paths organize related content into structured
learning journeys without changing the underlying content model.

------------------------------------------------------------------------

## Goals

-   Group related infographics
-   Build progressive learning paths
-   Increase content discoverability
-   Encourage sequential learning

------------------------------------------------------------------------

## Public Pages

### Series Listing

Route: `/series`

Features: - Search - Category filter - Difficulty filter - Pagination

### Series Details

Route: `/series/:slug`

Displays: - Cover - Description - Ordered items - Estimated reading
time - Continue learning CTA

### Reading Path

Route: `/reading-paths/:slug`

Displays: - Learning objective - Ordered modules - Progress overview
(future) - Related content

------------------------------------------------------------------------

## Admin

Routes:

-   /admin/series
-   /admin/series/new
-   /admin/series/{id}/edit
-   /admin/reading-paths

Capabilities:

-   Create
-   Edit
-   Publish
-   Archive
-   Soft delete
-   Restore
-   Reorder items
-   Manage relationships

------------------------------------------------------------------------

## Components

Reuse:

-   Series Card
-   Reading Path Card
-   Progress Timeline
-   Step List
-   Badge
-   Empty State
-   Skeleton Loader

------------------------------------------------------------------------

## UX Principles

-   Clear sequence
-   Visible progress
-   Minimal clicks
-   Consistent navigation

------------------------------------------------------------------------

## Responsive

Desktop: - Sidebar + content

Tablet: - Compact layout

Mobile: - Single-column flow

------------------------------------------------------------------------

## Angular Structure

src/app/features/series/

Contains:

-   pages
-   components
-   data-access
-   models
-   routes

------------------------------------------------------------------------

## API

Public:

-   GET /api/series
-   GET /api/series/{slug}
-   GET /api/reading-paths/{slug}

Admin:

-   CRUD
-   Publish
-   Archive
-   Restore
-   Reorder items

------------------------------------------------------------------------

## Validation

Required:

-   Title
-   Slug
-   Ordered items

Optional:

-   Description
-   Cover image

------------------------------------------------------------------------

## Testing

Unit: - Create series - Add/remove items - Validate positions

Integration: - Listing - Detail - Ordering - Pagination

UI: - Cards - Timeline - Empty state

------------------------------------------------------------------------

## Deferred

-   User progress
-   Certificates
-   Gamification
-   Recommendations

------------------------------------------------------------------------

## Approval

Status: Approved

Role: Series & Reading Paths Feature Specification

Priority: High
