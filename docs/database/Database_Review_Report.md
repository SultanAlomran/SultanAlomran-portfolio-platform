# Database Review Report

## Sources reviewed

The approved feature brief was reviewed first, followed by `docs/Database_Specification.md`, `docs/Project_00_Master_Document.md`, `docs/Implementation_Plan.md`, `docs/ERD.dbml`, `docs/ERD.png`, `CONTRIBUTING.md`, and the root `README.md`. The feature brief resolves the draft specification's blocking questions and is authoritative.

## Approved model and inventory

The foundation contains **45 entities/tables**: Users, Roles, Permissions, UserRoles, RolePermissions; Profiles, ExperienceItems, SkillCategories, Skills, Certifications; Categories, Tags, Series, SeriesItems; Infographics, InfographicTags, InfographicSteps, InfographicResources, InfographicCodeExamples; Projects, Technologies, ProjectTechnologies, ProjectImages, ProjectLinks; MediaFiles, MediaCollections, MediaCollectionItems; ReadingPaths, ReadingPathItems; UserBookmarks, UserRatings, UserHelpfulVotes, UserInteractions; ContactMessages; PageViews, Sessions, DailyStats, PopularSearches, EntityStatistics; SiteSettings, AuditLogs, Notifications, RefreshTokens, PasswordResetTokens, and EmailVerificationTokens.

Article and `PublicCount` are explicitly omitted. `SeriesItems` is the sole ordered Series–Infographic association. `ProjectImages.MediaFileId` replaces `ImageUrl`. `EntityStatistics` is a denormalized cache; raw events remain authoritative.

## Relationships and deletion

Authorization uses ordered User/Role/Permission junctions. Profile and Certification optionally reference media; no User/Profile relationship is introduced. Categories self-reference and own Infographics. Infographics own steps, resources, examples, tags, and series memberships. Projects own technology memberships, media-backed images, and links. Collections own media items. Reading paths own polymorphic items. Users optionally own tracking/history and necessarily own authenticated engagement and tokens.

Ownership junctions/children cascade. Referenced taxonomy, technology, and media use Restrict. Historical audit/analytics, uploads, notifications, and anonymous-capable engagement use SetNull. Token ownership cascades; refresh-token replacement uses NoAction to avoid SQL Server cascade paths.

## Columns, Unicode, and auditing

Approved initial limits are centralized in `DatabaseLengths`: UserName 100, Email 320, FullName 200, PasswordHash 500, ShortName 150, Name 200, Title 250, Slug 200, ShortDescription 500, URL 2048, FileName 255, FilePath 1000, MIME type 150, Icon 200, EntityType 100, interaction value 50, IP 45, UserAgent 1000, token hash 500, issuer 250, verification code 500, job title 250, and organization 250. Long description/JSON/code/message values use `nvarchar(max)`. All human and bilingual text is Unicode (`nvarchar`) with no duplicate language columns.

Managed published content uses UTC `CreatedAt`, nullable `UpdatedAt`, and nullable actor IDs. Projects and Infographics include publishing audit fields. SQL `datetime2` is used throughout.

## Nullability, indexes, and constraints

Optional media, end dates, publication dates, audit actors, users on anonymous analytics/interactions, and privacy context are nullable. Required identity, names, slugs, ownership keys, hashes, event types, and core content are non-null.

Unique indexes cover user name/email, role/permission/name vocabularies, active public slugs/category names, junction pairs, ordered positions, token hashes, session identifiers, setting keys, entity statistic pairs, and media storage paths. Supporting indexes cover foreign keys, status/publication, public-content creation, analytics dates, and token expiry.

Named checks enforce ratings 1–5; nonnegative counters/file sizes/display orders; positive dimensions and ordered positions; bounce rate 0–100; rating average 0–5; and valid experience/session date ranges.

## Selective soft deletion

Only Category, Infographic, Project, Series, and ReadingPath have `IsDeleted`, `DeletedAt`, and `DeletedBy`, plus query filters. Active-row filtered unique indexes allow a deleted name/slug to be reused. A later admin workflow may call restore after resolving collisions and use `IgnoreQueryFilters()` for recovery views. Tokens, junctions, audit, sessions, and tracking remain visible.

## Professional profile

Profile stores the single approved professional summary and optional profile/CV media. A deterministic single-profile key is seeded nowhere; uniqueness is enforced through a singleton key. Experience supports date ordering and display order, skills belong to uniquely named categories, and certifications can reference media and verification URLs. No claims or achievements are seeded.

## Privacy and identity

`IpAddress`, `UserAgent`, `Referrer`, `Country`, `SessionId`, `Device`, and `Browser` are sensitive and require retention/access controls in later workflows. No fingerprint is modeled. Bookmarks, ratings, and helpful votes require users. Interactions permit a null user only for View, Download, or Share; PageViews and Sessions may be anonymous. Polymorphic `EntityType`/`EntityId` supports Infographic, Project, Category, Series, and ReadingPath; SQL Server cannot enforce a cross-table FK.

## Seed decisions

Only deterministic, non-secret roles, permissions, categories, technologies, skill categories, and safe settings are eligible. No users, credentials, hashes, tokens, analytics, achievements, secrets, or portfolio claims are seeded. Administrator account bootstrap is deferred.

## Remaining assumptions and resolved contradictions

The prompt approves future-enhancement tables now and overrides the draft status. Status enums are numeric with explicit values. Skill deletion is Restrict to prevent accidental professional-data loss. Certification uniqueness is `(Name, Issuer)`. The profile singleton uses a constant `SingletonKey`. `MimeType` is the canonical media type. The DBML's Article, direct Infographic SeriesId, ProjectImage URL, unclear PublicCount, nullable publishing status, and duplicate counters are superseded. No unresolved contradiction blocks implementation.

## Cleanup implementation review

The mapping review removed the centralized entity-type switch. Each entity configuration now directly exposes its relationships, indexes, query filters, check constraints, precision, and delete behavior, and typed expressions replace the former string-based engagement and token mappings. Public DbSet names now use correct plurals without changing entity class names or the established explicit table mappings. Credential-bearing base configuration was removed.

The model still contains exactly the approved 45 entities. No Article, `PublicCount`, direct `Infographic.SeriesId`, or `ProjectImage.ImageUrl` was introduced; `ProjectImage.MediaFileId` and `EntityStatistic` remain authoritative. Initial migration and real-SQL-Server validation are outstanding because this execution environment lacks .NET 10 and Docker.
