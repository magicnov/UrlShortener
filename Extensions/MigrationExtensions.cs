using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Database.Migrate();
    }
}