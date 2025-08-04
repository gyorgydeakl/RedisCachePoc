using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCachePocApi.Dal;
using StackExchange.Redis;

namespace MoviePlanner;

public static class Endpoints
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapGet("watchlist/{userId:guid}", async (Guid userId, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var key = $"watchlist:{userId:N}";
            var cache = redis.GetDatabase();
            string? cachedJson = await cache.StringGetAsync(key);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                var dtoList = JsonSerializer.Deserialize<List<WatchlistItemDto>>(cachedJson)!;
                return TypedResults.Ok(dtoList);
            }

            var dtoListFromDb = await db.WatchlistItems
                .Where(w => w.UserId == userId)
                .Include(w => w.Movie)
                .Include(w => w.User)
                .Select(w => w.ToDto())
                .ToListAsync();

            await cache.StringSetAsync(
                key,
                JsonSerializer.Serialize(dtoListFromDb),
                expiry: TimeSpan.FromMinutes(5),
                flags: CommandFlags.FireAndForget);

            return TypedResults.Ok(dtoListFromDb);
        })
        .WithOpenApi()
        .WithName("GetWatchList");

        app.MapGet("/movies/{id:guid}", async (Guid id, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var key = $"movie:{id:N}";
            var cache = redis.GetDatabase();

            string? cachedJson = await cache.StringGetAsync(key);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                var dto = JsonSerializer.Deserialize<MovieDetailsDto>(cachedJson)!;
                return TypedResults.Ok(dto);
            }

            var dbResult = await db.Movies
                .Where(m => m.Id == id)
                .Select(m => new { Movie = m, ReviewCount = m.Reviews.Count})
                .SingleOrDefaultAsync();
            var movieDetails = dbResult?.Movie.ToDetailsDto(dbResult.ReviewCount);
            if (movieDetails is null)
            {
                return Results.NotFound();
            }

            await cache.StringSetAsync(
                key,
                JsonSerializer.Serialize(movieDetails),
                TimeSpan.FromMinutes(1),
                flags: CommandFlags.FireAndForget);

            return TypedResults.Ok(movieDetails);
        })
        .WithOpenApi()
        .WithName("GetMovie");

        app.MapGet("users", async (AppDbContext db, IConnectionMultiplexer redis) =>
        {
            const string key = "users:all";

            var cache = redis.GetDatabase();
            string? cachedJson = await cache.StringGetAsync(key);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                var dtoList = JsonSerializer.Deserialize<List<UserDto>>(cachedJson)!;
                return TypedResults.Ok(dtoList);
            }

            var dtoListFromDb = await db.Users
                .Select(u => u.ToDto())
                .ToListAsync();

            await cache.StringSetAsync(
                key: key,
                value: JsonSerializer.Serialize(dtoListFromDb),
                expiry: TimeSpan.FromMinutes(10),
                flags: CommandFlags.FireAndForget);

            return TypedResults.Ok(dtoListFromDb);
        })
        .WithOpenApi()
        .WithName("GetUsers");

        app.MapPost("/watchlist/{userId:guid}/generate", async (Guid userId, [FromQuery] int count, AppDbContext db) =>
        {
            if (count <= 0)
            {
                return Results.BadRequest("Parameter 'count' must be a positive number.");
            }

            var user = await db.Users
                .Include(u => u.WatchList)
                .Where(u => u.Id == userId)
                .SingleOrDefaultAsync();

            if (user is null)
            {
                return Results.NotFound($"User {userId} not found.");
            }

            var existingMovieIds = user.WatchList.Select(w => w.Id).ToHashSet();

            var candidates = await db.Movies
                .Where(m => !existingMovieIds.Contains(m.Id))
                .ToListAsync();

            if (candidates.Count == 0)
            {
                return Results.BadRequest("No movies available to add to this watch‑list.");
            }

            var rand      = new Random();
            var toInsert  = new List<WatchlistItem>();
            var take      = Math.Min(count, candidates.Count);

            for (int i = 0; i < take; i++)
            {
                var movie = candidates[rand.Next(candidates.Count)];
                candidates.Remove(movie);               // ensure uniqueness

                toInsert.Add(new WatchlistItem
                {
                    UserId  = userId,
                    MovieId = movie.Id
                });
            }

            db.WatchlistItems.AddRange(toInsert);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new
            {
                UserId = userId,
                ItemsInserted = toInsert.Count
            });
        })
        .WithOpenApi()
        .WithName("GenerateWatchList");

        app.MapGet("user/{userId:guid}", async (Guid userId, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var key = $"user:{userId:N}";

            var cache = redis.GetDatabase();
            string? cachedJson = await cache.StringGetAsync(key);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                var dto = JsonSerializer.Deserialize<UserDto>(cachedJson)!;
                return TypedResults.Ok(dto);
            }

            var user = await db.Users
                .Where(u => u.Id == userId)
                .SingleOrDefaultAsync();

            if (user is null)
            {
                return Results.NotFound();
            }

            var dtoFromDb = user.ToDto();

            await cache.StringSetAsync(
                key,
                JsonSerializer.Serialize(dtoFromDb),
                expiry: TimeSpan.FromMinutes(15),
                flags: CommandFlags.FireAndForget);

            return TypedResults.Ok(dtoFromDb);
        })
        .WithOpenApi()
        .WithName("GetUser");
    }
}