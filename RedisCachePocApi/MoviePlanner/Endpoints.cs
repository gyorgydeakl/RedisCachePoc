using Microsoft.EntityFrameworkCore;
using RedisCachePocApi.Dal;

namespace MoviePlanner;

public static class Endpoints
{
    public static void MapMoviePlannerEndpoints(this WebApplication app)
    {
        app.MapGet("watchlist/{userId:guid}", async (Guid userId, AppDbContext db) =>
        {
            var items = await db.WatchlistItems
                .Where(w => w.UserId == userId)
                .Include(w => w.Movie)
                .Include(w => w.User)
                .Select(w => w.ToDto())
                .ToListAsync();
            return TypedResults.Ok(items);
        })
        .WithOpenApi()
        .WithName("GetWatchList");

        app.MapGet("users", async (AppDbContext db) =>
        {
            var users = await db.Users.Select(u => u.ToDto()).ToListAsync();
            return TypedResults.Ok(users);
        })
        .WithOpenApi()
        .WithName("GetUsers");
    }
}