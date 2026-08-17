using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Portfolio.Application.TestAnalytics;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.TestAnalytics;

internal sealed class TestArtifactContentService(
    PortfolioDbContext db,
    IConfiguration configuration,
    IHostEnvironment environment) : ITestArtifactContentService
{
    private static readonly IReadOnlyDictionary<string, string> PreviewTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp",
        ["video/webm"] = ".webm",
        ["video/mp4"] = ".mp4"
    };

    private static readonly IReadOnlySet<string> DownloadTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/webp", "video/webm", "video/mp4",
        "application/zip", "application/json", "application/xml", "text/xml", "text/html", "text/plain"
    };

    public async Task<TestArtifactContentResult> ResolveAsync(Guid artifactId, bool download, CancellationToken cancellationToken)
    {
        var item = await db.TestArtifacts.AsNoTracking().Where(x => x.Id == artifactId)
            .Select(x => new
            {
                x.Id,
                x.ArtifactType,
                x.Provider,
                x.Name,
                x.MimeType,
                x.StoragePath,
                x.ExpiresAtUtc,
                x.AvailabilityStatus,
                x.TestRun.ProviderRunId,
                x.TestRun.ExecutionMode,
                x.Browser
            }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
            return new(TestArtifactContentStatus.NotFound, Message: "The artifact does not exist.");
        if (item.AvailabilityStatus is TestArtifactAvailabilityStatus.Expired or TestArtifactAvailabilityStatus.Deleted ||
            item.ExpiresAtUtc is { } expires && expires <= DateTime.UtcNow)
            return new(TestArtifactContentStatus.Gone, Message: "Preview is no longer available for this artifact.");

        var reportArchive = download && item.ArtifactType == TestArtifactType.HtmlReport;
        var contentType = reportArchive ? "application/zip" : NormalizeContentType(item.MimeType, item.Name);
        var downloadName = reportArchive ? $"{SanitizeFileName(item.Name)}.zip" : item.Name;
        if ((!download && !PreviewTypes.ContainsKey(contentType)) || (download && !DownloadTypes.Contains(contentType)))
            return new(TestArtifactContentStatus.Unsupported, Message: download
                ? "This artifact type cannot be downloaded through the Quality API."
                : "Preview is not supported for this artifact type.");

        if (!TryNormalizeRelativePath(item.StoragePath, out var relativePath))
            return new(TestArtifactContentStatus.Unavailable, Message: "The artifact file reference is invalid.");

        var local = ResolveLocalPath(relativePath);
        if (local is not null && File.Exists(local))
            return Available(local, contentType, downloadName, download);
        if (local is not null && reportArchive && Directory.Exists(local))
        {
            var archive = GetCachePath(item.Id, ".zip");
            if (!File.Exists(archive))
                ZipFile.CreateFromDirectory(local, archive, CompressionLevel.Fastest, includeBaseDirectory: false);
            return Available(archive, contentType, downloadName, download);
        }

        if (item.Provider != TestArtifactProvider.GitHubActions)
            return new(TestArtifactContentStatus.Unavailable, Message: "The artifact file is unavailable from its external provider.");

        var token = configuration["TestArtifacts:GitHub:Token"];
        var repository = configuration["TestArtifacts:GitHub:Repository"];
        if (string.IsNullOrWhiteSpace(token) || !TryParseRepository(repository, out var owner, out var repo))
            return new(TestArtifactContentStatus.Unavailable, Message: "GitHub artifact preview is not configured on this server.");
        if (!long.TryParse(item.ProviderRunId, out var runId))
            return new(TestArtifactContentStatus.Unavailable, Message: "The GitHub workflow run reference is invalid.");

        try
        {
            var cached = await FetchGitHubFileAsync(item.Id, runId, owner, repo, token, relativePath,
                item.ArtifactType, item.ExecutionMode, item.Browser, item.Name, reportArchive, cancellationToken);
            return cached is null
                ? new(TestArtifactContentStatus.Unavailable, Message: "The artifact file was not found or has expired.")
                : Available(cached, contentType, downloadName, download);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or InvalidDataException or JsonException)
        {
            return new(TestArtifactContentStatus.Unavailable, Message: "The artifact provider is temporarily unavailable.");
        }
    }

    private TestArtifactContentResult Available(string path, string contentType, string name, bool download) =>
        new(TestArtifactContentStatus.Available, path, contentType, SanitizeFileName(name),
            !download && contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase));

    private string? ResolveLocalPath(string relativePath)
    {
        var configured = configuration["TestArtifacts:LocalRoot"] ?? "../..";
        var root = Path.GetFullPath(configured, environment.ContentRootPath);
        var candidate = Path.GetFullPath(relativePath.Replace('/', Path.DirectorySeparatorChar), root);
        return IsWithin(root, candidate) ? candidate : null;
    }

    private async Task<string?> FetchGitHubFileAsync(Guid artifactId, long runId, string owner, string repo,
        string token, string relativePath, TestArtifactType artifactType, TestExecutionMode mode,
        string? browser, string name, bool downloadContainer, CancellationToken cancellationToken)
    {
        var cacheRoot = GetCacheRoot();
        var extension = downloadContainer ? ".zip" : Path.GetExtension(name);
        var cachePath = Path.Combine(cacheRoot, artifactId.ToString("N") + extension);
        if (File.Exists(cachePath)) return cachePath;

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(GetInt("TestArtifacts:TimeoutSeconds", 45, 5, 120)) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Portfolio-Quality-Dashboard/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        var apiBase = (configuration["TestArtifacts:GitHub:ApiBaseUrl"] ?? "https://api.github.com").TrimEnd('/');
        using var listing = await client.GetAsync($"{apiBase}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/actions/runs/{runId}/artifacts?per_page=100", cancellationToken);
        if (listing.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone) return null;
        listing.EnsureSuccessStatusCode();
        await using var jsonStream = await listing.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken);
        var candidates = document.RootElement.GetProperty("artifacts").EnumerateArray()
            .Where(x => !x.GetProperty("expired").GetBoolean())
            .Select(x => new GitHubArtifact(x.GetProperty("name").GetString() ?? "", x.GetProperty("archive_download_url").GetString() ?? ""))
            .Where(x => MatchesContainer(x.Name, artifactType, mode, browser))
            .ToList();
        if (candidates.Count == 0) return null;

        var maxArchiveBytes = GetLong("TestArtifacts:MaxArchiveBytes", 268_435_456, 1_048_576, 1_073_741_824);
        var maxFileBytes = GetLong("TestArtifacts:MaxFileBytes", 134_217_728, 1_024, maxArchiveBytes);
        foreach (var candidate in candidates)
        {
            var temporaryZip = Path.Combine(cacheRoot, $"{artifactId:N}-{RandomNumberGenerator.GetHexString(8)}.zip");
            try
            {
                using var response = await client.GetAsync(candidate.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (response.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone) continue;
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength > maxArchiveBytes) throw new InvalidDataException("Artifact archive is too large.");
                await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken))
                await using (var destination = new FileStream(temporaryZip, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                    await CopyLimitedAsync(source, destination, maxArchiveBytes, cancellationToken);
                if (downloadContainer)
                {
                    File.Move(temporaryZip, cachePath, true);
                    return cachePath;
                }

                using var archive = ZipFile.OpenRead(temporaryZip);
                var expected = relativePath.Replace('\\', '/');
                var withoutRoot = expected.StartsWith("test-results/", StringComparison.OrdinalIgnoreCase)
                    ? expected["test-results/".Length..] : expected;
                var entry = archive.Entries.FirstOrDefault(x =>
                    string.Equals(NormalizeArchivePath(x.FullName), expected, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(NormalizeArchivePath(x.FullName), withoutRoot, StringComparison.OrdinalIgnoreCase))
                    ?? FindVisualEvidenceEntry(archive, withoutRoot, artifactType);
                if (entry is null || entry.Length > maxFileBytes) continue;
                var temporaryFile = cachePath + ".tmp";
                await using (var source = entry.Open())
                await using (var destination = new FileStream(temporaryFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                    await CopyLimitedAsync(source, destination, maxFileBytes, cancellationToken);
                File.Move(temporaryFile, cachePath, true);
                return cachePath;
            }
            finally
            {
                if (File.Exists(temporaryZip)) File.Delete(temporaryZip);
            }
        }
        return null;
    }

    private static bool MatchesContainer(string container, TestArtifactType type, TestExecutionMode mode, string? browser)
    {
        var prefix = type == TestArtifactType.HtmlReport ? "playwright-report-" : mode switch
        {
            TestExecutionMode.FullRecording => "playwright-full-recording-",
            TestExecutionMode.Visual => "playwright-visual-evidence-",
            _ => "playwright-failures-"
        };
        return container.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrWhiteSpace(browser) || container.EndsWith($"-{browser}", StringComparison.OrdinalIgnoreCase));
    }

    private static ZipArchiveEntry? FindVisualEvidenceEntry(ZipArchive archive, string path, TestArtifactType type)
    {
        if (type != TestArtifactType.Screenshot || !path.Contains("/attachments/", StringComparison.OrdinalIgnoreCase)) return null;
        var marker = path.IndexOf("/attachments/", StringComparison.OrdinalIgnoreCase);
        var directory = path[..marker];
        var fileName = Path.GetFileName(path);
        var extension = Path.GetExtension(fileName);
        var stem = Path.GetFileNameWithoutExtension(fileName);
        if (stem.Length > 41 && stem[^41] == '-' && stem[^40..].All(Uri.IsHexDigit)) stem = stem[..^41];
        var expected = $"{directory}/evidence/{stem}{extension}";
        return archive.Entries.FirstOrDefault(x => string.Equals(NormalizeArchivePath(x.FullName), expected, StringComparison.OrdinalIgnoreCase));
    }

    private string GetCacheRoot()
    {
        var root = Path.GetFullPath(configuration["TestArtifacts:CachePath"] ?? Path.Combine(Path.GetTempPath(), "portfolio-test-artifacts"), environment.ContentRootPath);
        Directory.CreateDirectory(root);
        return root;
    }

    private string GetCachePath(Guid id, string extension) => Path.Combine(GetCacheRoot(), id.ToString("N") + extension);
    private static async Task CopyLimitedAsync(Stream source, Stream destination, long limit, CancellationToken token)
    {
        var buffer = new byte[81920];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, token)) > 0)
        {
            total += read;
            if (total > limit) throw new InvalidDataException("Artifact content is too large.");
            await destination.WriteAsync(buffer.AsMemory(0, read), token);
        }
    }

    private static bool TryNormalizeRelativePath(string? value, out string path)
    {
        path = (value ?? "").Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(value) || path.Split('/').Any(x => x is ".." or "."))
        {
            path = "";
            return false;
        }
        return true;
    }

    private static string NormalizeArchivePath(string value) => value.Replace('\\', '/').TrimStart('/');
    private static bool IsWithin(string root, string candidate) => candidate.StartsWith(
        root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    private static bool TryParseRepository(string? value, out string owner, out string repo)
    {
        var parts = (value ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        owner = parts.Length == 2 ? parts[0] : "";
        repo = parts.Length == 2 ? parts[1] : "";
        return parts.Length == 2 && parts.All(x => x.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.'));
    }
    private static string NormalizeContentType(string? value, string name) => !string.IsNullOrWhiteSpace(value)
        ? value.Split(';')[0].Trim().ToLowerInvariant()
        : Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".webm" => "video/webm",
            ".mp4" => "video/mp4",
            ".zip" => "application/zip",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".html" => "text/html",
            _ => "application/octet-stream"
        };
    private static string SanitizeFileName(string value)
    {
        var safe = new string(Path.GetFileName(value).Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "artifact" : safe;
    }
    private int GetInt(string key, int fallback, int min, int max) => Math.Clamp(
        int.TryParse(configuration[key], out var value) ? value : fallback, min, max);
    private long GetLong(string key, long fallback, long min, long max) => Math.Clamp(
        long.TryParse(configuration[key], out var value) ? value : fallback, min, max);
    private sealed record GitHubArtifact(string Name, string DownloadUrl);
}
