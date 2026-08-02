# # Database Specification — Draft 0.1

> **Status:** Database design draft awaiting validation and explicit approval.
>
> **Purpose:** Define the planned database schema and serve as the EF Core
> implementation contract after approval.
>
> Do not generate entities, EF Core configurations, persistence code, or migrations
> while this document remains in Draft status.
>
> Items marked **TBD**, **Needs confirmation**, **Proposed**, **Recommended**, or
> **Blocking** must be resolved and explicitly approved before they are implemented.
>
> When the database design is approved, change the status to:
> **Approved for EF Core implementation**.
>
> **Authority order:** Approved Database Specification → approved Implementation Plan
> → ERD image as a visual reference.
> 
## 1. Scope and Implementation Rules

This specification defines the relational database for:

- public portfolio and project case studies;
- Visual Handbook infographics;
- series and reading paths;
- media management;
- authentication and authorization;
- engagement and analytics;
- contact messages; and
- audit, notification, and token infrastructure.

Some domains are future enhancements rather than MVP requirements. Their presence here documents the candidate data model; it does not authorize their implementation.

### Implementation requirements

1. Use SQL Server and Entity Framework Core.
2. Use `uniqueidentifier` / `Guid` primary keys.
3. Prefer `NEWSEQUENTIALID()` for database-generated GUID keys unless application-generated GUIDs are intentionally approved.
4. Configure entities through separate `IEntityTypeConfiguration<TEntity>` classes.
5. Do not expose EF Core entities directly through API responses.
6. Use explicit foreign keys, indexes, unique constraints, and delete behaviors.
7. Use UTC timestamps in application code.
8. Do not silently add columns or relationships not defined here.
9. Soft delete is not shown consistently in the ERD. Apply it only after an explicit project-wide decision.
10. String lengths shown as `nvarchar(TBD)` require confirmation before the initial migration.

## 2. Naming and Shared Conventions

### 2.1 Tables and keys

- Table names use PascalCase plural names.
- The standard primary key is `Id uniqueidentifier`.
- Foreign keys use `<EntityName>Id`.
- Junction tables use a surrogate `Id` plus a composite unique constraint unless a composite primary key is explicitly approved.

### 2.2 Common columns

Where present in the ERD:

- `CreatedAt datetime2`
- `UpdatedAt datetime2 NULL`
- `CreatedBy uniqueidentifier NULL`
- `UpdatedBy uniqueidentifier NULL`

> **Needs confirmation:** The ERD contains several unclear audit-field labels. Confirm the exact audit convention before code generation.

### 2.3 Public slugs

Public content slugs are required, URL-safe, case-normalized by application policy, and unique per table.

## 3. Users and Authorization

### 3.1 Users

Purpose: stores administrator and authenticated user accounts.

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserName | nvarchar(TBD) | No | Unique |
| Email | nvarchar(TBD) | No | Unique |
| PasswordHash | nvarchar(TBD) | No | Never return through API |
| FullName | nvarchar(TBD) | No | |
| IsActive | bit | No | Default `1` proposed |
| EmailVerified | bit | No | Default `0` proposed |
| CreatedAt | datetime2 | No | UTC |
| UpdatedAt | datetime2 | Yes | UTC |

**Indexes:** `UX_Users_UserName`, `UX_Users_Email`.

**Relationships:** one User to many UserRoles, Sessions, RefreshTokens, PasswordResetTokens, EmailVerificationTokens, Notifications, AuditLogs, and authenticated engagement records.

### 3.2 Roles

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | Unique |
| Description | nvarchar(TBD) | Yes | |

One Role has many UserRoles and RolePermissions.

### 3.3 Permissions

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | Unique |
| Description | nvarchar(TBD) | Yes | |

### 3.4 UserRoles

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | No | FK → Users.Id |
| RoleId | uniqueidentifier | No | FK → Roles.Id |
| CreatedAt | datetime2 | No | UTC |

- Unique constraint: `(UserId, RoleId)`.
- User deletion: Restrict or Cascade — **Needs confirmation**.
- Role deletion: Restrict — **Recommended**.

### 3.5 RolePermissions

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| RoleId | uniqueidentifier | No | FK → Roles.Id |
| PermissionId | uniqueidentifier | No | FK → Permissions.Id |

