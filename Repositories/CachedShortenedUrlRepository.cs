using Microsoft.Extensions.Caching.Memory;
using UrlShortener.Entities;
using UrlShortener.Repositories.Interfaces;

namespace UrlShortener.Repositories;

public class CachedShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly ShortenedUrlRepository _decorated;
    private readonly IMemoryCache _memoryCache;

    public CachedShortenedUrlRepository(ShortenedUrlRepository decorated, IMemoryCache memoryCache)
    {
        _decorated = decorated;
        _memoryCache = memoryCache;
    }

    public Task<ShortenedUrl?> GetByCodeAsync(string code)
    {
        return _memoryCache.GetOrCreateAsync(
            code,
            _ => _decorated.GetByCodeAsync(code));
    }
}