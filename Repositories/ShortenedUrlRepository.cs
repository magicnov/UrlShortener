using Microsoft.EntityFrameworkCore;
using UrlShortener.Entities;
using UrlShortener.Repositories.Interfaces;

namespace UrlShortener.Repositories;

public class ShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ShortenedUrlRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShortenedUrl?> GetByCodeAsync(string code) => 
        await _dbContext.ShortenedUrls.FirstOrDefaultAsync(s => s.Code == code);
}