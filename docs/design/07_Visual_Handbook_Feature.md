# 07 --- Visual Handbook Feature Specification

> **Status:** Approved Vertical Slice

This document defines the Visual Handbook feature for the Portfolio
Platform.

It builds upon:

-   00_Master_Project_Specification.md
-   01_Design_System.md
-   02_Public_Website.md
-   03_Admin_Portal.md
-   04_Reusable_Components.md
-   05_Homepage.md
-   06_Projects_Feature.md

------------------------------------------------------------------------

# 1. Purpose

The Visual Handbook is the educational hub of the platform.

It presents technical knowledge through high-quality infographics,
structured learning paths, and categorized content.

------------------------------------------------------------------------

# 2. Goals

-   Share technical knowledge
-   Build credibility
-   Encourage continuous learning
-   Organize content into reusable collections
-   Support future growth

------------------------------------------------------------------------

# 3. Public Pages

## Visual Handbook

Route:

`/visual-handbook`

Features:

-   Search
-   Categories
-   Difficulty filter
-   Reading paths
-   Latest content
-   Pagination

------------------------------------------------------------------------

## Infographic Details

Route:

`/visual-handbook/:slug`

Display:

-   Cover image
-   Title
-   Category
-   Difficulty
-   Description
-   Sections
-   Resources
-   Code examples
-   Related infographics

------------------------------------------------------------------------

# 4. Admin Pages

Routes:

-   /admin/visual-handbook
-   /admin/visual-handbook/new
-   /admin/visual-handbook/{id}/edit

Capabilities:

-   Create
-   Edit
-   Publish
-   Archive
-   Soft Delete
-   Restore
-   Categorize
-   Tag
-   Manage sections
-   Attach resources

------------------------------------------------------------------------

# 5. Content Structure

Each infographic may contain:

-   Title
-   Slug
-   Category
-   Difficulty
-   Summary
-   Sections
-   Code Examples
-   Resources
-   Tags

------------------------------------------------------------------------

# 6. Reading Experience

Navigation should support:

Overview

↓

Infographic

↓

Related Content

↓

Series / Reading Path

------------------------------------------------------------------------

# 7. Components

Reuse:

-   Infographic Card
-   Category Badge
-   Difficulty Badge
-   Section Header
-   Resource List
-   Code Block
-   Empty State
-   Skeleton Loader
-   Pagination

------------------------------------------------------------------------

# 8. Responsive Design

Desktop: - Grid layout

Tablet: - Reduced columns

Mobile: - Single-column layout

------------------------------------------------------------------------

# 9. Validation

Required:

-   Title
-   Slug
-   Category
-   Summary

Optional:

-   Resources
-   Code examples
-   Images

------------------------------------------------------------------------

# 10. Angular Structure

Suggested feature:

src/app/features/visual-handbook/

Contains:

-   pages
-   components
-   data-access
-   models
-   routes

------------------------------------------------------------------------

# 11. API Expectations

Public:

-   GET /api/visual-handbook
-   GET /api/visual-handbook/{slug}

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

# 12. Testing

Unit:

-   Create
-   Update
-   Publish
-   Archive

Integration:

-   Listing
-   Details
-   Filters
-   Pagination

UI:

-   Cards
-   Detail page
-   Search
-   Empty state

------------------------------------------------------------------------

# 13. Deferred

-   PDF generation
-   Bookmarks
-   Reading progress
-   User ratings
-   Comments
-   AI recommendations
-   Offline support

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Visual Handbook Feature Specification

**Priority:** Very High
