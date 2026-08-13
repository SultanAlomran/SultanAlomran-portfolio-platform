using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Media;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Media;

internal sealed class MediaService(PortfolioDbContext db, IMediaStorage storage) : IMediaService
{
    private const long ImageLimit = 10 * 1024 * 1024;
    private const long PdfLimit = 20 * 1024 * 1024;

    public async Task<MediaPage> ListAsync(MediaQuery query, CancellationToken token)
    {
        var source = db.MediaFiles.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) source = source.Where(x => x.OriginalFileName.Contains(query.Search));
        if (query.Type == "image") source = source.Where(x => x.MimeType.StartsWith("image/"));
        if (query.Type == "pdf") source = source.Where(x => x.MimeType == "application/pdf");
        var referencedIds = await ReferencedIds(token);
        if (query.Usage == "referenced") source = source.Where(x => referencedIds.Contains(x.Id));
        if (query.Usage == "unreferenced") source = source.Where(x => !referencedIds.Contains(x.Id));
        source = query.Sort switch { "oldest" => source.OrderBy(x => x.UploadedAt), "name" => source.OrderBy(x => x.OriginalFileName), _ => source.OrderByDescending(x => x.UploadedAt) };
        var total = await source.CountAsync(token);
        var page = Math.Max(query.Page, 1);
        var size = Math.Clamp(query.PageSize, 1, 100);
        var rows = await source.Skip((page - 1) * size).Take(size).ToListAsync(token);
        var all = db.MediaFiles.AsNoTracking();
        return new MediaPage(rows.Select(x => Map(x, referencedIds)).ToList(), page, size, total,
            await all.CountAsync(x => x.MimeType.StartsWith("image/"), token), await all.CountAsync(x => x.MimeType == "application/pdf", token),
            await all.CountAsync(x => !referencedIds.Contains(x.Id), token));
    }

    public async Task<MediaFileDto?> GetAsync(Guid id, CancellationToken token)
    {
        var file = await db.MediaFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, token);
        if (file is null) return null;
        return Map(file, await ReferencedIds(token));
    }

    public async Task<MediaFileDto> UploadAsync(MediaUpload upload, CancellationToken token)
    {
        var extension = Path.GetExtension(upload.OriginalFileName).ToLowerInvariant();
        var allowed = extension switch { ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".webp" => "image/webp", ".pdf" => "application/pdf", _ => null };
        if (allowed is null) throw new InvalidMediaException("Only PNG, JPEG, WebP, and PDF files are supported.");
        var limit = allowed == "application/pdf" ? PdfLimit : ImageLimit;
        if (upload.Length <= 0 || upload.Length > limit) throw new InvalidMediaException($"The file must be between 1 byte and {limit / 1024 / 1024} MB.");
        await using var memory = new MemoryStream();
        await upload.Content.CopyToAsync(memory, token);
        var bytes = memory.ToArray();
        if (!MatchesSignature(allowed, bytes)) throw new InvalidMediaException("The file contents do not match its extension.");
        var key = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
        memory.Position = 0;
        await storage.StoreAsync(key, memory, token);
        try
        {
            var entity = MediaFile.Create(Path.GetFileName(key), Path.GetFileName(upload.OriginalFileName), storage.GetUrl(key), allowed, upload.Length, "local");
            db.MediaFiles.Add(entity);
            await db.SaveChangesAsync(token);
            return Map(entity, []);
        }
        catch
        {
            await storage.DeleteAsync(key, token);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        var file = await db.MediaFiles.SingleOrDefaultAsync(x => x.Id == id, token) ?? throw new KeyNotFoundException("Media file was not found.");
        if ((await ReferencedIds(token)).Contains(id)) throw new MediaInUseException("This media file is referenced by portfolio content and cannot be deleted.");
        db.MediaFiles.Remove(file);
        await db.SaveChangesAsync(token);
        await storage.DeleteAsync(file.FilePath, token);
    }

    private MediaFileDto Map(MediaFile x, HashSet<Guid> references) => new(x.Id, x.OriginalFileName, x.MimeType, x.FileSize, x.Width, x.Height, storage.GetUrl(x.FilePath), x.UploadedAt, references.Contains(x.Id), []);

    private async Task<HashSet<Guid>> ReferencedIds(CancellationToken token)
    {
        var ids = await db.ProjectImages.Select(x => x.MediaFileId)
            .Concat(db.Projects.Where(x => x.ThumbnailMediaFileId != null).Select(x => x.ThumbnailMediaFileId!.Value))
            .Concat(db.Infographics.Where(x => x.CoverMediaFileId != null).Select(x => x.CoverMediaFileId!.Value))
            .Concat(db.Infographics.Where(x => x.InfographicMediaFileId != null).Select(x => x.InfographicMediaFileId!.Value))
            .Concat(db.Infographics.Where(x => x.PdfMediaFileId != null).Select(x => x.PdfMediaFileId!.Value))
            .Concat(db.InfographicSteps.Where(x => x.MediaFileId != null).Select(x => x.MediaFileId!.Value))
            .Concat(db.Certifications.Where(x => x.MediaFileId != null).Select(x => x.MediaFileId!.Value))
            .Concat(db.Profiles.Where(x => x.ProfileImageMediaFileId != null).Select(x => x.ProfileImageMediaFileId!.Value))
            .Concat(db.Profiles.Where(x => x.CvMediaFileId != null).Select(x => x.CvMediaFileId!.Value))
            .Concat(db.MediaCollectionItems.Select(x => x.MediaFileId)).Distinct().ToListAsync(token);
        return ids.ToHashSet();
    }

    private static bool MatchesSignature(string type, byte[] b) => type switch
    {
        "image/png" => b.Length > 8 && b.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        "image/jpeg" => b.Length > 3 && b[0] == 255 && b[1] == 216 && b[2] == 255,
        "image/webp" => b.Length > 12 && b.AsSpan(0, 4).SequenceEqual("RIFF"u8) && b.AsSpan(8, 4).SequenceEqual("WEBP"u8),
        "application/pdf" => b.Length > 5 && b.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
        _ => false
    };
}
