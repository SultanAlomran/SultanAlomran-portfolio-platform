using Portfolio.Domain.Enums;

namespace Portfolio.Application.Projects;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record ProjectTechnologyDto(Guid Id, string Name, string Category, string? Icon);
public sealed record ProjectImageDto(Guid Id, Guid MediaFileId, string Url, string AltText, string? Caption, int DisplayOrder);
public sealed record ProjectLinkDto(Guid Id, string Title, string Url, string LinkType, int DisplayOrder);

public sealed record ProjectListItemDto(Guid Id, string Title, string Slug, string ShortDescription, string? ThumbnailUrl,
    bool IsFeatured, DateTime? PublishedAt, IReadOnlyList<ProjectTechnologyDto> Technologies);

public sealed record AdminProjectListItemDto(Guid Id, string Title, string Slug, string ShortDescription,
    string? ThumbnailUrl, ContentStatus Status, bool IsFeatured, DateTime CreatedAt, DateTime? PublishedAt,
    IReadOnlyList<ProjectTechnologyDto> Technologies);

public sealed record ProjectDetailsDto(Guid Id, string Title, string Slug, string ShortDescription,
    string? Description, string? BusinessProblem, string? Solution, string? Architecture, string? KeyFeatures,
    string? Challenges, string? Impact, string? LessonsLearned, string? ThumbnailUrl, string? LiveUrl,
    bool IsFeatured, DateTime? PublishedAt, IReadOnlyList<ProjectTechnologyDto> Technologies,
    IReadOnlyList<ProjectImageDto> Images, IReadOnlyList<ProjectLinkDto> Links);

public sealed record AdminProjectDetailsDto(Guid Id, string Title, string Slug, string ShortDescription,
    string? Description, string? BusinessProblem, string? Solution, string? Architecture, string? KeyFeatures,
    string? Challenges, string? Impact, string? LessonsLearned, Guid? ThumbnailMediaFileId, string? ThumbnailUrl,
    string? LiveUrl, ContentStatus Status, bool IsFeatured, DateTime CreatedAt, DateTime? PublishedAt,
    IReadOnlyList<ProjectTechnologyDto> Technologies, IReadOnlyList<ProjectImageDto> Images,
    IReadOnlyList<ProjectLinkDto> Links);

public sealed record ProjectQuery(string? Search = null, string? Technology = null, ContentStatus? Status = null,
    bool? Featured = null, string Sort = "newest", int Page = 1, int PageSize = 12);

public sealed record ProjectTechnologyRequest(Guid TechnologyId);
public sealed record ProjectImageRequest(Guid MediaFileId, string AltText, string? Caption, int DisplayOrder);
public sealed record ProjectLinkRequest(string Title, string Url, string LinkType, int DisplayOrder);

public sealed record UpsertProjectRequest(string Title, string Slug, string ShortDescription, string? Description,
    string? BusinessProblem, string? Solution, string? Architecture, string? KeyFeatures, string? Challenges,
    string? Impact, string? LessonsLearned, Guid? ThumbnailMediaFileId, string? LiveUrl, bool IsFeatured,
    IReadOnlyList<ProjectTechnologyRequest>? Technologies, IReadOnlyList<ProjectImageRequest>? Images,
    IReadOnlyList<ProjectLinkRequest>? Links);

public sealed record PublishReadinessResponse(bool IsReady, IReadOnlyList<string> MissingRequirements);