Unique constraint: `(RoleId, PermissionId)`.

## 4. Content Structure

### 4.1 Categories

Purpose: hierarchical taxonomy for infographics and optionally other content.

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | Unique |
| Slug | nvarchar(TBD) | No | Unique |
| Description | nvarchar(max) | Yes | |
| ParentId | uniqueidentifier | Yes | Self FK → Categories.Id |
| Icon | nvarchar(TBD) | Yes | Icon key or asset reference |
| DisplayOrder | int | No | Default `0` proposed |
| IsActive | bit | No | Default `1` proposed |
| CreatedAt | datetime2 | No | UTC |
| UpdatedAt | datetime2 | Yes | UTC |

Parent deletion should be restricted. A category used by published content must not be deleted without reassignment.

### 4.2 Tags

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | Unique |
| Slug | nvarchar(TBD) | No | Unique |
| CreatedAt | datetime2 | No | UTC |

### 4.3 Series

Purpose: ordered collection of related infographics.

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | |
| Slug | nvarchar(TBD) | No | Unique |
| Description | nvarchar(max) | Yes | |
| DisplayOrder | int | No | Default `0` proposed |
| IsActive | bit | No | Default `1` proposed |
| CreatedAt | datetime2 | No | UTC |
| UpdatedAt | datetime2 | Yes | UTC |

### 4.4 SeriesItems

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| SeriesId | uniqueidentifier | No | FK → Series.Id |
| InfographicId | uniqueidentifier | No | FK → Infographics.Id |
| Position | int | No | Must be positive |

- Unique: `(SeriesId, InfographicId)`.
- Unique `(SeriesId, Position)` — **Proposed; Needs confirmation**.

## 5. Visual Handbook / Knowledge Base

### 5.1 Infographics

Purpose: stores public Visual Handbook entries and publishing metadata.

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Title | nvarchar(TBD) | No | |
| Slug | nvarchar(TBD) | No | Unique |
| ShortDescription | nvarchar(TBD) | No | |
| Description | nvarchar(max) | Yes | Markdown/rich-text policy TBD |
| CategoryId | uniqueidentifier | No | FK → Categories.Id |
| SeriesId | uniqueidentifier | Yes | May duplicate SeriesItems; see decision |
| DifficultyLevel | tinyint | No | Enum |
| ViewCount | int | No | Default `0` |
| PublicCount | int | No | ERD label unclear; **Needs confirmation** |
| CreatedAt | datetime2 | No | ERD label unclear |
| CreatedBy | uniqueidentifier | Yes | FK → Users.Id proposed |
| UpdatedAt | datetime2 | Yes | |
| UpdatedBy | uniqueidentifier | Yes | Proposed; ERD label unclear |

**Blocking relationship decision:** the ERD shows both `Infographics.SeriesId` and `SeriesItems(SeriesId, InfographicId, Position)`. Keep only one unless both are explicitly justified. The recommended approach is to remove `Infographics.SeriesId` and make SeriesItems authoritative.

An Infographic belongs to a Category and has many InfographicTags, InfographicSteps, InfographicResources, InfographicCodeExamples, and SeriesItems. Media may be referenced by child `ImageId` values and/or a future explicit cover-media FK.

### 5.2 InfographicTags

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| InfographicId | uniqueidentifier | No | FK → Infographics.Id |
| TagId | uniqueidentifier | No | FK → Tags.Id |

Unique: `(InfographicId, TagId)`.

### 5.3 InfographicSteps

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| InfographicId | uniqueidentifier | No | FK → Infographics.Id |
| StepNumber | int | No | |
| Title | nvarchar(TBD) | No | |
| Content | nvarchar(max) | Yes | |
| ImageId | uniqueidentifier | Yes | FK → MediaFiles.Id |
| DisplayOrder | int | No | |

Unique: `(InfographicId, StepNumber)`.

### 5.4 InfographicResources

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| InfographicId | uniqueidentifier | No | FK → Infographics.Id |
| Title | nvarchar(TBD) | No | |
| Url | nvarchar(TBD) | No | Validate URL |
| ResourceType | nvarchar(TBD) | No | Enum/value-object candidate |
| DisplayOrder | int | No | |

