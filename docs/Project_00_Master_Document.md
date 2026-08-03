**PROJECT 00**

**Sultan Alomran  
Portfolio Platform**

Master Document to Give Figma AI

**A premium developer portfolio with Projects and the Visual Handbook as its two content pillars.**

Prepared for staged generation in Figma AI • Version 1.1 • August 2026

# Document purpose and usage

> **OUTCOME** Use this document as the single source of truth for Figma AI generation, UX review, prototype wiring, and later Angular/.NET implementation. It consolidates the earlier portfolio decisions without changing the product into a course platform or generic dashboard.


## Document hierarchy

This document is the overall product specification and primary source of truth for project scope, architecture, verified content, and implementation direction.

Detailed visual and user experience decisions are delegated to `docs/design/00_Master_Project_Specification.md` and its specialized design documents.

| **Project control** |                                                                           |
|---------------------|---------------------------------------------------------------------------|
| **Project owner**   | Sultan Alomran                                                            |
| **Product**         | Premium personal portfolio and engineering showcase                       |
| **Primary pillars** | Projects / Case Studies and Visual Handbook / Technical Content           |
| **Public stack**    | Portfolio.Web — Angular + Tailwind CSS + custom design                    |
| **Admin stack**     | Portfolio.Admin — Angular + Metronic-based private CMS                    |
| **Backend stack**   | Portfolio.Api — ASP.NET Core Web API + EF Core + SQL Server               |
| **Document use**    | Upload to Figma AI or paste the ready-to-use prompts in controlled stages |
| **Generation rule** | Do not generate every screen in one uncontrolled canvas                   |

## Recommended Figma AI sequence

1.  Create Figma page 00 — Project Overview from the ready-to-paste prompt in Section 14.

2.  Create page 01 — Sitemap and User Flows, then connect the primary public and admin journeys.

3.  Create page 02 — Design System and pages 03 / 07 — Public and Admin Components.

4.  Generate the Home page first in desktop, tablet, and mobile; review its visual language before expanding.

5.  Generate public listing and details pages in small related batches.

6.  Generate the admin desktop screens, then responsive fallbacks.

7.  Wire page 10 — Interactive Prototype and finish pages 11 / 12 — Technical Documentation and Developer Handoff.

> **IMPORTANT** Every major screen must include a side documentation frame listing purpose, user, route, components, APIs, entities, states, responsive behavior, accessibility, SEO, and implementation notes.
>
> **COMPANION INPUTS** Attach the Markdown or PDF plus the approved homepage image. This document governs product and technical rules; the image governs visual direction. Add the ERD later for technical documentation.

# 1. Product definition

*What the platform is, what it proves, and what it must never become.*

## Product statement

Sultan Alomran Portfolio Platform is a premium personal portfolio and engineering showcase for a Senior Full-Stack Software Engineer specializing in .NET and enterprise web systems, with Angular and TypeScript as the modern frontend stack for this portfolio. It presents professional identity, real enterprise software work, visual technical communication, certifications, experience, and the engineering process behind the platform itself.

## CV-grounded professional profile

> **CONTENT RULE** Use the approved CV facts below as factual grounding. Do not invent employers, client names, project counts, awards, job titles, dates, technologies, or measurable results. Confidential government and defence work must be described at a safe case-study level.

| **Approved profile facts** |                                                                                                                                                                                                                               |
|----------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Professional title**     | Senior Full-Stack Software Engineer \| .NET \| Certified OutSystems Architecture Specialist and Associate Reactive Web Developer                                                                                              |
| **Experience**             | 8+ years delivering enterprise web systems for government and defence sectors in Saudi Arabia                                                                                                                                 |
| **Current employer**       | SAMI Advanced Electronics — Full-Stack Web Developer, February 2019 to present                                                                                                                                                |
| **Earlier experience**     | Frontend Web Developer, January 2018 to February 2019; Web Developer / Business Analyst trainee, July to December 2017                                                                                                        |
| **Delivery proof**         | 7 government projects using Razor, MVC, and WebForms; 3 RSAF solutions delivered with OutSystems Reactive Web                                                                                                                 |
| **Current system**         | Request & Approval Management System using .NET 8 MVC, Razor, and Metronic, with custom multi-step approvals, configurable chains, dynamic forms, versioned attachments, dashboards, email, and notifications                 |
| **OutSystems proof**       | Reactive Web, secure operational workflows, role-based access, REST/SOAP integrations, Advanced SQL optimization, and modular 4-Layer Canvas architecture                                                                     |
| **Education**              | Bachelor's degree in Computer Software Engineering — King Saud University, 2011–2017                                                                                                                                          |
| **Certifications**         | OutSystems Architecture Specialist (February 2026); OutSystems Associate Reactive Web Developer (December 2024, score 92%); Scrum attendance certificate (February 2026); JavaScript development certificate (September 2018) |

## Core value pillars

| **Pillar**            | **What it shows**                                                               | **Primary proof**                           |
|-----------------------|---------------------------------------------------------------------------------|---------------------------------------------|
| Professional identity | Senior engineering experience, credibility, and personal brand                  | Hero, About, Experience, certifications     |
| Projects              | Real systems, architecture choices, challenges, results, and lessons            | Case-study pages and project gallery        |
| Visual Handbook       | Ability to simplify complex engineering topics visually                         | Infographics, series, categories, downloads |
| Engineering           | Architecture, API design, responsive UX, accessibility, and delivery discipline | Technical documentation and handoff         |
| Private CMS           | A maintainable content workflow instead of hard-coded pages                     | Admin dashboard and management screens      |

## Product goals

- Create a strong first impression for recruiters, employers, clients, and fellow developers.

- Showcase enterprise software projects without exposing confidential code or employer data.

- Make the Visual Handbook a major portfolio feature, not a separate learning product.

- Demonstrate proven .NET, API, SQL Server, architecture, performance, and OutSystems expertise while showing Angular and TypeScript through the portfolio implementation itself.

