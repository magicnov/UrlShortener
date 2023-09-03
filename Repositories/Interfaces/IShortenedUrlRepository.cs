using UrlShortener.Entities;

namespace UrlShortener.Repositories.Interfaces;

public interface IShortenedUrlRepository
{
    Task<ShortenedUrl?> GetByCodeAsync(string code);
}