### 5.5 InfographicCodeExamples

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| InfographicId | uniqueidentifier | No | FK → Infographics.Id |
| Title | nvarchar(TBD) | No | |
| Language | nvarchar(TBD) | No | |
| Code | nvarchar(max) | No | |
| FilePath | nvarchar(TBD) | Yes | |
| DisplayOrder | int | No | |

## 6. Projects

### 6.1 Projects

Purpose: stores public engineering case studies.

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Title | nvarchar(TBD) | No | |
| Slug | nvarchar(TBD) | No | Unique |
| ShortDescription | nvarchar(TBD) | No | |
| Description | nvarchar(max) | Yes | |
| ThumbnailImageId | uniqueidentifier | Yes | FK → MediaFiles.Id |
| LiveUrl | nvarchar(TBD) | Yes | |
| Status | tinyint | Yes | Enum; ERD shows nullable |
| CreatedBy | uniqueidentifier | Yes | ERD label/type unclear |
| CreatedAt | datetime2 | No | Proposed |
| UpdatedAt | datetime2 | Yes | |

**Needs confirmation:** `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, nullable status, and whether repository URL/confidentiality mode belongs here or in ProjectLinks.

### 6.2 Technologies

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | Unique |
| Icon | nvarchar(TBD) | Yes | |
| Category | nvarchar(TBD) | No | Vocabulary TBD |
| CreatedAt | datetime2 | No | UTC |

### 6.3 ProjectTechnologies

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| ProjectId | uniqueidentifier | No | FK → Projects.Id |
| TechnologyId | uniqueidentifier | No | FK → Technologies.Id |

Unique: `(ProjectId, TechnologyId)`.

### 6.4 ProjectImages

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| ProjectId | uniqueidentifier | No | FK → Projects.Id |
| ImageUrl | nvarchar(TBD) | No | ERD uses URL rather than MediaFile FK |
| AltText | nvarchar(TBD) | No | Accessibility |
| Caption | nvarchar(TBD) | Yes | |
| DisplayOrder | int | No | |

**Recommended review:** replace `ImageUrl` with `MediaFileId` to avoid competing media models.

### 6.5 ProjectLinks

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| ProjectId | uniqueidentifier | No | FK → Projects.Id |
| Title | nvarchar(TBD) | No | |
| Url | nvarchar(TBD) | No | |
| LinkType | nvarchar(TBD) | No | Demo, Repository, CaseStudy, External |
| DisplayOrder | int | No | |

## 7. Media and Files

### 7.1 MediaFiles

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| FileName | nvarchar(TBD) | No | |
| OriginalFileName | nvarchar(TBD) | No | ERD marks unique; confirm necessity |
| FilePath | nvarchar(TBD) | No | Blob key/path preferred over public URL |
| FileType | nvarchar(TBD) | No | MIME type |
| FileSize | bigint | No | |
| Width | int | Yes | Images |
| Height | int | Yes | Images |
| UploadedBy | uniqueidentifier | Yes | FK → Users.Id |
| UploadedAt | datetime2 | No | UTC |

**Needs confirmation:** uniqueness of OriginalFileName (normally not unique), checksum/hash, alt text, storage-provider metadata, and archival/soft-delete status.

### 7.2 MediaCollections *(future enhancement)*

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | Unique |
| Description | nvarchar(TBD) | Yes | |
| CreatedAt | datetime2 | No | UTC |

### 7.3 MediaCollectionItems *(future enhancement)*

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| CollectionId | uniqueidentifier | No | FK → MediaCollections.Id |
| MediaFileId | uniqueidentifier | No | FK → MediaFiles.Id |
| DisplayOrder | int | No | |

Unique: `(CollectionId, MediaFileId)`.

## 8. Reading Paths *(future enhancement)*

### 8.1 ReadingPaths

> The ERD child table uses `LearningPathId`, while its parent is named `ReadingPaths`. Standardize terminology before implementation.

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Title | nvarchar(TBD) | No | |
| Slug | nvarchar(TBD) | No | Unique |
| Description | nvarchar(max) | Yes | |
| Level | nvarchar(TBD) | No | Beginner/Intermediate/Advanced or enum |
| Icon | nvarchar(TBD) | Yes | |
| IsActive | bit | No | |
| DisplayOrder | int | No | |
| CreatedAt | datetime2 | No | UTC |
| UpdatedAt | datetime2 | Yes | UTC |

### 8.2 ReadingPathItems

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| ReadingPathId | uniqueidentifier | No | FK → ReadingPaths.Id |
| EntityType | nvarchar(TBD) | No | Polymorphic type |
| EntityId | uniqueidentifier | No | No enforceable SQL FK |
| Title | nvarchar(TBD) | Yes | Override title |
| Position | int | No | |
| IsOptional | bit | No | Default `0` |

- Unique: `(ReadingPathId, Position)`.
- Unique `(ReadingPathId, EntityType, EntityId)` — **Proposed; Needs confirmation**.
- ERD entity types: Infographic, Project, Article, Category, Series, ReadingPath.

> **Blocking:** `Article` appears in the polymorphic legend, but no Articles table is visible in the ERD.

## 9. Engagement and Interaction *(future enhancement except ContactMessages)*

### 9.1 UserBookmarks

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | No | FK → Users.Id |
| EntityType | nvarchar(TBD) | No | Polymorphic |
| EntityId | uniqueidentifier | No | |
| CreatedAt | datetime2 | No | UTC |

Unique: `(UserId, EntityType, EntityId)`.

### 9.2 UserRatings

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | No | FK → Users.Id |
| EntityType | nvarchar(TBD) | No | |
| EntityId | uniqueidentifier | No | |
| Rating | tinyint | No | Range `1–5` |
| CreatedAt | datetime2 | No | UTC |

Unique: `(UserId, EntityType, EntityId)`. Check: `Rating BETWEEN 1 AND 5`.

### 9.3 UserHelpfulVotes

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | No | FK → Users.Id |
| EntityType | nvarchar(TBD) | No | |
| EntityId | uniqueidentifier | No | |
| IsHelpful | bit | No | |
| CreatedAt | datetime2 | No | UTC |

Unique: `(UserId, EntityType, EntityId)`.

### 9.4 UserInteractions

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | Yes | Anonymous interactions permitted |
| EntityType | nvarchar(TBD) | No | |
| EntityId | uniqueidentifier | No | |
| InteractionType | nvarchar(TBD) | No | View, Like, Download, Share |
| IpAddress | nvarchar(TBD) | Yes | Privacy review required |
| UserAgent | nvarchar(TBD) | Yes | Privacy review required |
| CreatedAt | datetime2 | No | UTC |

Project 00 requires purposeful analytics and avoidance of excessive surveillance. Retention, anonymization, and consent must be defined.

### 9.5 ContactMessages

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Name | nvarchar(TBD) | No | |
| Email | nvarchar(TBD) | No | |
| Subject | nvarchar(TBD) | No | |
| Message | nvarchar(max) | No | |
| PageRoute | nvarchar(TBD) | Yes | Present in ERD |
| Referrer | nvarchar(TBD) | Yes | Privacy review |
| Status | tinyint | No | Enum |
| CreatedAt | datetime2 | No | UTC |
| UpdatedAt | datetime2 | Yes | UTC |

ContactStatus: `0 = New`, `1 = InProgress`, `2 = Closed`.

## 10. Analytics and Tracking *(future enhancement)*

### 10.1 PageViews

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Url | nvarchar(TBD) | No | |
| Title | nvarchar(TBD) | No | |
| Referrer | nvarchar(TBD) | Yes | |
| UserId | uniqueidentifier | Yes | FK → Users.Id |
| SessionId | uniqueidentifier | Yes | FK → Sessions.Id |
| Country | nvarchar(TBD) | Yes | |
| Device | nvarchar(TBD) | Yes | |
| Browser | nvarchar(TBD) | Yes | |
| CreatedAt | datetime2 | No | UTC |

Indexes: CreatedAt, Url, UserId, SessionId.

### 10.2 Sessions

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| SessionId | nvarchar(TBD) | No | Unique |
| UserId | uniqueidentifier | Yes | FK → Users.Id |
| IpAddress | nvarchar(TBD) | Yes | Privacy review |
| Device | nvarchar(TBD) | Yes | |
| Browser | nvarchar(TBD) | Yes | |
| Country | nvarchar(TBD) | Yes | |
| StartedAt | datetime2 | No | UTC |
| EndedAt | datetime2 | Yes | UTC |

Indexes: UserId, StartedAt.

### 10.3 DailyStats

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| Date | date | No | Unique proposed |
| VisitorCount | int | No | |
| SessionCount | int | No | |
| PageViewCount | int | No | |
| UniqueUsers | int | No | ERD label needs verification |
| BounceRate | decimal(TBD) | Yes | |
| CreatedAt | datetime2 | No | UTC |

### 10.4 PopularSearches

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| SearchTerm | nvarchar(TBD) | No | Unique |
| SearchCount | int | No | Default `0` |

### 10.5 EntityStatistics

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| EntityType | nvarchar(TBD) | No | |
| EntityId | uniqueidentifier | No | |
| ViewCount | int | No | Default `0` |
| UniqueViewCount | int | No | Default `0` |
| DownloadCount | int | No | Default `0` |
| ShareCount | int | No | Default `0` |
| HelpfulCount | int | No | Default `0` |
| RatingAverage | decimal(TBD) | No | Default `0` |
| UpdatedAt | datetime2 | No | UTC |

Unique: `(EntityType, EntityId)`. This is a denormalized aggregate/cache; source events remain authoritative.

## 11. System Tables

### 11.1 SiteSettings

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| SettingKey | nvarchar(TBD) | No | Unique |
| SettingValue | nvarchar(max) | Yes | |
| Description | nvarchar(TBD) | Yes | |
| IsEncrypted | bit | No | Default `0` |
| UpdatedAt | datetime2 | Yes | UTC |

Do not store production secrets here when Key Vault or environment configuration is appropriate.

### 11.2 AuditLogs

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | Yes | FK → Users.Id |
| Action | nvarchar(TBD) | No | |
| EntityType | nvarchar(TBD) | No | |
| EntityId | uniqueidentifier | Yes | |
| OldValues | nvarchar(max) | Yes | JSON |
| NewValues | nvarchar(max) | Yes | JSON |
| IpAddress | nvarchar(TBD) | Yes | Privacy review |
| CreatedAt | datetime2 | No | UTC |

### 11.3 Notifications *(future enhancement)*

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | Yes | FK → Users.Id |
| Title | nvarchar(TBD) | No | |
| Message | nvarchar(max) | No | |
| Type | tinyint | No | Enum |
| IsRead | bit | No | Default `0` |
| ReadAt | datetime2 | Yes | UTC |
| CreatedAt | datetime2 | No | UTC |

Types: Info, Success, Warning, Error, System. Numeric values need confirmation.

### 11.4 RefreshTokens

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | No | FK → Users.Id |
| TokenHash | nvarchar(TBD) | No | Unique; never store raw token |
| ExpiresAt | datetime2 | No | UTC |
| RevokedAt | datetime2 | Yes | UTC |
| CreatedAt | datetime2 | No | UTC |
| CreatedByIp | nvarchar(TBD) | Yes | Privacy review |
| ReplacedByTokenId | uniqueidentifier | Yes | Self FK |
| ReplacedByIp | nvarchar(TBD) | Yes | ERD label unclear |

Indexes: `(UserId, RevokedAt)` and unique TokenHash.

### 11.5 PasswordResetTokens

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | No | FK → Users.Id |
| TokenHash | nvarchar(TBD) | No | Unique |
| ExpiresAt | datetime2 | No | UTC |
| CreatedAt | datetime2 | No | UTC |
| UsedAt | datetime2 | Yes | UTC |
| IpUsed | nvarchar(TBD) | Yes | Privacy review |
| IpUsedFrom | nvarchar(TBD) | Yes | ERD label unclear |

Index: `(UserId, ExpiresAt)`.

### 11.6 EmailVerificationTokens

| Column | SQL type | Null | Constraints / notes |
|---|---|---:|---|
| Id | uniqueidentifier | No | PK |
| UserId | uniqueidentifier | No | FK → Users.Id |
| TokenHash | nvarchar(TBD) | No | Unique |
| ExpiresAt | datetime2 | No | UTC |
| CreatedAt | datetime2 | No | UTC |
| UsedAt | datetime2 | Yes | UTC |
| IpUsed | nvarchar(TBD) | Yes | Privacy review |

Index: `(UserId, ExpiresAt)`.

## 12. Enums Shown in the ERD

Values must be consistent in C#, SQL constraints, DTOs, and documentation.

| Enum | Values | Status |
|---|---|---|
| InfographicStatus | `0 Draft`, `1 Published`, `2 Archived` | No clear Infographics status column; confirmation required |
| ProjectStatus | `0 Draft`, `1 Published`, `2 Archived` | Confirm nullability |
| DifficultyLevel | `1 Beginner`, `2 Intermediate`, `3 Advanced` | Confirmed by ERD image |
| InteractionType | View, Like, Download, Share | Numeric mapping TBD |
| ContactStatus | `0 New`, `1 InProgress`, `2 Closed` | Confirmed by ERD image |
| NotificationType | Info, Success, Warning, Error, System | Numeric mapping TBD |

## 13. Index and Constraint Checklist

Codex must validate or generate:

- unique username and email;
- unique role and permission names;
- unique category name and slug;
- unique tag name and slug;
- unique series, project, infographic, and reading-path slugs;
- unique junction-table pairs and ordered positions where required;
- indexes on every foreign key and on public filter/sort columns;
- indexes on CreatedAt, publication/status fields, and analytics dates;
- check constraints for ratings and non-negative counters; and
- an approved concurrency strategy for aggregate counters.

## 14. Delete Behavior Recommendations

These recommendations require approval before migration generation.

| Relationship | Recommended behavior |
|---|---|
| User → UserRoles | Cascade |
| Role → UserRoles | Restrict |
| Role → RolePermissions | Cascade |
| Category parent → children | Restrict |
| Category → Infographics | Restrict |
| Infographic → child content tables | Cascade |
| Project → ProjectTechnologies / Images / Links | Cascade |
| MediaFile referenced by content | Restrict |
| Series → SeriesItems | Cascade |
| ReadingPath → ReadingPathItems | Cascade |
| User → audit/analytics history | Restrict or SetNull |
| User → auth tokens | Cascade |

## 15. Blocking Decisions Before EF Core Generation

1. Confirm exact string lengths.
2. Confirm all audit fields and their types.
3. Resolve `Infographics.SeriesId` versus SeriesItems.
4. Resolve `ProjectImages.ImageUrl` versus `MediaFileId`.
5. Confirm the unclear `PublicCount` field in Infographics.
6. Confirm whether Projects and Infographics include status and publication timestamps.
7. Confirm whether soft delete is global, selective, or unused.
8. Standardize ReadingPath versus LearningPath terminology.
9. Decide whether Article remains a supported polymorphic entity.
10. Confirm anonymous versus authenticated bookmarks, ratings, and helpful votes.
11. Confirm privacy and retention for IP address, user agent, country, session, and referrer data.
12. Confirm whether ASP.NET Core Identity replaces the custom Users/Roles/token design.
13. Confirm seed data: roles, permissions, categories, technologies, and one administrator.
14. Confirm whether counters are stored directly, event-derived, or cached in EntityStatistics.
15. Confirm all SQL delete behaviors.

No persistence code or initial migration may be generated until every applicable blocking decision is resolved.

## 16. Codex Usage Contract

Use this instruction before persistence implementation:

```text
Read Database_Specification.md before generating persistence code.

Treat confirmed sections as authoritative.
Treat ERD.png as a visual reference only.
Do not implement any item marked TBD, Needs confirmation, proposed, recommended,
or blocking without explicit approval.

First create Database_Review_Report.md listing:
- contradictions
- missing lengths
- unclear nullability
- relationship conflicts
- proposed resolutions

Do not generate the EF Core migration until blocking decisions are resolved.
```

## 17. Source Notes

This draft was derived from:

- the approved Project 00 master document;
- the supplied ERD image; and
- existing project decisions about Angular, ASP.NET Core, EF Core, SQL Server, Projects, Visual Handbook, series, reading paths, media, analytics, and the admin CMS.

Where the ERD image is unclear, this document marks the item for confirmation rather than guessing. The Database Specification is the authoritative written persistence contract; the ERD remains its visual companion.