- Support content growth through categories, tags, series, media management, SEO, and analytics.

- Use the portfolio platform itself as evidence of full-stack architecture and product thinking.

## Explicit non-goals

- Not an online course, learning management system, social network, or community forum.

- Not a generic blog template, generic SaaS dashboard, or one-page-only landing site.

- No public anonymous comments; helpful votes and ratings are enough.

- No excessive gradients, dense glassmorphism, playful decoration, or animation that distracts from credibility.

- No exposure of private repositories, private phone numbers, confidential screenshots, or employer-sensitive information.

# 2. Users and experience goals

*Design around the visitor’s intent while keeping the product unmistakably personal.*

| **User**            | **Primary intent**                                  | **Fastest successful path**                          |
|---------------------|-----------------------------------------------------|------------------------------------------------------|
| Recruiter / HR      | Verify role, experience, stack, and credibility     | Home → Projects / Experience → Download CV / Contact |
| Engineering manager | Assess system thinking and engineering decisions    | Home → Project Details → Architecture / Impact       |
| Developer / peer    | Explore technical content and practical knowledge   | Home → Visual Handbook → Guide / Series              |
| Client / company    | Understand capabilities and initiate a conversation | Home / About → Projects → Contact                    |
| Administrator       | Publish and maintain content efficiently            | Admin Login → Dashboard → Create / Edit / Publish    |

## Experience principles

- The homepage leads; Projects and Visual Handbook receive equal strategic prominence.

- The first viewport answers who Sultan is, what he builds, and why the visitor should continue.

- Case studies emphasize problem, solution, architecture, decisions, and impact — not only screenshots.

- Visual content remains easy to browse by category, topic, difficulty, language, and series.

- Public pages feel custom and premium; the admin feels efficient and enterprise-oriented.

- Responsive behavior is designed intentionally for desktop, tablet, and mobile rather than scaled mechanically.

# 3. Solution architecture

*Three separately deployed applications sharing one API and data platform.*

| **Application landscape** |                                                                                                   |
|---------------------------|---------------------------------------------------------------------------------------------------|
| **Portfolio.Web**         | Angular public application; Tailwind CSS; custom portfolio design; SEO-ready public routes        |
| **Portfolio.Admin**       | Angular private CMS; Metronic-based shell; authentication, content operations, analytics          |
| **Portfolio.Api**         | ASP.NET Core Web API; EF Core; SQL Server; authentication; media integration; engagement services |
| **Storage**               | SQL Server for structured data and metadata; Azure Blob Storage or Cloudinary for actual files    |
| **External analytics**    | Microsoft Clarity or GA4 for broad visitor behavior                                               |
| **Internal analytics**    | Portfolio-specific views, downloads, shares, searches, votes, and ratings                         |

## High-level request flows

1.  Public visitor → Portfolio.Web → Portfolio.Api → SQL Server / media storage.

2.  Administrator → Portfolio.Admin → Portfolio.Api → SQL Server / media storage.

3.  Both Angular applications consume shared API contracts and never access the database directly.

## Architecture style

- Clean Architecture dependency principles at solution boundaries.

- Feature-oriented vertical slices for business capabilities such as Projects, Infographics, Series, Media, Authentication, Analytics, Contact Messages, and Settings.

- Feature-based Angular folders with reusable UI components and route-level lazy loading.

- Shared request/response contracts; thin controllers; application services or handlers own use cases.

> **SUGGESTED SOLUTION NAMES** Portfolio.Web / Portfolio.Admin / Portfolio.Api. A separate Portfolio.Shared project may hold DTOs only when the chosen solution structure benefits from it; avoid sharing EF Core entities with Angular or public API consumers.

# 4. Information architecture and navigation

*Public navigation is simple; deeper content is reached through contextual routes and filters.*

## Public sitemap

| **Level 1**     | **Level 2 / details**                                                       | **Primary route**                                    |
|-----------------|-----------------------------------------------------------------------------|------------------------------------------------------|
| Home            | Featured projects, guides, series, experience, technologies, certifications | /                                                    |
| Projects        | Project Details                                                             | /projects; /projects/:slug                           |
| Visual Handbook | Category View; Infographic Details; Series Details                          | /visual-handbook; /infographics/:slug; /series/:slug |
| Experience      | Career progression, roles, skills, certifications                           | /experience                                          |
| About           | Identity, values, engineering philosophy                                    | /about                                               |
| Contact         | Contact form and professional links                                         | /contact                                             |
| Search          | Projects, infographics, series, optional articles                           | /search?q=                                           |
| Fallback        | Professional page-not-found experience                                      | /404 / wildcard                                      |

## Global navigation

| **Primary**          | Home, Projects, Visual Handbook, Experience, About, Contact                                                      |
|----------------------|------------------------------------------------------------------------------------------------------------------|
| **Secondary**        | Download CV, LinkedIn, GitHub, Email, Theme toggle                                                               |
| **Public behavior**  | Sticky desktop navigation; accessible mobile menu; visible focus and active route states                         |
| **Admin navigation** | Dashboard, Infographics, Projects, Categories, Tags, Series, Media, Analytics, Messages, Users & Roles, Settings |

# 5. Figma file organization

*A controlled page structure that separates foundations, responsive screens, prototype, and handoff.*

