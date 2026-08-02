using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal static class EntityRelationships
{
    public static void Configure<TEntity>(EntityTypeBuilder<TEntity> source) where TEntity : Entity
    {
        // Each entity configuration delegates here so cross-entity cascade decisions remain reviewable together.
        switch ((object)source)
        {
            case EntityTypeBuilder<User> b:
                b.HasIndex(x => x.UserName).IsUnique(); b.HasIndex(x => x.Email).IsUnique();
                break;
            case EntityTypeBuilder<Role> b: b.HasIndex(x => x.Name).IsUnique(); break;
            case EntityTypeBuilder<Permission> b: b.HasIndex(x => x.Name).IsUnique(); break;
            case EntityTypeBuilder<UserRole> b:
                b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
                b.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict); break;
            case EntityTypeBuilder<RolePermission> b:
                b.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
                b.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict); break;
            case EntityTypeBuilder<Profile> b:
                b.HasIndex(x => x.SingletonKey).IsUnique();
                b.HasOne(x => x.ProfileImageMediaFile).WithMany().HasForeignKey(x => x.ProfileImageMediaFileId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(x => x.CvMediaFile).WithMany().HasForeignKey(x => x.CvMediaFileId).OnDelete(DeleteBehavior.Restrict); break;
            case EntityTypeBuilder<ExperienceItem> b:
                b.HasIndex(x => new { x.StartDate, x.DisplayOrder }); b.ToTable(t => { t.HasCheckConstraint("CK_ExperienceItems_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]"); t.HasCheckConstraint("CK_ExperienceItems_DisplayOrder", "[DisplayOrder] >= 0"); }); break;
            case EntityTypeBuilder<SkillCategory> b: b.HasIndex(x => x.Name).IsUnique(); break;
            case EntityTypeBuilder<Skill> b:
                b.HasOne(x => x.SkillCategory).WithMany(x => x.Skills).HasForeignKey(x => x.SkillCategoryId).OnDelete(DeleteBehavior.Restrict);
                b.HasIndex(x => new { x.SkillCategoryId, x.Name }).IsUnique(); b.ToTable(t => t.HasCheckConstraint("CK_Skills_DisplayOrder", "[DisplayOrder] >= 0")); break;
            case EntityTypeBuilder<Certification> b:
                b.HasIndex(x => new { x.Name, x.Issuer }).IsUnique(); b.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).OnDelete(DeleteBehavior.Restrict);
                b.ToTable(t => t.HasCheckConstraint("CK_Certifications_DateRange", "[ExpiresDate] IS NULL OR [ExpiresDate] >= [IssuedDate]")); break;
            case EntityTypeBuilder<Category> b:
                b.HasQueryFilter(x => !x.IsDeleted); b.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0"); b.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0");
                b.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict); break;
            case EntityTypeBuilder<Tag> b: b.HasIndex(x => x.Name).IsUnique(); b.HasIndex(x => x.Slug).IsUnique(); break;
            case EntityTypeBuilder<Series> b: Soft(b); b.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0"); break;
            case EntityTypeBuilder<SeriesItem> b:
                b.HasIndex(x => new { x.SeriesId, x.InfographicId }).IsUnique(); b.HasIndex(x => new { x.SeriesId, x.Position }).IsUnique();
                b.HasOne(x => x.Series).WithMany(x => x.Items).HasForeignKey(x => x.SeriesId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Infographic).WithMany(x => x.SeriesItems).HasForeignKey(x => x.InfographicId).OnDelete(DeleteBehavior.Cascade);
                b.ToTable(t => t.HasCheckConstraint("CK_SeriesItems_Position", "[Position] > 0")); break;
            case EntityTypeBuilder<Infographic> b:
                Soft(b); b.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0"); b.HasIndex(x => new { x.Status, x.PublishedAt }); b.HasIndex(x => x.CreatedAt);
                b.HasOne(x => x.Category).WithMany(x => x.Infographics).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict); break;
            case EntityTypeBuilder<InfographicTag> b:
                b.HasIndex(x => new { x.InfographicId, x.TagId }).IsUnique(); b.HasOne(x => x.Infographic).WithMany(x => x.InfographicTags).HasForeignKey(x => x.InfographicId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); break;
            case EntityTypeBuilder<InfographicStep> b:
                b.HasIndex(x => new { x.InfographicId, x.StepNumber }).IsUnique(); b.HasOne(x => x.Infographic).WithMany(x => x.Steps).HasForeignKey(x => x.InfographicId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).OnDelete(DeleteBehavior.Restrict); DisplayOrder(b, "InfographicSteps"); break;
            case EntityTypeBuilder<InfographicResource> b: b.HasOne(x => x.Infographic).WithMany(x => x.Resources).HasForeignKey(x => x.InfographicId).OnDelete(DeleteBehavior.Cascade); DisplayOrder(b, "InfographicResources"); break;
            case EntityTypeBuilder<InfographicCodeExample> b: b.HasOne(x => x.Infographic).WithMany(x => x.CodeExamples).HasForeignKey(x => x.InfographicId).OnDelete(DeleteBehavior.Cascade); DisplayOrder(b, "InfographicCodeExamples"); break;
            case EntityTypeBuilder<Project> b:
                Soft(b); b.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0"); b.HasIndex(x => new { x.Status, x.PublishedAt }); b.HasIndex(x => x.CreatedAt); b.HasOne(x => x.ThumbnailMediaFile).WithMany().HasForeignKey(x => x.ThumbnailMediaFileId).OnDelete(DeleteBehavior.Restrict); break;
            case EntityTypeBuilder<Technology> b: b.HasIndex(x => x.Name).IsUnique(); break;
            case EntityTypeBuilder<ProjectTechnology> b:
                b.HasIndex(x => new { x.ProjectId, x.TechnologyId }).IsUnique(); b.HasOne(x => x.Project).WithMany(x => x.ProjectTechnologies).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x => x.Technology).WithMany(x => x.ProjectTechnologies).HasForeignKey(x => x.TechnologyId).OnDelete(DeleteBehavior.Restrict); break;
            case EntityTypeBuilder<ProjectImage> b:
                b.HasOne(x => x.Project).WithMany(x => x.Images).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).OnDelete(DeleteBehavior.Restrict); DisplayOrder(b, "ProjectImages"); break;
            case EntityTypeBuilder<ProjectLink> b: b.HasOne(x => x.Project).WithMany(x => x.Links).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade); DisplayOrder(b, "ProjectLinks"); break;
            case EntityTypeBuilder<MediaFile> b:
                b.HasIndex(x => x.FilePath).IsUnique(); b.HasOne(x => x.Uploader).WithMany(x => x.UploadedMediaFiles).HasForeignKey(x => x.UploadedBy).OnDelete(DeleteBehavior.SetNull);
                b.ToTable(t => { t.HasCheckConstraint("CK_MediaFiles_FileSize", "[FileSize] >= 0"); t.HasCheckConstraint("CK_MediaFiles_Width", "[Width] IS NULL OR [Width] > 0"); t.HasCheckConstraint("CK_MediaFiles_Height", "[Height] IS NULL OR [Height] > 0"); }); break;
            case EntityTypeBuilder<MediaCollection> b: b.HasIndex(x => x.Name).IsUnique(); break;
            case EntityTypeBuilder<MediaCollectionItem> b:
                b.HasIndex(x => new { x.MediaCollectionId, x.MediaFileId }).IsUnique(); b.HasOne(x => x.MediaCollection).WithMany(x => x.Items).HasForeignKey(x => x.MediaCollectionId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).OnDelete(DeleteBehavior.Restrict); DisplayOrder(b, "MediaCollectionItems"); break;
            case EntityTypeBuilder<ReadingPath> b: Soft(b); b.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0"); break;
            case EntityTypeBuilder<ReadingPathItem> b:
                b.HasIndex(x => new { x.ReadingPathId, x.Position }).IsUnique(); b.HasIndex(x => new { x.ReadingPathId, x.EntityType, x.EntityId }).IsUnique(); b.HasOne(x => x.ReadingPath).WithMany(x => x.Items).HasForeignKey(x => x.ReadingPathId).OnDelete(DeleteBehavior.Cascade); b.ToTable(t => t.HasCheckConstraint("CK_ReadingPathItems_Position", "[Position] > 0")); break;
            case EntityTypeBuilder<UserBookmark> b: Engagement(b, x => x.User); break;
            case EntityTypeBuilder<UserRating> b: Engagement(b, x => x.User); b.ToTable(t => t.HasCheckConstraint("CK_UserRatings_Rating", "[Rating] BETWEEN 1 AND 5")); break;
            case EntityTypeBuilder<UserHelpfulVote> b: Engagement(b, x => x.User); break;
            case EntityTypeBuilder<UserInteraction> b: b.HasIndex(x => new { x.EntityType, x.EntityId, x.InteractionType, x.CreatedAt }); b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); break;
            case EntityTypeBuilder<ContactMessage> b: b.HasIndex(x => new { x.Status, x.CreatedAt }); break;
            case EntityTypeBuilder<Session> b:
                b.HasIndex(x => x.SessionIdentifier).IsUnique(); b.HasIndex(x => x.StartedAt); b.HasOne(x => x.User).WithMany(x => x.Sessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_Sessions_DateRange", "[EndedAt] IS NULL OR [EndedAt] >= [StartedAt]")); break;
            case EntityTypeBuilder<PageView> b:
                b.HasIndex(x => x.CreatedAt); b.HasIndex(x => x.Url); b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction); b.HasOne(x => x.Session).WithMany(x => x.PageViews).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.SetNull); break;
            case EntityTypeBuilder<DailyStat> b: b.HasIndex(x => x.Date).IsUnique(); b.Property(x => x.BounceRate).HasPrecision(5, 2); b.ToTable(t => { t.HasCheckConstraint("CK_DailyStats_Counters", "[VisitorCount] >= 0 AND [SessionCount] >= 0 AND [PageViewCount] >= 0 AND [UniqueUsers] >= 0"); t.HasCheckConstraint("CK_DailyStats_BounceRate", "[BounceRate] IS NULL OR [BounceRate] BETWEEN 0 AND 100"); }); break;
            case EntityTypeBuilder<PopularSearch> b: b.HasIndex(x => x.SearchTerm).IsUnique(); b.ToTable(t => t.HasCheckConstraint("CK_PopularSearches_Count", "[SearchCount] >= 0")); break;
            case EntityTypeBuilder<EntityStatistic> b: b.HasIndex(x => new { x.EntityType, x.EntityId }).IsUnique(); b.Property(x => x.RatingAverage).HasPrecision(5, 2); b.ToTable(t => { t.HasCheckConstraint("CK_EntityStatistics_Counters", "[ViewCount] >= 0 AND [UniqueViewCount] >= 0 AND [DownloadCount] >= 0 AND [ShareCount] >= 0 AND [HelpfulCount] >= 0"); t.HasCheckConstraint("CK_EntityStatistics_RatingAverage", "[RatingAverage] BETWEEN 0 AND 5"); }); break;
            case EntityTypeBuilder<SiteSetting> b: b.HasIndex(x => x.SettingKey).IsUnique(); break;
            case EntityTypeBuilder<AuditLog> b: b.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt }); b.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); break;
            case EntityTypeBuilder<Notification> b: b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt }); b.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); break;
            case EntityTypeBuilder<RefreshToken> b:
                Token(b, x => x.User); b.HasOne(x => x.ReplacedByToken).WithMany().HasForeignKey(x => x.ReplacedByTokenId).OnDelete(DeleteBehavior.NoAction); break;
            case EntityTypeBuilder<PasswordResetToken> b: Token(b, x => x.User); break;
            case EntityTypeBuilder<EmailVerificationToken> b: Token(b, x => x.User); break;
        }
    }

    private static void Soft<T>(EntityTypeBuilder<T> b) where T : SoftDeletableEntity { b.HasQueryFilter(x => !x.IsDeleted); }
    private static void DisplayOrder<T>(EntityTypeBuilder<T> b, string table) where T : Entity => b.ToTable(t => t.HasCheckConstraint($"CK_{table}_DisplayOrder", "[DisplayOrder] >= 0"));
    private static void Engagement<T>(EntityTypeBuilder<T> b, System.Linq.Expressions.Expression<Func<T, User>> nav) where T : Entity
    { b.HasIndex("UserId", "EntityType", "EntityId").IsUnique(); b.HasOne(nav).WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade); }
    private static void Token<T>(EntityTypeBuilder<T> b, System.Linq.Expressions.Expression<Func<T, User>> nav) where T : Entity
    { b.HasIndex("TokenHash").IsUnique(); b.HasIndex("UserId", "ExpiresAt"); b.HasOne(nav).WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade); }
}
