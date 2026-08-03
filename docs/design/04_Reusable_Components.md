# 04 --- Reusable Components Specification

> **Status:** Approved Foundation

This document defines the reusable UI component library for the
Portfolio Platform.

It extends:

-   00_Master_Project_Specification.md
-   01_Design_System.md
-   02_Public_Website.md
-   03_Admin_Portal.md

------------------------------------------------------------------------

# 1. Purpose

Every screen must be assembled from reusable components rather than
creating one-off UI.

Goals:

-   Consistency
-   Reusability
-   Faster development
-   Easier maintenance
-   Better UX

------------------------------------------------------------------------

# 2. Principles

Each component should be:

-   Reusable
-   Accessible
-   Responsive
-   Configurable
-   Easy to test

Avoid duplicate implementations.

------------------------------------------------------------------------

# 3. Buttons

Variants:

-   Primary
-   Secondary
-   Ghost
-   Danger
-   Icon

States:

-   Default
-   Hover
-   Focus
-   Disabled
-   Loading

------------------------------------------------------------------------

# 4. Cards

Shared cards:

-   Project Card
-   Infographic Card
-   Series Card
-   Technology Card
-   Statistic Card
-   Certification Card

Common properties:

-   Image
-   Title
-   Description
-   Actions
-   Hover animation

------------------------------------------------------------------------

# 5. Navigation

Components:

-   Header
-   Navigation Bar
-   Mobile Menu
-   Breadcrumb
-   Footer

------------------------------------------------------------------------

# 6. Forms

Reusable controls:

-   Text Input
-   Textarea
-   Select
-   Search
-   Checkbox
-   Radio
-   Toggle
-   Date Picker
-   File Selector

------------------------------------------------------------------------

# 7. Tables

Support:

-   Sorting
-   Filtering
-   Pagination
-   Empty State
-   Bulk Selection

------------------------------------------------------------------------

# 8. Feedback Components

-   Alert
-   Toast
-   Confirmation Dialog
-   Progress Indicator
-   Loading Spinner
-   Skeleton Loader

------------------------------------------------------------------------

# 9. Content Components

-   Section Header
-   Hero Banner
-   Timeline
-   Badge
-   Tag
-   Technology Chip
-   Statistic Widget

------------------------------------------------------------------------

# 10. Search & Filtering

Reusable:

-   Search Box
-   Filter Panel
-   Sort Selector
-   Pagination

------------------------------------------------------------------------

# 11. Media Components

-   Image Gallery
-   Image Preview
-   Thumbnail
-   Lightbox (future)

------------------------------------------------------------------------

# 12. Empty & Error States

Provide reusable components for:

-   No Data
-   No Search Results
-   Error
-   Access Denied
-   Loading

------------------------------------------------------------------------

# 13. Responsive Rules

Every component must support:

-   Desktop
-   Tablet
-   Mobile

without custom redesign.

------------------------------------------------------------------------

# 14. Accessibility

All components should provide:

-   Keyboard support
-   Focus styles
-   ARIA labels where appropriate
-   Sufficient contrast

------------------------------------------------------------------------

# 15. Angular Mapping

Recommended structure:

src/app/shared/components/

Each component owns:

-   HTML
-   TypeScript
-   Styles
-   Tests

Feature-specific components remain inside their feature.

------------------------------------------------------------------------

# 16. Figma Mapping

Each reusable component should exist once in Figma using:

-   Auto Layout
-   Variants
-   Variables
-   Component Sets

------------------------------------------------------------------------

# 17. Naming Convention

Examples:

-   PrimaryButtonComponent
-   ProjectCardComponent
-   SectionHeaderComponent
-   EmptyStateComponent
-   SkeletonLoaderComponent
-   TechnologyBadgeComponent

Use consistent naming across Figma and Angular.

------------------------------------------------------------------------

# 18. Future Expansion

Support future additions without redesign, including:

-   Charts
-   Notifications
-   Dashboards
-   Rich Editors
-   Media Upload

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Reusable Components Specification

**Priority:** High