| **Page** | **Name**                 | **Required content**                                                                     |
|----------|--------------------------|------------------------------------------------------------------------------------------|
| 00       | Project Overview         | Vision, goals, users, pillars, architecture, scope, success criteria, generation roadmap |
| 01       | Sitemap and User Flows   | Public sitemap, admin map, primary journeys, content publishing flow                     |
| 02       | Design System            | Colors, typography, spacing, grid, radii, shadows, icons, state tokens                   |
| 03       | Public Components        | Buttons, cards, navigation, filters, content states, footer, CTA                         |
| 04       | Public Desktop Screens   | All approved public pages at desktop width                                               |
| 05       | Public Tablet Screens    | Major pages and responsive behavior at tablet width                                      |
| 06       | Public Mobile Screens    | Major pages and mobile navigation/filter behavior                                        |
| 07       | Admin Components         | Metronic-aligned forms, tables, uploaders, status chips, charts                          |
| 08       | Admin Desktop Screens    | Admin flows and management pages                                                         |
| 09       | Admin Responsive Screens | Tablet/mobile fallbacks for essential admin operations                                   |
| 10       | Interactive Prototype    | Connected public and admin journeys with state transitions                               |
| 11       | Technical Documentation  | Architecture, APIs, entity relationships, screen documentation panels                    |
| 12       | Developer Handoff        | Tokens, naming, measurements, assets, state and implementation notes                     |

## Frame naming convention

> **NAMING** Use \[Area\] / \[Page\] / \[Breakpoint\] / \[State\]. Examples: Public / Home / Desktop / Default; Public / Visual Handbook / Mobile / Empty; Admin / Infographic Editor / Desktop / Uploading.

## Required documentation frame beside each desktop screen

| **Field**      | **What to document**                                                |
|----------------|---------------------------------------------------------------------|
| Purpose / user | Why the page exists and who is using it                             |
| Route          | Angular route and key query parameters                              |
| Components     | Page container and reusable components                              |
| APIs           | Requests, parameters, mutations, and expected data groups           |
| Entities       | Related SQL Server tables / domain entities                         |
| States         | Loading, empty, error, success, validation, permission, offline     |
| Responsive     | Layout changes for desktop, tablet, and mobile                      |
| Accessibility  | Focus order, labels, contrast, keyboard behavior, touch targets     |
| SEO            | H1, metadata, Open Graph, canonical and indexability notes          |
| Implementation | Lazy loading, @defer, image optimization, privacy, or caching notes |

# 6. Design system

*Premium, enterprise-focused, technical, and visually polished — with restraint.*

| **Token**       | **Value** | **Use**                                                              |
|-----------------|-----------|----------------------------------------------------------------------|
| Primary         | \#7C3AED  | Primary action, active navigation, selected filters, important links |
| Secondary       | \#4F46E5  | Secondary accent, icons, supporting active states                    |
| Hero / ink      | \#0F172A  | Dark hero, navigation contrast, primary text                         |
| Page background | \#F8FAFC  | Public page canvas and subtle section separation                     |
| Surface         | \#FFFFFF  | Cards, panels, form surfaces                                         |
| Secondary text  | \#64748B  | Metadata, helper text, captions                                      |
| Border          | \#E2E8F0  | Card outlines, dividers, form controls                               |
| Success         | \#16A34A  | Published, completed, successful action                              |
| Warning         | \#D97706  | Draft warning, partial state, attention                              |
| Danger          | \#DC2626  | Error, destructive action, invalid state                             |

| **Layout and type tokens**     |                                                                     |
|--------------------------------|---------------------------------------------------------------------|
| **Typography**                 | Inter or equivalent professional sans-serif                         |
| **Display**                    | 48–64 px desktop; 40–48 px tablet; 34–40 px mobile                  |
| **Page heading**               | 36–48 px                                                            |
| **Section heading**            | 26–34 px                                                            |
| **Card heading**               | 18–22 px                                                            |
| **Body / secondary / caption** | 16 px / 14 px / 12 px                                               |
| **Spacing**                    | 8 px base system; 80–112 px desktop section spacing; 24 px card gap |
| **Container**                  | 1200–1280 px maximum width                                          |
| **Radius**                     | 12 px small; 20 px card; 28 px hero where appropriate               |
| **Grid**                       | 12 columns desktop; 8 tablet; 4 mobile                              |
| **Breakpoints**                | Mobile \<768; tablet 768–1023; laptop 1024–1439; desktop ≥1440      |

## Visual direction

- Use generous whitespace, strong hierarchy, subtle depth, crisp borders, and deliberate content rhythm.

- The hero may be dark and expressive; content pages should remain calm, bright, and highly readable.

- Use violet and indigo selectively — never as constant decoration.

- Prefer aligned card grids over masonry unless source thumbnails genuinely require variable heights.

- Use purposeful motion only: navigation feedback, content reveal, light card interaction, modal and drawer transitions.

- Avoid generic Bootstrap or template styling in the public application; the admin may use Metronic conventions consistently.

# 7. Reusable component inventory

*Components are named consistently between Figma and Angular handoff.*

| **Group**         | **Components**                                                                                                     |
|-------------------|--------------------------------------------------------------------------------------------------------------------|
| Navigation        | PublicNavbar, MobileNavigation, Breadcrumb, Pagination, PublicFooter                                               |
| Actions           | PublicButton, ShareButtons, HelpfulVote, Rating, CTASection                                                        |
| Content cards     | ProjectCard, InfographicCard, CategoryCard, SeriesCard, CertificationCard, StatCard                                |
| Content structure | SectionHeader, ExperienceTimelineItem, TechnologyBadge, RelatedContent                                             |
| Discovery         | SearchBar, FilterBar, mobile filter drawer, sort control                                                           |
| System states     | LoadingSkeleton, EmptyState, ErrorState, no-image placeholder, retry / offline                                     |
| Admin             | DataTable, status badge, form field, rich editor, media picker, uploader, progress row, confirmation dialog, toast |

> **COMPONENT RULE** Figma components and Angular components should share semantic names and states. Each component should document default, hover, focus, disabled, loading, validation, and responsive behavior when applicable.

# 8. Public screen specifications

*The homepage establishes the visual language; listing and detail pages extend it consistently.*

## 8.1 Home

