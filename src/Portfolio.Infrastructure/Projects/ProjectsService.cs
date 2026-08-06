using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Projects;
using Portfolio.Domain.Constants;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Projects;

internal sealed partial class ProjectsService(PortfolioDbContext dbContext) : IProjectsService
{
    public async Task<PagedResult<ProjectListItemDto>> GetPublicProjectsAsync(ProjectQuery request, CancellationToken cancellationToken)
    {
        var query = ApplyFilters(dbContext.Projects.AsNoTracking().Where(x => x.Status == ContentStatus.Published), request, false);
        var count = await query.CountAsync(cancellationToken);
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var items = await ApplySort(query, request.Sort).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ProjectListItemDto(x.Id, x.Title, x.Slug, x.ShortDescription,
                x.ThumbnailMediaFile == null ? null : x.ThumbnailMediaFile.FilePath, x.IsFeatured, x.PublishedAt,
                x.ProjectTechnologies.OrderBy(pt => pt.Technology.Name)
                    .Select(pt => new ProjectTechnologyDto(pt.Technology.Id, pt.Technology.Name, pt.Technology.Category, pt.Technology.Icon)).ToList()))
            .ToListAsync(cancellationToken);
        return new(items, page, pageSize, count);
    }

    public Task<ProjectDetailsDto?> GetPublicProjectBySlugAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Projects.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published && x.Slug == slug.ToLower())
            .Select(x => new ProjectDetailsDto(x.Id, x.Title, x.Slug, x.ShortDescription, x.Description,
                x.BusinessProblem, x.Solution, x.Architecture, x.KeyFeatures, x.Challenges, x.Impact,
                x.LessonsLearned, x.ThumbnailMediaFile == null ? null : x.ThumbnailMediaFile.FilePath, x.LiveUrl,
                x.IsFeatured, x.PublishedAt,
                x.ProjectTechnologies.OrderBy(pt => pt.Technology.Name)
                    .Select(pt => new ProjectTechnologyDto(pt.Technology.Id, pt.Technology.Name, pt.Technology.Category, pt.Technology.Icon)).ToList(),
                x.Images.OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProjectImageDto(i.Id, i.MediaFileId, i.MediaFile.FilePath, i.AltText, i.Caption, i.DisplayOrder)).ToList(),
                x.Links.OrderBy(l => l.DisplayOrder)
                    .Select(l => new ProjectLinkDto(l.Id, l.Title, l.Url, l.LinkType, l.DisplayOrder)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<AdminProjectListItemDto>> GetAdminProjectsAsync(ProjectQuery request, CancellationToken cancellationToken)
    {
        var query = ApplyFilters(dbContext.Projects.AsNoTracking(), request, true);
        var count = await query.CountAsync(cancellationToken);
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var items = await ApplySort(query, request.Sort).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AdminProjectListItemDto(x.Id, x.Title, x.Slug, x.ShortDescription,
                x.ThumbnailMediaFile == null ? null : x.ThumbnailMediaFile.FilePath, x.Status, x.IsFeatured,
                x.CreatedAt, x.PublishedAt,
                x.ProjectTechnologies.OrderBy(pt => pt.Technology.Name)
                    .Select(pt => new ProjectTechnologyDto(pt.Technology.Id, pt.Technology.Name, pt.Technology.Category, pt.Technology.Icon)).ToList()))
            .ToListAsync(cancellationToken);
        return new(items, page, pageSize, count);
    }

    public Task<AdminProjectDetailsDto?> GetAdminProjectAsync(Guid id, CancellationToken cancellationToken) =>
        ProjectDetailsQuery(id).SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ProjectTechnologyDto>> GetTechnologiesAsync(CancellationToken cancellationToken) =>
        await dbContext.Technologies.AsNoTracking().OrderBy(x => x.Category).ThenBy(x => x.Name)
            .Select(x => new ProjectTechnologyDto(x.Id, x.Name, x.Category, x.Icon)).ToListAsync(cancellationToken);

    public async Task<AdminProjectDetailsDto> CreateAsync(UpsertProjectRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, null, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var project = Project.Create(request.Title, request.Slug, request.ShortDescription);
        ApplyContent(project, request);
        AddRelationships(project, request);
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (await GetAdminProjectAsync(project.Id, cancellationToken))!;
    }

    public async Task<AdminProjectDetailsDto> UpdateAsync(Guid id, UpsertProjectRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, id, cancellationToken);
        var project = await dbContext.Projects.Include(x => x.ProjectTechnologies).Include(x => x.Images).Include(x => x.Links)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new ProjectNotFoundException(id);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        ApplyContent(project, request);
        dbContext.ProjectTechnologies.RemoveRange(project.ProjectTechnologies);
        dbContext.ProjectImages.RemoveRange(project.Images);
        dbContext.ProjectLinks.RemoveRange(project.Links);
        await dbContext.SaveChangesAsync(cancellationToken);
        AddRelationships(project, request);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (await GetAdminProjectAsync(id, cancellationToken))!;
    }

    public async Task<AdminProjectDetailsDto> SaveDraftAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await FindAsync(id, cancellationToken);
        project.SaveDraft();
        await dbContext.SaveChangesAsync(cancellationToken);
        return (await GetAdminProjectAsync(id, cancellationToken))!;
    }

    public async Task<PublishReadinessResponse> GetPublishReadinessAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await FindAsync(id, cancellationToken);
        var missing = GetMissingRequirements(project);
        return new(missing.Count == 0, missing);
    }

    public async Task<AdminProjectDetailsDto> PublishAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await FindAsync(id, cancellationToken);
        var missing = GetMissingRequirements(project);
        if (missing.Count > 0)
            throw new ProjectValidationException(new Dictionary<string, string[]> { ["publish"] = missing.ToArray() });
        project.Publish();
        await dbContext.SaveChangesAsync(cancellationToken);
        return (await GetAdminProjectAsync(id, cancellationToken))!;
    }

    public async Task<AdminProjectDetailsDto> ArchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await FindAsync(id, cancellationToken);
        project.Archive();
        await dbContext.SaveChangesAsync(cancellationToken);
        return (await GetAdminProjectAsync(id, cancellationToken))!;
    }

    public async Task<AdminProjectDetailsDto> SetFeaturedAsync(Guid id, bool featured, CancellationToken cancellationToken)
    {
        var project = await FindAsync(id, cancellationToken);
        project.SetFeatured(featured);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (await GetAdminProjectAsync(id, cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await FindAsync(id, cancellationToken);
        project.SoftDelete();
        project.SetFeatured(false);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<AdminProjectDetailsDto> ProjectDetailsQuery(Guid id) => dbContext.Projects.AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new AdminProjectDetailsDto(x.Id, x.Title, x.Slug, x.ShortDescription, x.Description,
            x.BusinessProblem, x.Solution, x.Architecture, x.KeyFeatures, x.Challenges, x.Impact, x.LessonsLearned,
            x.ThumbnailMediaFileId, x.ThumbnailMediaFile == null ? null : x.ThumbnailMediaFile.FilePath, x.LiveUrl,
            x.Status, x.IsFeatured, x.CreatedAt, x.PublishedAt,
            x.ProjectTechnologies.OrderBy(pt => pt.Technology.Name)
                .Select(pt => new ProjectTechnologyDto(pt.Technology.Id, pt.Technology.Name, pt.Technology.Category, pt.Technology.Icon)).ToList(),
            x.Images.OrderBy(i => i.DisplayOrder)
                .Select(i => new ProjectImageDto(i.Id, i.MediaFileId, i.MediaFile.FilePath, i.AltText, i.Caption, i.DisplayOrder)).ToList(),
            x.Links.OrderBy(l => l.DisplayOrder)
                .Select(l => new ProjectLinkDto(l.Id, l.Title, l.Url, l.LinkType, l.DisplayOrder)).ToList()));

    private async Task<Project> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Projects.SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new ProjectNotFoundException(id);

    private async Task ValidateAsync(UpsertProjectRequest request, Guid? currentId, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Title)) errors["title"] = ["Title is required."];
        else if (request.Title.Trim().Length > DatabaseLengths.Title) errors["title"] = [$"Title cannot exceed {DatabaseLengths.Title} characters."];
        if (string.IsNullOrWhiteSpace(request.Slug) || !SlugPattern().IsMatch(request.Slug)) errors["slug"] = ["Slug must contain lowercase letters, numbers, and single hyphens only."];
        else if (request.Slug.Length > DatabaseLengths.Slug) errors["slug"] = [$"Slug cannot exceed {DatabaseLengths.Slug} characters."];
        else if (await dbContext.Projects.AnyAsync(x => x.Slug == request.Slug.ToLower() && x.Id != currentId, cancellationToken))
            throw new ProjectConflictException($"The slug '{request.Slug}' is already in use.");
        if (string.IsNullOrWhiteSpace(request.ShortDescription)) errors["shortDescription"] = ["Short description is required."];
        else if (request.ShortDescription.Trim().Length > DatabaseLengths.ShortDescription) errors["shortDescription"] = [$"Short description cannot exceed {DatabaseLengths.ShortDescription} characters."];
        if (!IsValidUrl(request.LiveUrl)) errors["liveUrl"] = ["Live URL must be an absolute HTTP or HTTPS URL."];

        var technologies = request.Technologies ?? [];
        if (technologies.Select(x => x.TechnologyId).Distinct().Count() != technologies.Count) errors["technologies"] = ["Technologies cannot be duplicated."];
        else if (technologies.Count > 0 && await dbContext.Technologies.CountAsync(x => technologies.Select(t => t.TechnologyId).Contains(x.Id), cancellationToken) != technologies.Count)
            errors["technologies"] = ["One or more technologies do not exist."];

        var images = request.Images ?? [];
        if (images.Any(x => x.DisplayOrder < 0) || images.Select(x => x.DisplayOrder).Distinct().Count() != images.Count) errors["images"] = ["Gallery order must be unique and zero or greater."];
        if (images.Any(x => string.IsNullOrWhiteSpace(x.AltText))) errors["images.altText"] = ["Every gallery image requires alternative text."];
        var mediaIds = images.Select(x => x.MediaFileId).Append(request.ThumbnailMediaFileId ?? Guid.Empty).Where(x => x != Guid.Empty).Distinct().ToArray();
        if (mediaIds.Length > 0 && await dbContext.MediaFiles.CountAsync(x => mediaIds.Contains(x.Id), cancellationToken) != mediaIds.Length)
            errors["media"] = ["One or more media files do not exist."];

        var links = request.Links ?? [];
        if (links.Any(x => x.DisplayOrder < 0) || links.Select(x => x.DisplayOrder).Distinct().Count() != links.Count) errors["links"] = ["Link order must be unique and zero or greater."];
        if (links.Any(x => string.IsNullOrWhiteSpace(x.Title) || string.IsNullOrWhiteSpace(x.LinkType) || !IsValidUrl(x.Url)))
            errors["links"] = ["Every link requires a title, type, and absolute HTTP or HTTPS URL."];
        if (errors.Count > 0) throw new ProjectValidationException(errors);
    }

    private static void ApplyContent(Project project, UpsertProjectRequest request)
    {
        project.UpdateContent(request.Title, request.Slug, request.ShortDescription, request.Description,
            request.BusinessProblem, request.Solution, request.Architecture, request.KeyFeatures, request.Challenges,
            request.Impact, request.LessonsLearned, request.ThumbnailMediaFileId, request.LiveUrl);
        project.SetFeatured(request.IsFeatured);
    }

    private static void AddRelationships(Project project, UpsertProjectRequest request)
    {
        foreach (var item in request.Technologies ?? []) project.ProjectTechnologies.Add(ProjectTechnology.Create(item.TechnologyId));
        foreach (var item in request.Images ?? []) project.Images.Add(ProjectImage.Create(item.MediaFileId, item.AltText, item.Caption, item.DisplayOrder));
        foreach (var item in request.Links ?? []) project.Links.Add(ProjectLink.Create(item.Title, item.Url, item.LinkType, item.DisplayOrder));
    }

    private static List<string> GetMissingRequirements(Project project)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(project.Title)) missing.Add("Title is required.");
        if (string.IsNullOrWhiteSpace(project.Slug)) missing.Add("Slug is required.");
        if (string.IsNullOrWhiteSpace(project.ShortDescription)) missing.Add("Short description is required.");
        return missing;
    }

    private static IQueryable<Project> ApplyFilters(IQueryable<Project> query, ProjectQuery request, bool includeStatus)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.ShortDescription.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(request.Technology))
        {
            var technology = request.Technology.Trim();
            query = query.Where(x => x.ProjectTechnologies.Any(pt => pt.Technology.Name == technology));
        }
        if (includeStatus && request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.Featured.HasValue) query = query.Where(x => x.IsFeatured == request.Featured);
        return query;
    }

    private static IOrderedQueryable<Project> ApplySort(IQueryable<Project> query, string sort) => sort.ToLowerInvariant() switch
    {
        "title" => query.OrderBy(x => x.Title).ThenBy(x => x.Id),
        "oldest" => query.OrderBy(x => x.PublishedAt ?? x.CreatedAt).ThenBy(x => x.Id),
        _ => query.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt).ThenBy(x => x.Id)
    };

    private static int NormalizePage(int page) => Math.Max(1, page);
    private static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, 100);
    private static bool IsValidUrl(string? value) => string.IsNullOrWhiteSpace(value) ||
        (Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
