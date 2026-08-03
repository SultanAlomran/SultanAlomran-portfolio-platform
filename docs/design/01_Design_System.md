# 01 --- Design System

> **Status:** Approved Foundation

This document defines the complete visual language for the Sultan
Alomran Portfolio Platform.

It expands the decisions defined in
**00_Master_Project_Specification.md** and serves as the implementation
contract between Figma and Angular.

------------------------------------------------------------------------

# 1. Purpose

The Design System provides a reusable visual language focused on:

-   Consistency
-   Scalability
-   Accessibility
-   Maintainability
-   Faster development

------------------------------------------------------------------------

# 2. Design Philosophy

The interface should feel:

-   Premium
-   Modern
-   Technical
-   Elegant
-   Spacious

Avoid:

-   Visual clutter
-   Excessive gradients
-   Heavy shadows
-   Inconsistent spacing
-   Decorative effects

------------------------------------------------------------------------

# 3. Color System

## Brand

-   Primary: Purple
-   Secondary: Indigo
-   Accent: Blue

## Neutral

-   Background: Dark Navy
-   Surface: White
-   Secondary Surface: Light Gray
-   Border: Soft Gray
-   Primary Text: Slate
-   Secondary Text: Gray

## Semantic

-   Success: Green
-   Warning: Amber
-   Danger: Red
-   Information: Blue

------------------------------------------------------------------------

# 4. Tailwind Mapping

Every visual token should map cleanly to Tailwind utilities.

Examples:

-   `bg-primary`
-   `text-primary`
-   `border-primary`
-   `rounded-xl`
-   `shadow-md`
-   `gap-8`
-   `p-8`

------------------------------------------------------------------------

# 5. Typography

Primary font:

**Inter**

Hierarchy:

-   Hero: 56--72px
-   H1: 48px
-   H2: 36px
-   H3: 28px
-   H4: 24px
-   Body: 16px
-   Small: 14px
-   Caption: 12px

Weights:

-   400
-   500
-   600
-   700

------------------------------------------------------------------------

# 6. Spacing

Base unit: **8px**

Preferred scale:

8, 16, 24, 32, 40, 48, 64, 80, 96, 120

------------------------------------------------------------------------

# 7. Radius

-   Small: 8px
-   Medium: 12px
-   Large: 20px
-   Hero: 28px

------------------------------------------------------------------------

# 8. Shadows

-   Small: Inputs
-   Medium: Cards
-   Large: Dialogs

Keep shadows subtle.

------------------------------------------------------------------------

# 9. Layout

Container:

-   Max Width: 1280px
-   Content Width: 1200px
-   Horizontal Padding: 24px

Grid:

-   Desktop: 12 Columns
-   Tablet: 8 Columns
-   Mobile: 4 Columns

------------------------------------------------------------------------

# 10. Buttons

Variants:

-   Primary
-   Secondary
-   Ghost
-   Danger

Support:

-   Hover
-   Focus
-   Disabled
-   Loading

------------------------------------------------------------------------

# 11. Forms

Shared styling for:

-   Text
-   Textarea
-   Select
-   Search
-   Checkbox
-   Radio
-   Toggle
-   File Upload

------------------------------------------------------------------------

# 12. Cards

Reusable cards include:

-   Project Card
-   Infographic Card
-   Series Card
-   Technology Card
-   Statistic Card
-   Certification Card

All cards share spacing, radius, elevation, and hover behavior.

------------------------------------------------------------------------

# 13. Badges

-   Technology
-   Difficulty
-   Category
-   Status

------------------------------------------------------------------------

# 14. Icons

Use one icon family consistently.

Sizes:

16, 20, 24, 32

------------------------------------------------------------------------

# 15. Images

Preferred ratios:

-   16:9
-   4:3
-   1:1

Use lazy loading.

------------------------------------------------------------------------

# 16. Motion

Duration:

150--300ms

Effects:

-   Fade
-   Slide
-   Scale
-   Subtle lift

Animations should communicate state.

------------------------------------------------------------------------

# 17. Responsive Breakpoints

-   Mobile: 0--639px
-   Tablet: 640--1023px
-   Desktop: 1024--1439px
-   Large Desktop: 1440px+

------------------------------------------------------------------------

# 18. Accessibility

-   WCAG AA contrast
-   Keyboard navigation
-   Focus states
-   Semantic HTML
-   ARIA labels
-   Reduced motion support

------------------------------------------------------------------------

# 19. Core Reusable Components

-   Primary Button
-   Section Header
-   Project Card
-   Infographic Card
-   Technology Badge
-   Statistic Card
-   Search Box
-   Filter Panel
-   Pagination
-   Dialog
-   Drawer
-   Skeleton
-   Empty State

------------------------------------------------------------------------

# 20. Figma Requirements

Use:

-   Variables
-   Auto Layout
-   Components
-   Variants
-   Color Styles
-   Text Styles
-   Responsive Constraints

------------------------------------------------------------------------

# 21. Angular Mapping

Each design component maps directly to an Angular component.

Examples:

-   SectionHeaderComponent
-   PrimaryButtonComponent
-   ProjectCardComponent
-   TechnologyBadgeComponent
-   StatCardComponent
-   EmptyStateComponent

------------------------------------------------------------------------

# 22. Future Expansion

The system should naturally support:

-   Blog
-   Search
-   Bookmarks
-   Reading Progress
-   Analytics
-   Dark Mode

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Design Language

**Priority:** High