| **Purpose**            | Create a strong first impression and direct visitors to Projects or the Visual Handbook.                                                                                                                                    |
|------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Primary user**       | All public visitors                                                                                                                                                                                                         |
| **Route**              | /                                                                                                                                                                                                                           |
| **Angular components** | PublicNavbar; Hero; StatCard; FeaturedProjects; ProjectCard; LatestGuides; InfographicCard; FeaturedSeries; SeriesCard; ExperienceTimeline; TechnologyStack; CertificationList; CTASection; PublicFooter                    |
| **API endpoints**      | GET /api/home; GET /api/projects/featured; GET /api/infographics/latest; GET /api/series/featured; GET /api/profile/statistics; GET /api/profile/experience; GET /api/profile/technologies; GET /api/profile/certifications |
| **Related entities**   | Projects; ProjectTechnologies; Technologies; Infographics; Categories; Series; SeriesItems; ExperienceItems; Certifications; SiteSettings; MediaFiles                                                                       |

### Required sections

- Sticky navigation and dark premium hero.

- Hero: Sultan Alomran; Senior Full-Stack Software Engineer \| .NET & Certified OutSystems Architecture Specialist; ‘I build enterprise solutions and explain technology visually.’

- Primary CTA: View My Projects. Secondary CTA: Explore Visual Handbook.

- Statistics strip: 8+ years experience; 7 government projects; 3 RSAF solutions; 92% OutSystems certification score.

- Optional dynamic content metric: show 50+ visual guides only when the published CMS count supports it; never use 100+ projects and solutions.

- Featured projects: Request & Approval Management System; selected government web systems; selected RSAF OutSystems solutions; Portfolio Platform.

- Latest guides, featured .NET / Angular / OutSystems series, timeline, technologies, certifications, CTA, footer.

> **IMPLEMENTATION NOTES** Profile text may begin static, while featured content comes from the API. Lazy-load images, use Angular @defer below the fold, show skeletons for dynamic sections, hide empty optional sections, and preserve strong SEO metadata.

## 8.2 Projects listing

| **Purpose**            | Let recruiters and engineers browse projects and case studies.                                        |
|------------------------|-------------------------------------------------------------------------------------------------------|
| **Primary user**       | Recruiters, engineering managers, clients, developers                                                 |
| **Route**              | /projects?search=&technology=&type=&page=                                                             |
| **Angular components** | ProjectsPage; SearchBar; FilterBar; ProjectGrid; ProjectCard; Pagination; EmptyState; LoadingSkeleton |
| **API endpoints**      | GET /api/projects; GET /api/technologies; GET /api/project-types                                      |
| **Related entities**   | Projects; Technologies; ProjectTechnologies; ProjectImages; ProjectLinks; MediaFiles                  |

### Required sections

- Page heading, search, technology and project-type filters.

- Optional featured project area, three-column desktop grid, pagination, CTA, footer.

- Cards show thumbnail, title, summary, technologies, type, status, and View Case Study.

- Filters: All, .NET, Angular, OutSystems, APIs & Integration, Data & Reporting, Architecture.

> **IMPLEMENTATION NOTES** Use server-side pagination; preserve filters in URL query parameters; use a mobile filter drawer; collapse to one column on mobile.

## 8.3 Project details

| **Purpose**            | Present a project as a professional engineering case study.                                                                                 |
|------------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| **Primary user**       | Engineering managers, recruiters, clients, peers                                                                                            |
| **Route**              | /projects/:slug                                                                                                                             |
| **Angular components** | ProjectDetailsPage; Breadcrumb; ProjectHero; ArchitectureViewer; TechnologyBadge; ScreenshotGallery; RelatedContent; PreviousNextNavigation |
| **API endpoints**      | GET /api/projects/{slug}; GET /api/projects/{id}/related-infographics; POST /api/projects/{id}/views                                        |
| **Related entities**   | Projects; Technologies; ProjectTechnologies; ProjectImages; ProjectLinks; MediaFiles; Infographics; ContentEvents                           |

### Required sections

- Project hero, title, status, summary, business problem, proposed solution, architecture diagram.

- Key features, technology stack, gallery, challenges, engineering decisions, impact, lessons learned.

- Related infographics, external links, previous / next project, CTA, footer.

> **IMPLEMENTATION NOTES** Use slug routing and an image lightbox. Clearly label public demo, private repository, and case-study-only work. Never expose confidential source code or employer-sensitive details.

## 8.4 Visual Handbook

| **Purpose**            | Showcase Sultan’s technical infographics as a visual gallery inside the portfolio.                   |
|------------------------|------------------------------------------------------------------------------------------------------|
| **Primary user**       | Developers, recruiters, technical peers                                                              |
| **Route**              | /visual-handbook?search=&category=&tags=&difficulty=&language=&series=&sort=&page=                   |
| **Angular components** | VisualHandbookPage; SearchBar; CategoryCard; FilterBar; InfographicGrid; InfographicCard; Pagination |
| **API endpoints**      | GET /api/infographics; GET /api/categories; GET /api/tags; GET /api/series                           |
| **Related entities**   | Infographics; Categories; Tags; InfographicTags; Series; SeriesItems; MediaFiles; ContentEvents      |

### Required sections

- Intro, search, category / topic / difficulty filters, language and sort controls.

- Featured categories, latest guides, aligned guide grid, pagination, footer.

- Categories include .NET, Angular, OutSystems, Architecture, SQL, APIs, Performance, Security, DevOps, Background Services, Career, Problem Solving.

- Cards show thumbnail, title, category, tags, reading time, views, date, language, optional series badge.

> **IMPLEMENTATION NOTES** This is not a course catalogue. Use server-side search and pagination, query-parameter state, optimized thumbnails, and a clean aligned grid.

## 8.5 Infographic details

| **Purpose**            | Display one visual guide as a polished portfolio content page.                                                                                                                                                                                                 |
|------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Primary user**       | Developers, peers, recruiters                                                                                                                                                                                                                                  |
| **Route**              | /infographics/:slug                                                                                                                                                                                                                                            |
| **Angular components** | InfographicDetailsPage; Breadcrumb; InfographicViewer; Metadata; ShareButtons; HelpfulVote; Rating; RelatedContent; SeriesContext                                                                                                                              |
| **API endpoints**      | GET /api/infographics/{slug}; GET /api/infographics/{id}/related; POST /api/infographics/{id}/views; POST /api/infographics/{id}/downloads; POST /api/infographics/{id}/shares; POST /api/infographics/{id}/helpful-votes; POST /api/infographics/{id}/ratings |
| **Related entities**   | Infographics; Categories; Tags; InfographicTags; Series; SeriesItems; MediaFiles; ContentEvents; HelpfulVotes; Ratings                                                                                                                                         |

