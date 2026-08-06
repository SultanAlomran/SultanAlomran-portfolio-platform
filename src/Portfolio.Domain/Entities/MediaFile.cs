using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class MediaFile : Entity
{
    private MediaFile() { }
    public static MediaFile Create(string fileName, string originalFileName, string filePath, string mimeType,
        long fileSize, string storageProvider, string? altText = null) => new()
        {
            FileName = fileName.Trim(),
            OriginalFileName = originalFileName.Trim(),
            FilePath = filePath.Trim(),
            MimeType = mimeType.Trim(),
            FileSize = fileSize,
            StorageProvider = storageProvider.Trim(),
            AltText = string.IsNullOrWhiteSpace(altText) ? null : altText.Trim()
        };
    public string FileName { get; private set; } = "";
    public string OriginalFileName { get; private set; } = "";
    public string FilePath { get; private set; } = "";
    public string MimeType { get; private set; } = "";
    public long FileSize { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public string? Checksum { get; private set; }
    public string? AltText { get; private set; }
    public string StorageProvider { get; private set; } = "";
    public Guid? UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;
    public User? Uploader { get; private set; }

}
