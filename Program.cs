using System.Text.RegularExpressions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using UrlShortener;
using UrlShortener.Entities;
using UrlShortener.Extensions;
using UrlShortener.Models;
using UrlShortener.Repositories;
using UrlShortener.Repositories.Interfaces;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<UrlShorteningService>();
builder.Services.AddScoped<ShortenedUrlRepository>();
builder.Services.AddScoped<IShortenedUrlRepository, CachedShortenedUrlRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.ApplyMigrations();

app.MapPost("api/shorten", async (
    ShortenUrlRequest request,
    UrlShorteningService urlShorteningService,
    ApplicationDbContext dbContext,
    HttpContext httpContext) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("The specified URL is invalid.");
    }

    var code = await urlShorteningService.GenerateUniqueCodeAsync();

    var shortenedUrl = new ShortenedUrl
    {
        Id = Guid.NewGuid(),
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
        LongUrl = request.Url,
        Code = code,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}",
        CreatedOnUtc = DateTime.UtcNow
    };

    dbContext.ShortenedUrls.Add(shortenedUrl);
    
    await dbContext.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);
});

app.MapGet("api/{code}", async (string code, IShortenedUrlRepository shortenedUrlRepository) =>
{
    const string invalidCharsPattern = $"[^{UrlShorteningService.Alphabet}]";
    if (code.Length != UrlShorteningService.NumberOfCharsInShortLink || Regex.IsMatch(code, invalidCharsPattern))
    {
        return Results.BadRequest("The specified code is invalid.");
    }

    var shortenedUrl = await shortenedUrlRepository.GetByCodeAsync(code);

    if (shortenedUrl is null)
    {
        return Results.NotFound();
    }

    return Results.Redirect(shortenedUrl.LongUrl);
});

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

app.Run();