### Required sections

- Title, taxonomy, date, reading time, views, optional language toggle.

- High-resolution viewer with zoom / full-screen, description, key takeaways.

- Download PNG / PDF, share, helpful vote, rating, related guides, series context, previous / next.

> **IMPLEMENTATION NOTES** Do not add public anonymous comments. Preserve high-resolution access while optimizing delivery. Make large infographics usable on mobile and avoid excessive tracking.

## 8.6 Series details

| **Purpose**            | Group related infographics as an ordered professional content series. |
|------------------------|-----------------------------------------------------------------------|
| **Primary user**       | Developers and technical peers                                        |
| **Route**              | /series/:slug                                                         |
| **Angular components** | SeriesDetailsPage; SeriesHeader; SeriesItemList; InfographicCard      |
| **API endpoints**      | GET /api/series/{slug}                                                |
| **Related entities**   | Series; SeriesItems; Infographics; MediaFiles                         |

### Required sections

- Series cover, title, summary, part count, ordered infographic list, related categories, share action, footer.

> **IMPLEMENTATION NOTES** Use Series, Collection, or Visual Guide Series — never Course. Preserve order with Position or SortOrder.

## 8.7 Experience

| **Purpose**            | Show career progression, delivery impact, leadership, and skill depth.                |
|------------------------|---------------------------------------------------------------------------------------|
| **Primary user**       | Recruiters, engineering managers                                                      |
| **Route**              | /experience                                                                           |
| **Angular components** | ExperiencePage; ExperienceTimeline; ExperienceCard; SkillGroup; CertificationCard     |
| **API endpoints**      | GET /api/profile/experience; GET /api/profile/skills; GET /api/profile/certifications |
| **Related entities**   | ExperienceItems; Skills; SkillCategories; Certifications; MediaFiles                  |

### Required sections

- Introduction, timeline, roles, responsibilities, major systems, leadership and mentoring, grounded in the approved CV facts in Section 1.

- Skills grouped by domain, certifications, Download CV, CTA, footer.

> **IMPLEMENTATION NOTES** Keep wording concise and outcomes-oriented; do not expose confidential employer information.

## 8.8 About

| **Purpose**            | Explain professional identity, values, technical philosophy, and problem-solving approach. |
|------------------------|--------------------------------------------------------------------------------------------|
| **Primary user**       | All public visitors                                                                        |
| **Route**              | /about                                                                                     |
| **Angular components** | AboutPage; ProfileSummary; ValueCard; TechnologyBadge; CTASection                          |
| **API endpoints**      | GET /api/profile/about; GET /api/profile/technologies                                      |
| **Related entities**   | SiteSettings; Profile; Technologies; MediaFiles                                            |

### Required sections

- Professional image, who I am, what I build, engineering philosophy, problem solving, quality approach, mentoring, tools, CTA.

> **IMPLEMENTATION NOTES** Balance personality with engineering credibility. Keep the content specific to Sultan’s actual work and approach.

## 8.9 Contact

| **Purpose**            | Let recruiters, companies, clients, and developers contact Sultan. |
|------------------------|--------------------------------------------------------------------|
| **Primary user**       | Recruiters, companies, clients, developers                         |
| **Route**              | /contact                                                           |
| **Angular components** | ContactPage; ContactForm; ContactMethodCard                        |
| **API endpoints**      | POST /api/contact-messages                                         |
| **Related entities**   | ContactMessages                                                    |

### Required sections

- Introduction, form, email, LinkedIn, GitHub, location, availability, response-time note, footer.

- Fields: full name, email, subject, message.

> **IMPLEMENTATION NOTES** Use Angular Reactive Forms, validation, spam protection, disabled submitting state, and success/error feedback. Do not expose a private phone number.

## 8.10 Search and 404

| **Purpose**            | Provide global discovery and a professional fallback.                                    |
|------------------------|------------------------------------------------------------------------------------------|
| **Primary user**       | All public visitors                                                                      |
| **Route**              | /search; wildcard /404                                                                   |
| **Angular components** | SearchResultsPage; SearchBar; result tabs; filters; Pagination; EmptyState; NotFoundPage |
| **API endpoints**      | GET /api/search?query=&type=&page=&pageSize=                                             |
| **Related entities**   | Projects; Infographics; Series; Categories; Tags                                         |

### Required sections

- Search input, result-type tabs, filters, results, empty state, pagination.

- 404: Page not found, Return Home, Browse Projects, Explore Visual Handbook.

> **IMPLEMENTATION NOTES** Search should cover projects, infographics, series, and optional articles. The 404 state should preserve brand tone and offer useful recovery paths.

# 9. Admin application specifications

*A separate, private Angular CMS using a consistent Metronic-style enterprise shell.*

