# 02 --- Public Website Specification

> **Status:** Approved Foundation

This document defines the structure, navigation, page hierarchy, user
experience, and layout philosophy for the public website.

It builds on:

-   00_Master_Project_Specification.md
-   01_Design_System.md

------------------------------------------------------------------------

# 1. Purpose

The public website presents Sultan Alomran's work in a premium,
professional, and easy-to-navigate experience.

Goals:

-   Showcase projects
-   Highlight the Visual Handbook
-   Demonstrate engineering expertise
-   Encourage exploration
-   Make contacting easy

------------------------------------------------------------------------

# 2. User Journey

Home

↓

Projects

↓

Project Details

↓

Visual Handbook

↓

Series

↓

About

↓

Contact

The journey should feel natural with clear calls to action.

------------------------------------------------------------------------

# 3. Primary Navigation

-   Home
-   Projects
-   Visual Handbook
-   Series
-   About
-   Contact

Persistent actions:

-   Search
-   Theme (future)
-   LinkedIn
-   GitHub

------------------------------------------------------------------------

# 4. Footer

Include:

-   Quick Links
-   Social Links
-   Copyright
-   Contact
-   Technology Stack
-   Back to Top

------------------------------------------------------------------------

# 5. Public Routes

-   /
-   /projects
-   /projects/:slug
-   /visual-handbook
-   /visual-handbook/:slug
-   /series
-   /series/:slug
-   /about
-   /contact
-   /404

------------------------------------------------------------------------

# 6. Homepage Sections

1.  Hero
2.  Featured Projects
3.  Visual Handbook
4.  Skills & Technologies
5.  Experience Timeline
6.  Certifications
7.  Latest Content
8.  Call to Action
9.  Footer

------------------------------------------------------------------------

# 7. Projects

Listing page:

-   Search
-   Technology filters
-   Pagination
-   Sorting
-   Responsive cards

Details page:

-   Hero image
-   Description
-   Technologies
-   Gallery
-   External links
-   Related projects (future)

------------------------------------------------------------------------

# 8. Visual Handbook

Listing:

-   Categories
-   Search
-   Difficulty
-   Reading paths

Details:

-   Overview
-   Sections
-   Resources
-   Related content

------------------------------------------------------------------------

# 9. About

Include:

-   Biography
-   Career Timeline
-   Philosophy
-   Skills
-   Certifications
-   Resume download

------------------------------------------------------------------------

# 10. Contact

Simple contact page containing:

-   Contact form
-   Email
-   LinkedIn
-   GitHub

Future integrations remain out of scope.

------------------------------------------------------------------------

# 11. Layout Principles

-   Maximum content width: 1200px
-   Consistent spacing
-   Clear visual hierarchy
-   Reusable components only

------------------------------------------------------------------------

# 12. Responsive Behaviour

Desktop: - Multi-column layouts

Tablet: - Reduced columns

Mobile: - Single-column flow - Stacked navigation - Touch-friendly
spacing

------------------------------------------------------------------------

# 13. Loading States

Every major page should support:

-   Skeleton loading
-   Empty state
-   Error state
-   Success state

------------------------------------------------------------------------

# 14. SEO

Each page should define:

-   Title
-   Meta description
-   Open Graph image
-   Canonical URL

------------------------------------------------------------------------

# 15. Performance

Prefer:

-   Lazy-loaded images
-   Route-level lazy loading
-   Optimized assets
-   Minimal JavaScript

------------------------------------------------------------------------

# 16. Accessibility

Support:

-   Keyboard navigation
-   Focus indicators
-   Semantic headings
-   Accessible contrast
-   ARIA labels

------------------------------------------------------------------------

# 17. Angular Structure

Suggested feature organization:

src/app/features/

-   home
-   projects
-   visual-handbook
-   series
-   about
-   contact

Each feature owns:

-   pages
-   components
-   models
-   services
-   routes

------------------------------------------------------------------------

# 18. Future Modules

Designed to accommodate:

-   Blog
-   Search
-   Bookmarks
-   Analytics
-   Internationalization

without redesign.

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Public Website Specification

**Priority:** High
