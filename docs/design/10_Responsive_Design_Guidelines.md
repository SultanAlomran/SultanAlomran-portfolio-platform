# 10 --- Responsive Design Guidelines

> **Status:** Approved Design Standard

This document defines how the Portfolio Platform adapts across desktop,
tablet, and mobile devices.

It applies to:

-   Public Website
-   Admin Portal
-   Future modules

------------------------------------------------------------------------

# 1. Objectives

-   Excellent usability on every screen size
-   Consistent spacing
-   Predictable layouts
-   Accessibility
-   Performance

------------------------------------------------------------------------

# 2. Breakpoints

Recommended breakpoints:

  Device          Width
  --------------- --------------
  Mobile          \< 640px
  Small Tablet    640--767px
  Tablet          768--1023px
  Laptop          1024--1279px
  Desktop         1280--1535px
  Large Desktop   ≥1536px

Tailwind breakpoints should be used consistently.

------------------------------------------------------------------------

# 3. Layout Rules

Desktop:

-   Multi-column layouts
-   Wide content containers
-   Rich spacing

Tablet:

-   Reduced columns
-   Simplified navigation

Mobile:

-   Single-column flow
-   Larger touch targets
-   Simplified interactions

------------------------------------------------------------------------

# 4. Containers

Maximum content width:

1200--1280px

Standard horizontal padding:

-   Mobile: 16px
-   Tablet: 24px
-   Desktop: 32px

------------------------------------------------------------------------

# 5. Grid System

Use CSS Grid where appropriate.

Examples:

-   Project cards
-   Infographic cards
-   Certification cards

Avoid unnecessary nested grids.

------------------------------------------------------------------------

# 6. Navigation

Desktop:

-   Full navigation bar

Tablet:

-   Compact navigation

Mobile:

-   Drawer / hamburger menu

Sticky header remains available.

------------------------------------------------------------------------

# 7. Cards

Cards should:

-   Expand naturally
-   Maintain consistent spacing
-   Stack vertically on smaller screens

------------------------------------------------------------------------

# 8. Images

Rules:

-   Responsive sizing
-   Maintain aspect ratio
-   Lazy loading
-   Prevent layout shifts

------------------------------------------------------------------------

# 9. Typography

Scale headings proportionally.

Body text should remain readable without zooming.

Maintain adequate line height.

------------------------------------------------------------------------

# 10. Buttons

Minimum touch size:

44 × 44 px

Support:

-   Hover
-   Focus
-   Active
-   Disabled

------------------------------------------------------------------------

# 11. Forms

Forms should:

-   Collapse gracefully
-   Use full-width controls on mobile
-   Preserve validation messages

------------------------------------------------------------------------

# 12. Tables

Prefer:

-   Responsive tables
-   Horizontal scrolling where necessary
-   Card layouts for very small screens

------------------------------------------------------------------------

# 13. Performance

Prioritize:

-   Lazy loading
-   Code splitting
-   Optimized images
-   Deferred resources

------------------------------------------------------------------------

# 14. Accessibility

Support:

-   Keyboard navigation
-   Screen readers
-   WCAG AA contrast
-   Visible focus indicators

------------------------------------------------------------------------

# 15. Testing

Verify:

-   Desktop
-   Tablet
-   Mobile
-   Orientation changes
-   High zoom levels

------------------------------------------------------------------------

# 16. Future Enhancements

Deferred:

-   Container queries
-   Adaptive dashboards
-   Foldable device layouts

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Responsive Design Guidelines

**Priority:** High
