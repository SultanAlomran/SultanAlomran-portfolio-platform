namespace Portfolio.Application.Media;

public sealed record MediaQuery(string? Search = null, string? Type = null, string? Usage = null,
    string Sort = "newest", int Page = 1, int PageSize = 24);
public sealed record MediaUsageDto(string Kind, Guid Id, string Label);
public sealed record MediaFileDto(Guid Id, string OriginalFileName, string ContentType, long Size,
    int? Width, int? Height, string Url, DateTime UploadedAt, bool IsReferenced,
    IReadOnlyList<MediaUsageDto> Usages);
public sealed record MediaPage(IReadOnlyList<MediaFileDto> Items, int Page, int PageSize, int TotalCount,
    int ImageCount, int PdfCount, int UnreferencedCount);
public sealed record MediaUpload(string OriginalFileName, string ContentType, long Length, Stream Content);

public interface IMediaService
{
    Task<MediaPage> ListAsync(MediaQuery query, CancellationToken token);
    Task<MediaFileDto?> GetAsync(Guid id, CancellationToken token);
    Task<MediaFileDto> UploadAsync(MediaUpload upload, CancellationToken token);
    Task DeleteAsync(Guid id, CancellationToken token);
}

public interface IMediaStorage
{
    Task StoreAsync(string key, Stream content, CancellationToken token);
    Task DeleteAsync(string key, CancellationToken token);
    string GetUrl(string key);
}

public sealed class InvalidMediaException(string message) : Exception(message);
public sealed class MediaInUseException(string message) : Exception(message);
