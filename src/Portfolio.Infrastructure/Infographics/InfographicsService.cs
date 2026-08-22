using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Infographics;
using Portfolio.Domain.Constants;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Infographics;

internal sealed partial class InfographicsService(PortfolioDbContext db) : IInfographicsService
{
    public async Task<InfographicPagedResult<InfographicListItemDto>> GetPublicAsync(InfographicQuery request, CancellationToken token)
    {
        var query = ApplyFilters(db.Infographics.AsNoTracking().Where(x => x.Status == ContentStatus.Published), request, false);
        var count = await query.CountAsync(token);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var items = await ApplySort(query, request.Sort).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(PublicListProjection).ToListAsync(token);
        return new(items, page, pageSize, count);
    }

    public async Task<IReadOnlyList<InfographicListItemDto>> GetFeaturedAsync(int count, CancellationToken token) =>
        await db.Infographics.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published && x.IsFeatured)
            .OrderByDescending(x => x.PublishedAt).ThenBy(x => x.Id)
            .Take(Math.Clamp(count, 1, 12)).Select(PublicListProjection).ToListAsync(token);

    public async Task<IReadOnlyList<InfographicListItemDto>> GetPublicByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken token)
    {
        var orderedIds = ids.Where(id => id != Guid.Empty).Distinct().Take(50).ToArray();
        if (orderedIds.Length == 0) return [];
        var items = await db.Infographics.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published && orderedIds.Contains(x.Id))
            .Select(PublicListProjection).ToListAsync(token);
        var positions = orderedIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);
        return items.OrderBy(x => positions[x.Id]).ToList();
    }

    public async Task<InfographicDetailsDto?> GetPublicBySlugAsync(string slug, CancellationToken token)
    {
        var item = await db.Infographics.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published && x.Slug == slug.ToLower())
            .Select(x => new InfographicDetailsDto(x.Id, x.Title, x.Slug, x.ShortDescription, x.Description,
                x.DifficultyLevel, x.IsFeatured, x.CreatedAt, x.UpdatedAt, x.PublishedAt,
                x.CoverMediaFile == null ? null : x.CoverMediaFile.FilePath,
                x.InfographicMediaFile == null ? null : x.InfographicMediaFile.FilePath,
                x.PdfMediaFile == null ? null : x.PdfMediaFile.FilePath,
                new(x.Category.Id, x.Category.Name, x.Category.Slug, x.Category.Description),
                x.InfographicTags.OrderBy(t => t.Tag.Name).Select(t => new InfographicTagDto(t.Tag.Id, t.Tag.Name, t.Tag.Slug)).ToList(),
                x.Steps.OrderBy(s => s.DisplayOrder).ThenBy(s => s.StepNumber)
                    .Select(s => new InfographicStepDto(s.Id, s.StepNumber, s.Title, s.Content, s.MediaFileId,
                        s.MediaFile == null ? null : s.MediaFile.FilePath, s.DisplayOrder)).ToList(),
                x.Resources.OrderBy(r => r.DisplayOrder)
                    .Select(r => new InfographicResourceDto(r.Id, r.Title, r.Url, r.ResourceType, r.DisplayOrder)).ToList(),
                x.CodeExamples.OrderBy(c => c.DisplayOrder)
                    .Select(c => new InfographicCodeExampleDto(c.Id, c.Title, c.Language, c.Code, c.FilePath, c.DisplayOrder)).ToList(),
                x.SeriesItems.OrderBy(s => s.Series.DisplayOrder).ThenBy(s => s.Series.Name)
                    .Select(s => new InfographicSeriesDto(s.Series.Id, s.Series.Name, s.Series.Slug, s.Position)).ToList(),
                null, null, new List<InfographicListItemDto>()))
            .SingleOrDefaultAsync(token);
        if (item is null) return null;

        var tagIds = item.Tags.Select(tag => tag.Id).ToArray();
        var seriesIds = item.Series.Select(series => series.Id).ToArray();
        var related = await db.Infographics.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published && x.Id != item.Id &&
                (x.CategoryId == item.Category.Id || x.InfographicTags.Any(t => tagIds.Contains(t.TagId)) ||
                 x.SeriesItems.Any(s => seriesIds.Contains(s.SeriesId))))
            .OrderByDescending(x => x.SeriesItems.Any(s => seriesIds.Contains(s.SeriesId)))
            .ThenByDescending(x => x.CategoryId == item.Category.Id)
            .ThenByDescending(x => x.InfographicTags.Count(t => tagIds.Contains(t.TagId)))
            .ThenByDescending(x => x.PublishedAt).ThenBy(x => x.Id)
            .Take(3).Select(PublicListProjection).ToListAsync(token);

        InfographicListItemDto? previous = null;
        InfographicListItemDto? next = null;
        var primarySeries = item.Series.FirstOrDefault();
        if (primarySeries is not null)
        {
            previous = await db.SeriesItems.AsNoTracking()
                .Where(x => x.SeriesId == primarySeries.Id && x.Position < primarySeries.Position &&
                    x.Infographic.Status == ContentStatus.Published)
                .OrderByDescending(x => x.Position).Select(SeriesListProjection).FirstOrDefaultAsync(token);
            next = await db.SeriesItems.AsNoTracking()
                .Where(x => x.SeriesId == primarySeries.Id && x.Position > primarySeries.Position &&
                    x.Infographic.Status == ContentStatus.Published)
                .OrderBy(x => x.Position).Select(SeriesListProjection).FirstOrDefaultAsync(token);
        }
        return item with { Previous = previous, Next = next, Related = related };
    }

    public async Task<InfographicPagedResult<AdminInfographicListItemDto>> GetAdminAsync(InfographicQuery request, CancellationToken token)
    {
        var query = ApplyFilters(db.Infographics.AsNoTracking(), request, true);
        var count = await query.CountAsync(token);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var items = await ApplySort(query, request.Sort).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(AdminListProjection).ToListAsync(token);
        return new(items, page, pageSize, count);
    }

    public Task<AdminInfographicDetailsDto?> GetAdminByIdAsync(Guid id, CancellationToken token) =>
        AdminDetailsQuery(id).SingleOrDefaultAsync(token);

    public async Task<IReadOnlyList<InfographicCategoryDto>> GetCategoriesAsync(CancellationToken token) =>
        await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new InfographicCategoryDto(x.Id, x.Name, x.Slug, x.Description)).ToListAsync(token);

    public async Task<IReadOnlyList<InfographicTagDto>> GetTagsAsync(CancellationToken token) =>
        await db.Tags.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new InfographicTagDto(x.Id, x.Name, x.Slug)).ToListAsync(token);

    public async Task<IReadOnlyList<InfographicMediaDto>> GetMediaAsync(CancellationToken token) =>
        await db.MediaFiles.AsNoTracking().OrderByDescending(x => x.UploadedAt)
            .Select(x => new InfographicMediaDto(x.Id, x.FileName, x.OriginalFileName, x.FilePath,
                x.MimeType, x.FileSize, x.AltText, x.StorageProvider)).ToListAsync(token);

    public async Task<AdminInfographicDetailsDto> CreateAsync(UpsertInfographicRequest request, CancellationToken token)
    {
        await ValidateAsync(request, null, token);
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        var infographic = Infographic.Create(request.Title, request.Slug, request.ShortDescription,
            request.CategoryId, request.DifficultyLevel);
        ApplyContent(infographic, request);
        AddChildren(infographic, request);
        db.Infographics.Add(infographic);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return (await GetAdminByIdAsync(infographic.Id, token))!;
    }

    public async Task<AdminInfographicDetailsDto> UpdateAsync(Guid id, UpsertInfographicRequest request, CancellationToken token)
    {
        await ValidateAsync(request, id, token);
        var infographic = await db.Infographics.Include(x => x.InfographicTags).Include(x => x.Steps)
            .Include(x => x.Resources).Include(x => x.CodeExamples)
            .SingleOrDefaultAsync(x => x.Id == id, token) ?? throw new InfographicNotFoundException(id);
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        ApplyContent(infographic, request);
        db.InfographicTags.RemoveRange(infographic.InfographicTags);
        db.InfographicSteps.RemoveRange(infographic.Steps);
        db.InfographicResources.RemoveRange(infographic.Resources);
        db.InfographicCodeExamples.RemoveRange(infographic.CodeExamples);
        await db.SaveChangesAsync(token);
        AddChildren(infographic, request);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return (await GetAdminByIdAsync(id, token))!;
    }

    public async Task<AdminInfographicDetailsDto> SaveDraftAsync(Guid id, CancellationToken token)
    {
        var infographic = await FindAsync(id, token);
        infographic.SaveDraft();
        await db.SaveChangesAsync(token);
        return (await GetAdminByIdAsync(id, token))!;
    }

    public async Task<InfographicPublishReadinessResponse> GetPublishReadinessAsync(Guid id, CancellationToken token)
    {
        var infographic = await db.Infographics.AsNoTracking().Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.Id == id, token) ?? throw new InfographicNotFoundException(id);
        var missing = MissingRequirements(infographic);
        return new(missing.Count == 0, missing);
    }

    public async Task<AdminInfographicDetailsDto> PublishAsync(Guid id, CancellationToken token)
    {
        var infographic = await db.Infographics.Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.Id == id, token) ?? throw new InfographicNotFoundException(id);
        var missing = MissingRequirements(infographic);
        if (missing.Count > 0)
            throw new InfographicValidationException(new Dictionary<string, string[]> { ["publish"] = missing.ToArray() });
        infographic.Publish();
        await db.SaveChangesAsync(token);
        return (await GetAdminByIdAsync(id, token))!;
    }

    public async Task<AdminInfographicDetailsDto> ArchiveAsync(Guid id, CancellationToken token)
    {
        var infographic = await FindAsync(id, token);
        infographic.Archive();
        await db.SaveChangesAsync(token);
        return (await GetAdminByIdAsync(id, token))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        var infographic = await FindAsync(id, token);
        infographic.SoftDelete();
        await db.SaveChangesAsync(token);
    }

    private IQueryable<AdminInfographicDetailsDto> AdminDetailsQuery(Guid id) => db.Infographics.AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new AdminInfographicDetailsDto(x.Id, x.Title, x.Slug, x.ShortDescription, x.Description,
            x.CategoryId, x.DifficultyLevel, x.Status, x.IsFeatured, x.CreatedAt, x.UpdatedAt, x.PublishedAt,
            x.CoverMediaFileId, x.CoverMediaFile == null ? null : x.CoverMediaFile.FilePath,
            x.InfographicMediaFileId, x.InfographicMediaFile == null ? null : x.InfographicMediaFile.FilePath,
            x.PdfMediaFileId, x.PdfMediaFile == null ? null : x.PdfMediaFile.FilePath,
            x.InfographicTags.OrderBy(t => t.Tag.Name).Select(t => new InfographicTagDto(t.Tag.Id, t.Tag.Name, t.Tag.Slug)).ToList(),
            x.Steps.OrderBy(s => s.DisplayOrder).ThenBy(s => s.StepNumber)
                .Select(s => new InfographicStepDto(s.Id, s.StepNumber, s.Title, s.Content, s.MediaFileId,
                    s.MediaFile == null ? null : s.MediaFile.FilePath, s.DisplayOrder)).ToList(),
            x.Resources.OrderBy(r => r.DisplayOrder)
                .Select(r => new InfographicResourceDto(r.Id, r.Title, r.Url, r.ResourceType, r.DisplayOrder)).ToList(),
            x.CodeExamples.OrderBy(c => c.DisplayOrder)
                .Select(c => new InfographicCodeExampleDto(c.Id, c.Title, c.Language, c.Code, c.FilePath, c.DisplayOrder)).ToList(),
            x.SeriesItems.OrderBy(s => s.Position)
                .Select(s => new InfographicSeriesDto(s.Series.Id, s.Series.Name, s.Series.Slug, s.Position)).ToList()));

    private async Task<Infographic> FindAsync(Guid id, CancellationToken token) =>
        await db.Infographics.SingleOrDefaultAsync(x => x.Id == id, token) ?? throw new InfographicNotFoundException(id);

    private async Task ValidateAsync(UpsertInfographicRequest request, Guid? currentId, CancellationToken token)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Title)) errors["title"] = ["Title is required."];
        else if (request.Title.Trim().Length > DatabaseLengths.Title) errors["title"] = [$"Title cannot exceed {DatabaseLengths.Title} characters."];
        if (string.IsNullOrWhiteSpace(request.Slug) || !SlugPattern().IsMatch(request.Slug)) errors["slug"] = ["Slug must contain lowercase letters, numbers, and single hyphens only."];
        else if (request.Slug.Length > DatabaseLengths.Slug) errors["slug"] = [$"Slug cannot exceed {DatabaseLengths.Slug} characters."];
        else if (await db.Infographics.AnyAsync(x => x.Slug == request.Slug.ToLower() && x.Id != currentId, token))
            throw new InfographicConflictException($"The slug '{request.Slug}' is already in use.");
        if (string.IsNullOrWhiteSpace(request.ShortDescription)) errors["shortDescription"] = ["Short description is required."];
        else if (request.ShortDescription.Trim().Length > DatabaseLengths.ShortDescription) errors["shortDescription"] = [$"Short description cannot exceed {DatabaseLengths.ShortDescription} characters."];
        if (!Enum.IsDefined(request.DifficultyLevel)) errors["difficultyLevel"] = ["Difficulty level is invalid."];
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId && x.IsActive, token)) errors["categoryId"] = ["Select an active category."];

        var tagIds = request.TagIds ?? [];
        if (tagIds.Distinct().Count() != tagIds.Count) errors["tagIds"] = ["Tags cannot be duplicated."];
        else if (await db.Tags.CountAsync(x => tagIds.Contains(x.Id), token) != tagIds.Count) errors["tagIds"] = ["One or more tags do not exist."];

        var steps = request.Steps ?? [];
        if (steps.Any(x => x.StepNumber <= 0 || string.IsNullOrWhiteSpace(x.Title) || x.DisplayOrder < 0)) errors["steps"] = ["Every step requires a positive number, title, and non-negative order."];
        else if (steps.Select(x => x.StepNumber).Distinct().Count() != steps.Count || steps.Select(x => x.DisplayOrder).Distinct().Count() != steps.Count) errors["steps"] = ["Step numbers and display orders must be unique."];

        var resources = request.Resources ?? [];
        if (resources.Any(x => string.IsNullOrWhiteSpace(x.Title) || string.IsNullOrWhiteSpace(x.ResourceType) || !IsValidUrl(x.Url) || x.DisplayOrder < 0)) errors["resources"] = ["Resources require a title, type, valid HTTP/HTTPS URL, and non-negative order."];
        var examples = request.CodeExamples ?? [];
        if (examples.Any(x => string.IsNullOrWhiteSpace(x.Title) || string.IsNullOrWhiteSpace(x.Language) || string.IsNullOrWhiteSpace(x.Code) || x.DisplayOrder < 0)) errors["codeExamples"] = ["Code examples require a title, language, code, and non-negative order."];

        await ValidateMediaAsync(request, steps, errors, token);
        if (errors.Count > 0) throw new InfographicValidationException(errors);
    }

    private async Task ValidateMediaAsync(UpsertInfographicRequest request, IReadOnlyList<InfographicStepRequest> steps,
        Dictionary<string, string[]> errors, CancellationToken token)
    {
        var ids = new[] { request.CoverMediaFileId, request.InfographicMediaFileId, request.PdfMediaFileId }
            .Concat(steps.Select(x => x.MediaFileId)).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return;
        var media = await db.MediaFiles.AsNoTracking().Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, token);
        if (media.Count != ids.Count) { errors["media"] = ["One or more selected media files do not exist."]; return; }
        if (request.CoverMediaFileId is Guid cover && !media[cover].MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) errors["coverMediaFileId"] = ["Cover media must be an image."];
        if (request.InfographicMediaFileId is Guid image && !media[image].MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) errors["infographicMediaFileId"] = ["Main infographic media must be an image."];
        if (request.PdfMediaFileId is Guid pdf && !media[pdf].MimeType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)) errors["pdfMediaFileId"] = ["Download media must be a PDF."];
        if (steps.Any(x => x.MediaFileId is Guid id && !media[id].MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))) errors["steps"] = ["Step media must be an image."];
    }

    private static void ApplyContent(Infographic infographic, UpsertInfographicRequest request) =>
        infographic.UpdateContent(request.Title, request.Slug, request.ShortDescription, request.Description,
            request.CategoryId, request.DifficultyLevel, request.IsFeatured, request.CoverMediaFileId,
            request.InfographicMediaFileId, request.PdfMediaFileId);

    private static void AddChildren(Infographic infographic, UpsertInfographicRequest request)
    {
        foreach (var tagId in request.TagIds ?? []) infographic.InfographicTags.Add(InfographicTag.Create(tagId));
        foreach (var item in request.Steps ?? []) infographic.Steps.Add(InfographicStep.Create(item.StepNumber, item.Title, item.Content, item.MediaFileId, item.DisplayOrder));
        foreach (var item in request.Resources ?? []) infographic.Resources.Add(InfographicResource.Create(item.Title, item.Url, item.ResourceType, item.DisplayOrder));
        foreach (var item in request.CodeExamples ?? []) infographic.CodeExamples.Add(InfographicCodeExample.Create(item.Title, item.Language, item.Code, item.FilePath, item.DisplayOrder));
    }

    private static List<string> MissingRequirements(Infographic item)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(item.Description)) missing.Add("Add an introduction or description.");
        if (item.Steps.Count == 0) missing.Add("Add at least one content step.");
        return missing;
    }

    private static IQueryable<Infographic> ApplyFilters(IQueryable<Infographic> query, InfographicQuery request, bool admin)
    {
        if (!string.IsNullOrWhiteSpace(request.Search)) { var term = request.Search.Trim(); query = query.Where(x => x.Title.Contains(term) || x.ShortDescription.Contains(term)); }
        if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(x => x.Category.Slug == request.Category || x.Category.Name == request.Category);
        if (!string.IsNullOrWhiteSpace(request.Tag)) query = query.Where(x => x.InfographicTags.Any(t => t.Tag.Slug == request.Tag || t.Tag.Name == request.Tag));
        if (request.Difficulty.HasValue) query = query.Where(x => x.DifficultyLevel == request.Difficulty);
        if (request.Featured.HasValue) query = query.Where(x => x.IsFeatured == request.Featured);
        if (admin && request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        return query;
    }

    private static IOrderedQueryable<Infographic> ApplySort(IQueryable<Infographic> query, string sort) => sort.ToLowerInvariant() switch
    {
        "title" => query.OrderBy(x => x.Title).ThenBy(x => x.Id),
        "oldest" => query.OrderBy(x => x.PublishedAt ?? x.CreatedAt).ThenBy(x => x.Id),
        _ => query.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt).ThenBy(x => x.Id)
    };

    private static bool IsValidUrl(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static readonly Expression<Func<Infographic, InfographicListItemDto>> PublicListProjection = x =>
        new(x.Id, x.Title, x.Slug, x.ShortDescription, x.DifficultyLevel, x.IsFeatured, x.PublishedAt,
            x.CoverMediaFile == null ? null : x.CoverMediaFile.FilePath,
            new(x.Category.Id, x.Category.Name, x.Category.Slug, x.Category.Description),
            x.InfographicTags.OrderBy(t => t.Tag.Name).Select(t => new InfographicTagDto(t.Tag.Id, t.Tag.Name, t.Tag.Slug)).ToList());

    private static readonly Expression<Func<SeriesItem, InfographicListItemDto>> SeriesListProjection = x =>
        new(x.Infographic.Id, x.Infographic.Title, x.Infographic.Slug, x.Infographic.ShortDescription,
            x.Infographic.DifficultyLevel, x.Infographic.IsFeatured, x.Infographic.PublishedAt,
            x.Infographic.CoverMediaFile == null ? null : x.Infographic.CoverMediaFile.FilePath,
            new(x.Infographic.Category.Id, x.Infographic.Category.Name, x.Infographic.Category.Slug,
                x.Infographic.Category.Description),
            x.Infographic.InfographicTags.OrderBy(t => t.Tag.Name)
                .Select(t => new InfographicTagDto(t.Tag.Id, t.Tag.Name, t.Tag.Slug)).ToList());

    private static readonly Expression<Func<Infographic, AdminInfographicListItemDto>> AdminListProjection = x =>
        new(x.Id, x.Title, x.Slug, x.ShortDescription, x.DifficultyLevel, x.Status, x.IsFeatured,
            x.CreatedAt, x.UpdatedAt, x.PublishedAt, x.CoverMediaFile == null ? null : x.CoverMediaFile.FilePath,
            new(x.Category.Id, x.Category.Name, x.Category.Slug, x.Category.Description),
            x.InfographicTags.OrderBy(t => t.Tag.Name).Select(t => new InfographicTagDto(t.Tag.Id, t.Tag.Name, t.Tag.Slug)).ToList());

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