| **Admin screen**   | **Core content / action**                                                          | **API / entities**                                                                                                                                                  |
|--------------------|------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Login              | Email, password, remember me, sign in, forgot password                             | POST /api/auth/login; POST /api/auth/refresh; POST /api/auth/logout; Users, Roles, UserRoles, RefreshTokens                                                         |
| Dashboard          | Totals, views, downloads, charts, recent uploads, quick actions, activity, storage | GET /api/admin/dashboard/summary; GET /api/admin/dashboard/activity; Infographics, Projects, ContentEvents, MediaFiles, AuditLogs                                   |
| Infographics list  | Search, filters, table, bulk actions, pagination, add new                          | GET /api/admin/infographics; POST /api/admin/infographics; PUT /api/admin/infographics/{id}; DELETE /api/admin/infographics/{id}; Infographics and relations        |
| Infographic editor | Content, taxonomy, series, assets, SEO, status, schedule, featured                 | GET /api/admin/infographics/{id}; POST /api/admin/infographics; PUT /api/admin/infographics/{id}; Infographics, Tags, SeriesItems, MediaFiles                       |
| Bulk upload        | Drag/drop queue, defaults, validation, progress, retry                             | POST /api/admin/media/bulk-upload; POST /api/admin/infographics/bulk-create                                                                                         |
| Projects           | List/edit case-study sections, screenshots, links, tech, status, featured          | GET /api/admin/projects; POST /api/admin/projects; PUT /api/admin/projects/{id}; DELETE /api/admin/projects/{id}; Projects and relations                            |
| Categories & Tags  | Hierarchy, parent, slug, icon, order, active; tag usage                            | GET/POST /api/admin/categories; PUT/DELETE /api/admin/categories/{id}; GET/POST /api/admin/tags; PUT/DELETE /api/admin/tags/{id}; Categories, Tags, InfographicTags |
| Series             | Create, cover, description, add guides, drag reorder, publish                      | GET/POST /api/admin/series; GET/PUT/DELETE /api/admin/series/{id}; Series, SeriesItems, Infographics, MediaFiles                                                    |
| Media Library      | Upload, search, preview, details, usages, replace, delete                          | GET/POST /api/admin/media; GET/PUT/DELETE /api/admin/media/{id}; MediaFiles; optional MediaCollections / MediaCollectionItems                                       |
| Analytics          | Views, downloads, shares, votes, ratings, top content, search, sources             | GET /api/admin/analytics/summary; GET /api/admin/analytics/content; ContentEvents, HelpfulVotes, Ratings                                                            |
| Messages           | List, detail, status, mark read, archive                                           | GET /api/admin/contact-messages; GET /api/admin/contact-messages/{id}; PUT /api/admin/contact-messages/{id}/status; ContactMessages                                 |
| Users & Roles      | Users, roles, permissions, active state, last login                                | GET/POST /api/admin/users; PUT /api/admin/users/{id}; GET/POST /api/admin/roles; PUT /api/admin/roles/{id}; Users, Roles, UserRoles, Permissions, RolePermissions   |
| Settings           | Profile, homepage, social, SEO, contact, theme, storage, analytics                 | GET /api/admin/settings; PUT /api/admin/settings; SiteSettings                                                                                                      |

## Infographic editor requirements

- Title, slug, short summary, full description, category, tags, series, series position.

- Language, difficulty, reading time, main image, thumbnail, PDF.

- SEO title, SEO description, publish status/date, featured flag.

- Save Draft and Publish actions, upload progress, image preview, dimension/file validation.

- Unsaved-changes warning before route exit.

> **PRIORITY** Users and Roles is lower priority because a single administrator is sufficient initially. Design it for completeness, but keep MVP implementation focused on content, media, messages, analytics, and settings.

# 10. API and data model map

*The design documents real application behavior, not decorative screens.*

| **Feature**    | **Representative endpoints**                                                                                                                                                                                                                                                          | **Core entities**                                                           |
|----------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------|
| Home           | GET /api/home; or compose GET /api/projects/featured, GET /api/infographics/latest, GET /api/series/featured, GET /api/profile/statistics, GET /api/profile/experience, GET /api/profile/technologies, and GET /api/profile/certifications                                            | Projects, Infographics, Series, Profile, ExperienceItems, Certifications    |
| Projects       | GET /api/projects; GET /api/projects/{slug}; GET /api/projects/{id}/related-infographics; POST /api/projects/{id}/views                                                                                                                                                               | Projects, Technologies, ProjectTechnologies, ProjectImages, ProjectLinks    |
| Infographics   | GET /api/infographics; GET /api/infographics/{slug}; GET /api/infographics/{id}/related; POST /api/infographics/{id}/views; POST /api/infographics/{id}/downloads; POST /api/infographics/{id}/shares; POST /api/infographics/{id}/helpful-votes; POST /api/infographics/{id}/ratings | Infographics, Categories, Tags, InfographicTags, SeriesItems, ContentEvents |
| Search         | GET /api/search                                                                                                                                                                                                                                                                       | Projects, Infographics, Series, Categories, Tags                            |
| Profile        | GET /api/profile/about; GET /api/profile/experience; GET /api/profile/skills; GET /api/profile/technologies; GET /api/profile/certifications                                                                                                                                          | Profile, SiteSettings, ExperienceItems, Skills, Certifications              |
| Contact        | POST /api/contact-messages                                                                                                                                                                                                                                                            | ContactMessages                                                             |
| Authentication | POST /api/auth/login; POST /api/auth/refresh; POST /api/auth/logout                                                                                                                                                                                                                   | Users, Roles, Permissions, RefreshTokens                                    |
| Admin content  | GET/POST collection routes and GET/PUT/DELETE /api/admin/{resource}/{id} for infographics, projects, categories, tags, and series                                                                                                                                                     | Content entities plus relations, AuditLogs                                  |
| Media          | POST /api/admin/media; POST /api/admin/media/bulk-upload; PUT /api/admin/media/{id}; DELETE /api/admin/media/{id}                                                                                                                                                                     | MediaFiles, optional collections                                            |
| Analytics      | GET /api/admin/analytics/summary; GET /api/admin/analytics/content; explicit public engagement mutations                                                                                                                                                                              | ContentEvents, HelpfulVotes, Ratings                                        |

## Entity groups

