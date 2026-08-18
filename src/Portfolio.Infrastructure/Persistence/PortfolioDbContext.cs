using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Abstractions;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence.Seed;

namespace Portfolio.Infrastructure.Persistence;

/// <summary>SQL Server persistence boundary for the complete portfolio relational model.</summary>
public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options), IPortfolioDbContext
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<DailyStat> DailyStats => Set<DailyStat>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<EntityStatistic> EntityStatistics => Set<EntityStatistic>();
    public DbSet<ExperienceItem> ExperienceItems => Set<ExperienceItem>();
    public DbSet<Infographic> Infographics => Set<Infographic>();
    public DbSet<InfographicCodeExample> InfographicCodeExamples => Set<InfographicCodeExample>();
    public DbSet<InfographicResource> InfographicResources => Set<InfographicResource>();
    public DbSet<InfographicStep> InfographicSteps => Set<InfographicStep>();
    public DbSet<InfographicTag> InfographicTags => Set<InfographicTag>();
    public DbSet<MediaCollection> MediaCollections => Set<MediaCollection>();
    public DbSet<MediaCollectionItem> MediaCollectionItems => Set<MediaCollectionItem>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PageView> PageViews => Set<PageView>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PopularSearch> PopularSearches => Set<PopularSearch>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<ProjectLink> ProjectLinks => Set<ProjectLink>();
    public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();
    public DbSet<ReadingPath> ReadingPaths => Set<ReadingPath>();
    public DbSet<ReadingPathItem> ReadingPathItems => Set<ReadingPathItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<SeriesItem> SeriesItems => Set<SeriesItem>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<TestRun> TestRuns => Set<TestRun>();
    public DbSet<TestCaseResult> TestCaseResults => Set<TestCaseResult>();
    public DbSet<TestArtifact> TestArtifacts => Set<TestArtifact>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<UserBookmark> UserBookmarks => Set<UserBookmark>();
    public DbSet<UserHelpfulVote> UserHelpfulVotes => Set<UserHelpfulVote>();
    public DbSet<UserInteraction> UserInteractions => Set<UserInteraction>();
    public DbSet<UserRating> UserRatings => Set<UserRating>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
        ReferenceDataSeed.Apply(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
