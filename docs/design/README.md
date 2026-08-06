# Sultan Alomran Portfolio Platform — Design Documentation

> **Document role:** Entry point and navigation guide  
> **Status:** Active planning document  
> **Version:** 0.1  
> **Last updated:** 2026-08-03  
> **Owner:** Sultan Alomran

## 1. Purpose

This folder contains the design documentation for the **Sultan Alomran Portfolio Platform**.

The documents guide:

- Figma design generation and refinement;
- public website and admin dashboard UX decisions;
- reusable component and design-token creation;
- later Angular and ASP.NET Core implementation;
- design reviews, approvals, and controlled changes.

This `README.md` is the starting point. It explains how the documentation set is organized, but it does not replace the detailed specifications.

## 2. Product Summary

The platform is a premium personal portfolio and engineering showcase built around two primary pillars:

1. **Projects and Case Studies** — enterprise work, engineering decisions, implementation approaches, and outcomes that are safe to publish.
2. **Visual Handbook** — technical infographics, visual guides, series, and structured reading paths.

The platform also presents Sultan's professional identity, experience, technologies, certifications, engineering philosophy, timeline, and contact options.

It is **not** intended to be:

- a learning-management system;
- a social network;
- a generic blog template;
- a generic SaaS dashboard;
- a one-page-only portfolio.

## 3. Solution Context

| Application | Technology | Design direction |
| --- | --- | --- |
| `Portfolio.Web` | Angular + Tailwind CSS | Custom premium public experience |
| `Portfolio.Admin` | Angular + Metronic | Efficient content-management interface |
| `Portfolio.Api` | ASP.NET Core Web API + EF Core + SQL Server | API and persistence layer supporting both front ends |
| `Portfolio.Shared` | Shared contracts/DTOs where appropriate | Consistent application contracts |

The public website and admin dashboard belong to the same product, but they have different users and visual responsibilities. They should share product language and core data concepts without forcing the public visual style onto Metronic or making the public site look like an admin template.

## 4. Documentation Map

Read the files in the following order when starting the project. Open only the specialized documents needed for a focused task after the foundation is understood.

| Order | File | Responsibility |
| ---: | --- | --- |
| 0 | `README.md` | Entry point, document map, authority rules, and workflow |
| 1 | `00_Master_Project_Specification.md` | Product vision, approved scope, design direction, and main implementation context |
| 2 | `01_Design_System.md` | Colors, typography, spacing, grids, radii, elevation, imagery, icons, and design tokens |
| 3 | `02_Public_Website.md` | Public routes, page structures, section requirements, content hierarchy, and public user flows |
| 4 | `03_Admin_Portal.md` | Admin routes, workflows, Metronic usage, content-management screens, and operational states |
| 5 | `04_Reusable_Components.md` | Reusable public and admin components, variants, states, naming, and composition rules |
| 6 | `05_Homepage.md` | Homepage structure and experience |
| 7 | `06_Projects_Feature.md` | Projects vertical-slice UX and page requirements |
| 8 | `07_Visual_Handbook_Feature.md` | Visual Handbook UX and page requirements |
| 9 | `08_Series_Reading_Paths.md` | Series and reading-path experiences |

## 5. Source-of-Truth Order

When two design documents appear to conflict, use this order:

1. the latest explicitly approved design decision;
2. `00_Master_Project_Specification.md`;
3. the relevant specialized design document;
4. approved Figma frames and components;
5. reference images, moodboards, and inspiration;
6. assumptions made during design or implementation.

If a conflict remains unresolved, stop and request a decision. Do not silently choose one interpretation.

## 6. Current Approved Direction

- Premium, modern, and professional.
- Strong enough for a senior enterprise software engineer.
- Projects and the Visual Handbook remain the strongest content pillars.
- Dark navy/slate hero area.
- Violet as the primary brand color.
- Indigo as a supporting accent.
- White content surfaces and slate text.
- Modern typography with strong hierarchy.
- Rounded cards and panels with restrained depth.
- Reusable sections and components rather than one-off layouts.

## 7. Content Integrity and Confidentiality

- Use verified facts only.
- Do not invent achievements, metrics, employers, clients, technologies, certifications, or project outcomes.
- Do not expose classified, confidential, internal, personal, or security-sensitive information.
- Use placeholders marked `TBD` when source content is missing.
- Ask for approval before turning an assumption into a factual statement.

## 8. Design and Implementation Principles

### Reuse before duplication

New screens should use established tokens, components, variants, and layout patterns. A new pattern requires a documented reason.

### States are part of the design

Every data-driven experience should consider loading, empty, error, success, disabled, and permission-restricted states.

### Responsive behavior is intentional

Desktop, tablet, and mobile layouts must be designed as related experiences, not generated by mechanically shrinking a desktop frame.

### Accessibility is a foundation

Keyboard navigation, focus visibility, semantic structure, readable typography, color contrast, motion preferences, touch-target size, and meaningful alternative text must be considered during design.

### Design must remain implementable

Figma structures should map cleanly to Angular components and Tailwind tokens. Use Auto Layout, reusable components, variants, variables, responsive constraints, named layers, and editable content.

## 9. Working Method

1. Read `00_Master_Project_Specification.md`.
2. Read only the specialized documents relevant to the task.
3. Generate or update the smallest reviewable design package.
4. Review content accuracy, usability, responsiveness, accessibility, and implementation feasibility.
5. Record material decisions before starting dependent work.

Do not generate the entire platform in one uncontrolled pass.
