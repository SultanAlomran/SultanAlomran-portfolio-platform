using Portfolio.Domain.Enums;

namespace Portfolio.Application.Infographics;

public sealed record InfographicPagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record InfographicCategoryDto(Guid Id, string Name, string Slug, string? Description);
public sealed record InfographicTagDto(Guid Id, string Name, string Slug);
public sealed record InfographicMediaDto(Guid Id, string FileName, string OriginalFileName, string Url,
    string MimeType, long FileSize, string? AltText, string StorageProvider);
public sealed record InfographicStepDto(Guid Id, int StepNumber, string Title, string? Content,
    Guid? MediaFileId, string? MediaUrl, int DisplayOrder);
public sealed record InfographicResourceDto(Guid Id, string Title, string Url, string ResourceType, int DisplayOrder);
public sealed record InfographicCodeExampleDto(Guid Id, string Title, string Language, string Code,
    string? FilePath, int DisplayOrder);
public sealed record InfographicSeriesDto(Guid Id, string Name, string Slug, int Position);

public sealed record InfographicListItemDto(Guid Id, string Title, string Slug, string ShortDescription,
    DifficultyLevel DifficultyLevel, bool IsFeatured, DateTime? PublishedAt, string? CoverUrl,
    InfographicCategoryDto Category, IReadOnlyList<InfographicTagDto> Tags);

public sealed record AdminInfographicListItemDto(Guid Id, string Title, string Slug, string ShortDescription,
    DifficultyLevel DifficultyLevel, ContentStatus Status, bool IsFeatured, DateTime CreatedAt,
    DateTime? UpdatedAt, DateTime? PublishedAt, string? CoverUrl, InfographicCategoryDto Category,
    IReadOnlyList<InfographicTagDto> Tags);

public sealed record InfographicDetailsDto(Guid Id, string Title, string Slug, string ShortDescription,
    string? Description, DifficultyLevel DifficultyLevel, bool IsFeatured, DateTime? PublishedAt,
    string? CoverUrl, string? InfographicUrl, string? PdfUrl, InfographicCategoryDto Category,
    IReadOnlyList<InfographicTagDto> Tags, IReadOnlyList<InfographicStepDto> Steps,
    IReadOnlyList<InfographicResourceDto> Resources, IReadOnlyList<InfographicCodeExampleDto> CodeExamples,
    IReadOnlyList<InfographicSeriesDto> Series, IReadOnlyList<InfographicListItemDto> Related);

public sealed record AdminInfographicDetailsDto(Guid Id, string Title, string Slug, string ShortDescription,
    string? Description, Guid CategoryId, DifficultyLevel DifficultyLevel, ContentStatus Status,
    bool IsFeatured, DateTime CreatedAt, DateTime? UpdatedAt, DateTime? PublishedAt,
    Guid? CoverMediaFileId, string? CoverUrl, Guid? InfographicMediaFileId, string? InfographicUrl,
    Guid? PdfMediaFileId, string? PdfUrl, IReadOnlyList<InfographicTagDto> Tags,
    IReadOnlyList<InfographicStepDto> Steps, IReadOnlyList<InfographicResourceDto> Resources,
    IReadOnlyList<InfographicCodeExampleDto> CodeExamples, IReadOnlyList<InfographicSeriesDto> Series);

public sealed record InfographicQuery(string? Search = null, string? Category = null, string? Tag = null,
    ContentStatus? Status = null, DifficultyLevel? Difficulty = null, bool? Featured = null,
    string Sort = "newest", int Page = 1, int PageSize = 12);

public sealed record InfographicStepRequest(int StepNumber, string Title, string? Content,
    Guid? MediaFileId, int DisplayOrder);
public sealed record InfographicResourceRequest(string Title, string Url, string ResourceType, int DisplayOrder);
public sealed record InfographicCodeExampleRequest(string Title, string Language, string Code,
    string? FilePath, int DisplayOrder);

public sealed record UpsertInfographicRequest(string Title, string Slug, string ShortDescription,
    string? Description, Guid CategoryId, DifficultyLevel DifficultyLevel, bool IsFeatured,
    Guid? CoverMediaFileId, Guid? InfographicMediaFileId, Guid? PdfMediaFileId,
    IReadOnlyList<Guid>? TagIds, IReadOnlyList<InfographicStepRequest>? Steps,
    IReadOnlyList<InfographicResourceRequest>? Resources,
    IReadOnlyList<InfographicCodeExampleRequest>? CodeExamples);

public sealed record InfographicPublishReadinessResponse(bool IsReady, IReadOnlyList<string> MissingRequirements);