| **Domain**        | **Entities**                                                                       |
|-------------------|------------------------------------------------------------------------------------|
| Identity & access | Users, Roles, Permissions, UserRoles, RolePermissions, optional RefreshTokens      |
| Portfolio content | Projects, Technologies, ProjectTechnologies, ProjectImages, ProjectLinks           |
| Visual content    | Infographics, Categories with ParentId, Tags, InfographicTags, Series, SeriesItems |
| Media             | MediaFiles; optional MediaCollections, MediaItems, MediaCollectionItems            |
| Profile           | Profile, ExperienceItems, Skills, SkillCategories, Certifications, SiteSettings    |
| Engagement        | ContentEvents, HelpfulVotes, Ratings; optional likes only if later justified       |
| Communication     | ContactMessages                                                                    |
| Operations        | AuditLogs, Notifications, soft-delete fields                                       |

## Data and implementation conventions

- Use GUID primary keys with sequential generation where appropriate; index foreign keys and common filters.

- Use unique slugs for public content and index Title, CategoryId, CreatedAt / PublishedAt, Status, and SortOrder as needed.

- Apply soft delete to managed content and record important admin actions in AuditLogs.

- Store file bytes outside SQL Server; store URLs, MIME type, dimensions, size, alt text, and usage metadata in MediaFiles.

- Server-side pagination and filtering are required for projects, infographics, search results, and admin tables.

- Use DTO projections and AsNoTracking for read-only public queries; do not expose EF Core entities directly.

# 11. Prototype flows

*Wire the prototype around the most important evidence and publishing journeys.*

| **Flow**           | **Connection sequence**                                           | **Success signal**                                         |
|--------------------|-------------------------------------------------------------------|------------------------------------------------------------|
| Project discovery  | Home → View My Projects → Projects → Project Details              | Visitor reaches a case study and can navigate related work |
| Visual discovery   | Home → Explore Visual Handbook → Handbook → Infographic Details   | Visitor views, zooms, downloads, or shares a guide         |
| Category discovery | Handbook → Category/filter → Filtered guide listing               | Filters persist and results update predictably             |
| Series discovery   | Handbook → Series → Series Details → Infographic Details          | Ordered series context remains visible                     |
| Career proof       | Home → Experience → Download CV / Contact                         | Recruiter reaches verified professional detail             |
| Contact            | Home / About / Project → Contact → Valid submit → Success         | Form handles validation, sending, success, and error       |
| Admin publish      | Login → Dashboard → Infographics → Create → Save / Publish → List | New content appears with correct state                     |
| Admin project edit | Dashboard → Projects → Edit → Preview → Public details            | Admin can verify public presentation                       |
| Media workflow     | Dashboard → Media Library → Upload → Select / reuse               | Media asset is available to editors                        |
| Analytics review   | Dashboard → Analytics → Date range / content drilldown            | Admin understands content engagement                       |

> **PROTOTYPE FIDELITY** Prototype the meaningful states and decisions — menus, filters, pagination, opening a case study, infographic zoom, form validation, upload progress, publish confirmation, and preview. Do not waste prototype complexity on decorative motion.

# 12. States, responsive behavior, accessibility and quality

*These states are part of the product specification, not optional polish.*

| **State**     | **Required example**                                                     |
|---------------|--------------------------------------------------------------------------|
| Loading       | Card skeletons, detail-page skeleton, admin table skeleton               |
| Empty         | No projects / guides / messages with relevant recovery action            |
| Error         | API error with retry; field-level and summary validation errors          |
| Success       | Contact sent, content saved, upload completed, publish confirmed         |
| Progress      | Single and bulk upload progress with per-file status                     |
| Missing media | Branded no-image placeholder preserving layout                           |
| Permission    | Permission denied / unauthorized admin state                             |
| Offline       | Connection issue with retry and preserved user input when possible       |
| 404           | Branded fallback with Home, Projects, and Visual Handbook recovery paths |

## Responsive rules

- Create desktop, tablet, and mobile variants for Home and all major public pages.

- Desktop uses a 12-column grid, tablet 8 columns, mobile 4 columns.

- Projects and guide cards reduce from three columns to two and then one based on content width.

- Filters become an accessible drawer or sheet on mobile; active filter count remains visible.

- Large infographics support fit-to-width, zoom, full-screen, and comfortable vertical scrolling on mobile.

- Admin is desktop-first, with usable tablet and mobile fallbacks for essential actions rather than a squeezed data table.

## Accessibility

- Strong color contrast and visible keyboard focus on all interactive elements.

- Semantic heading hierarchy with one clear H1 per public page.

- Keyboard-friendly menus, modals, lightboxes, drawers, and admin dialogs.

- Programmatic form labels, helpful error text, and no status communicated only by color.

- Descriptive action labels, image alt text, accessible data-table headers, and sufficiently large mobile touch targets.

## SEO and performance

- SEO-friendly public routes, metadata, canonical URLs, Open Graph images, and sitemap-ready structure.

- Responsive optimized images with lazy loading and preserved high-resolution downloads.

- Angular route-level lazy loading and @defer for below-the-fold homepage content.

- Skeletons for perceived performance and deliberate empty-state handling.

- Keep core content indexable; define Angular SSR / prerendering strategy during implementation planning.

- Track only purposeful engagement metrics and avoid unnecessary visitor surveillance.

# 13. Developer handoff and definition of done

*The Figma file should reduce ambiguity for Angular and ASP.NET Core implementation.*

## Developer handoff contents

- Named variables for colors, typography, spacing, radii, elevation, breakpoints, and semantic states.

- Component variants and interaction states with consistent Figma / Angular naming.

- Desktop, tablet, and mobile measurements for major layouts.

- Export-ready assets with format, dimensions, compression, alt text, and intended use.

- Screen documentation panels containing route, APIs, entities, states, accessibility, SEO, and implementation notes.

- Prototype links for primary public journeys and admin content publishing.

- Notes distinguishing static launch content from API-driven content.

- Privacy notes for projects, screenshots, source links, employer information, and contact data.

## Design definition of done

