# 03 --- Admin Portal Specification

> **Status:** Approved Foundation

This document defines the structure, navigation, workflows, and design
principles of the Portfolio Administration Portal.

It builds upon:

-   00_Master_Project_Specification.md
-   01_Design_System.md
-   02_Public_Website.md

------------------------------------------------------------------------

# 1. Purpose

The Admin Portal is the internal CMS used to manage all public content.

Goals:

-   Efficient content management
-   Consistent workflows
-   Reusable forms
-   Fast administration
-   Enterprise-grade usability

------------------------------------------------------------------------

# 2. Target Users

-   Portfolio Owner
-   Future Administrators

No public access.

------------------------------------------------------------------------

# 3. Technology

-   Angular
-   Metronic UI
-   ASP.NET Core Web API
-   SQL Server

------------------------------------------------------------------------

# 4. Navigation

-   Dashboard
-   Projects
-   Visual Handbook
-   Series
-   Categories
-   Technologies
-   Skills
-   Certifications
-   Media Library
-   Contact Messages
-   Profile
-   Settings

------------------------------------------------------------------------

# 5. Dashboard

Display:

-   Total Projects
-   Published Projects
-   Draft Projects
-   Infographics
-   Series
-   Contact Messages
-   Recent Activity
-   Quick Actions

------------------------------------------------------------------------

# 6. Module Pattern

Every module follows:

-   List
-   Create
-   Edit
-   Publish
-   Archive
-   Soft Delete
-   Restore (where supported)

------------------------------------------------------------------------

# 7. Standard List Page

Includes:

-   Search
-   Filters
-   Sorting
-   Pagination
-   Bulk Selection
-   Status Badges
-   Row Actions

------------------------------------------------------------------------

# 8. Standard Edit Form

Sections:

-   General Information
-   Content
-   Relationships
-   Media
-   Publishing
-   Preview
-   Save Actions

------------------------------------------------------------------------

# 9. Forms

All forms should support:

-   Validation
-   Unsaved change warning
-   Loading state
-   Success/Error notifications
-   Keyboard accessibility

------------------------------------------------------------------------

# 10. Media Strategy

Current scope:

-   Select existing MediaFile records

Deferred:

-   File uploads
-   Azure Blob Storage
-   Image processing

------------------------------------------------------------------------

# 11. Workflow

Draft

↓

Review

↓

Publish

↓

Archive

↓

Restore (optional)

------------------------------------------------------------------------

# 12. Design Principles

-   Use Metronic components consistently
-   Avoid custom admin styling unless necessary
-   Keep layouts clean and task-oriented

------------------------------------------------------------------------

# 13. Responsive Behaviour

Desktop-first.

Tablet support required.

Mobile support for basic management tasks.

------------------------------------------------------------------------

# 14. Security

Authorization will be implemented later.

Current implementation should clearly separate admin endpoints from
public endpoints.

Do not fake authentication.

------------------------------------------------------------------------

# 15. Notifications

Support:

-   Success
-   Warning
-   Error
-   Confirmation dialogs

------------------------------------------------------------------------

# 16. Reusable Components

-   Data Table
-   Toolbar
-   Search Box
-   Filter Panel
-   Form Sections
-   Media Picker
-   Confirmation Dialog
-   Empty State
-   Skeleton Loader

------------------------------------------------------------------------

# 17. Feature Organization

Each feature owns:

-   Pages
-   Components
-   Services
-   Models
-   Routes

Example:

features/projects/ - pages - components - data-access - models

------------------------------------------------------------------------

# 18. Future Modules

Designed to accommodate:

-   User Management
-   Audit Logs
-   Analytics
-   Notifications
-   Background Jobs
-   System Health

------------------------------------------------------------------------

# Approval

**Status:** Approved

**Role:** Admin Portal Specification

**Priority:** High
