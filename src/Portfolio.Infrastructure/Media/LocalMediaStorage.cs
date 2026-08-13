using Microsoft.Extensions.Configuration;
using Portfolio.Application.Media;

namespace Portfolio.Infrastructure.Media;

internal sealed class LocalMediaStorage : IMediaStorage
{
    private readonly string _root;
    public LocalMediaStorage(IConfiguration configuration)
    {
        _root = Path.GetFullPath(configuration["Media:LocalPath"] ?? Path.Combine(AppContext.BaseDirectory, "media"));
        Directory.CreateDirectory(_root);
    }
    public async Task StoreAsync(string key, Stream content, CancellationToken token)
    {
        var path = Resolve(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await content.CopyToAsync(output, token);
    }
    public Task DeleteAsync(string key, CancellationToken token)
    {
        File.Delete(Resolve(key));
        return Task.CompletedTask;
    }

    public string GetUrl(string key) => key.StartsWith("/media/", StringComparison.Ordinal) ? key : $"/media/{Uri.EscapeDataString(key).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)}";

    private string Resolve(string key)
    {
        key = key.StartsWith("/media/", StringComparison.Ordinal) ? key[7..] : key;
        var path = Path.GetFullPath(Path.Combine(_root, key));
        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe media key.");
        return path;
    }
}