| **Check**     | **Acceptance condition**                                                                       |
|---------------|------------------------------------------------------------------------------------------------|
| Product fit   | Feels like Sultan’s premium engineering portfolio; not a course, social network, or template   |
| Homepage      | Clearly communicates identity and routes to Projects and Visual Handbook                       |
| Coverage      | All public and prioritized admin screens exist with required states                            |
| Consistency   | Components, tokens, spacing, and naming remain consistent across breakpoints                   |
| Documentation | Every desktop screen has the required technical side panel                                     |
| Prototype     | Primary discovery, contact, and content-publishing flows are connected                         |
| Accessibility | Focus, labels, contrast, keyboard behavior, and touch targets are documented                   |
| Handoff       | Assets, measurements, variants, routes, APIs, entities, and notes are ready for implementation |

# 14. Ready-to-paste Figma AI prompt — Foundation and homepage

*Attach the master document and approved homepage reference, then generate only the controlled first bundle below.*

> **HOW TO USE** Attach this Project 00 document as Markdown or PDF and attach the approved homepage image. Paste the full prompt below. Review the foundation and homepage bundle before generating any remaining public or admin screens.
>
> **ROLE AND TASK**
>
> Act as a senior product designer and design-systems architect. Use the attached Project 00 master document as the single source of truth for the Sultan Alomran Portfolio Platform. Use the attached homepage image as the approved visual direction.
>
> **PRODUCT**
>
> Design a premium personal portfolio and engineering showcase for Sultan Alomran, a Senior Full-Stack Software Engineer specializing in .NET and enterprise web systems. Angular and TypeScript are the modern frontend stack used to implement this portfolio. Showcase his professional identity, enterprise projects and case studies, technical infographics, visual guide series, experience, certifications, engineering philosophy, and the engineering process behind the platform.
>
> **PROFILE GROUNDING**
>
> Use only the approved CV-grounded facts in Section 1. Key proof points are 8+ years of experience, 7 government projects, 3 RSAF OutSystems solutions, and a 92% OutSystems Associate Reactive Web Developer certification score. Do not show ‘100+ projects and solutions’. Show ‘50+ visual guides’ only if the CMS content count supports it.
>
> **POSITIONING**
>
> The homepage leads the experience. Projects and Visual Handbook are the two primary content pillars. This is not an online course, LMS, social network, blog template, generic SaaS dashboard, or one-page-only portfolio.
>
> **APPLICATIONS**
>
> Show three separately deployed applications: Portfolio.Web — Angular + Tailwind CSS custom public experience; Portfolio.Admin — Angular + Metronic-based private CMS; Portfolio.Api — ASP.NET Core Web API + EF Core + SQL Server + authentication + media + analytics. Show both public and admin applications consuming the same API, with SQL Server for structured data and Azure Blob Storage or Cloudinary for media files.
>
> **ARCHITECTURE**
>
> Describe a pragmatic hybrid of Clean Architecture dependency principles, feature-oriented vertical slices, feature-based Angular organization, reusable components, and shared API contracts. Feature areas include Projects, Infographics, Categories, Tags, Series, Media, Authentication, Analytics, Contact Messages, and Site Settings.
>
> **USERS**
>
> Include five audiences: recruiter/HR, engineering manager, developer/peer, client/company, and administrator. For each, show primary intent and ideal path.
>
> **VISUAL DIRECTION**
>
> Modern, clean, premium, enterprise-focused, technical, and polished. Use Inter; primary violet \#7C3AED; secondary indigo \#4F46E5; deep navy \#0F172A; light slate \#F8FAFC; white surfaces; slate secondary text \#64748B; border \#E2E8F0. Use an 8px spacing system, generous whitespace, 20px cards, and restrained depth. Avoid excessive gradients, glassmorphism, playful decoration, generic templates, and excessive animation.
>
> **GENERATION SCOPE**
>
> For this generation create only: 1) Page 00 — Project Overview; 2) Page 01 — Sitemap and User Flows; 3) Page 02 — Public Design System; 4) Page 03 — Public Components; 5) Homepage Desktop; 6) Homepage Tablet; 7) Homepage Mobile; 8) Homepage Technical Documentation. Do not generate admin screens or the remaining public pages yet.
>
> **FOUNDATION CONTENT**
>
> Create clearly labeled overview frames for: cover and one-sentence vision; product purpose; core pillars; target users and successful paths; application architecture; product goals; explicit non-goals; public sitemap summary; admin capability summary; design principles; MVP vs later scope; success criteria; and the staged Figma generation roadmap.
>
> **HOMEPAGE DIRECTION**
>
> Follow the attached homepage image closely for hero composition, dark navy and violet styling, floating statistics strip, project cards, visual guide cards, featured series, experience timeline, technologies, certifications, CTA, and footer. Preserve editable layers and do not copy accidental artifacts from the reference image.
>
> **MVP EMPHASIS**
>
> Prioritize Home, Projects, Project Details, Visual Handbook, Infographic Details, Series Details, Experience, About, Contact, Search, 404, Admin Login, Dashboard, Infographics, Project management, Categories/Tags, Series, Media Library, Analytics, Messages, and Settings. Treat Users & Roles as lower priority because one administrator is sufficient initially.
>
> **FRAME RULES**
>
> Use Auto Layout, reusable components, named color, text, spacing, radius, and effect variables, clear grid alignment, concise documentation text, and editable layers. Use the frame naming convention in Section 5. Keep foundation pages readable as product documentation rather than dense application screens.
>
> **NEXT-STEP PANEL**
>
> End Page 00 with a generation plan: 01 Sitemap and User Flows; 02 Design System; 03 Public Components; 04–06 Public responsive screens; 07 Admin Components; 08–09 Admin screens; 10 Interactive Prototype; 11 Technical Documentation; 12 Developer Handoff. State: do not generate all screens in one uncontrolled layout.
>
> **OUTPUT**
>
> Produce only the eight foundation and homepage outputs listed in GENERATION SCOPE. Do not invent a course system, community features, public comments, pricing, subscriptions, or unrelated SaaS modules. Preserve Sultan’s personal brand and the equal prominence of Projects and the Visual Handbook.

**END OF PROJECT 00 SPECIFICATION**
