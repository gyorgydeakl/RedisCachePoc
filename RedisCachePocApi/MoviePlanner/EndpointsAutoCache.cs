using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using RedisCachePocApi.Dal;

namespace MoviePlanner
{
    public static class EndpointsAutoCache
    {
        public static void MapAppEndpointsWithAutoCache(this WebApplication app)
        {
            app.MapGet("watchlist/{userId:guid}", async (Guid userId, AppDbContext db) =>
            {
                var dtoListFromDb = await db.WatchlistItems
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Movie)
                    .Include(w => w.User)
                    .Select(w => w.ToDto())
                    .ToListAsync();

                return TypedResults.Ok(dtoListFromDb);
            })
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)))
            .WithOpenApi()
            .WithName("GetWatchList");

            app.MapGet("movies/{id:guid}", async (Guid id, AppDbContext db) =>
            {
                var dbResult = await db.Movies
                    .Where(m => m.Id == id)
                    .Select(m => new { Movie = m, ReviewCount = m.Reviews.Count })
                    .SingleAsync();

                var movieDetails = dbResult.Movie.ToDetailsDto(dbResult.ReviewCount);
                return TypedResults.Ok(movieDetails);
            })
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(1)))
            .WithOpenApi()
            .WithName("GetMovie");

            app.MapGet("users", async (AppDbContext db) =>
            {
                var dtoListFromDb = await db.Users
                    .Select(u => u.ToDto())
                    .ToListAsync();

                return TypedResults.Ok(dtoListFromDb);
            })
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(10)))
            .WithOpenApi()
            .WithName("GetUsers");

            app.MapPost("watchlist/{id:guid}/generate", async (Guid id, [FromQuery] int count, AppDbContext db) =>
            {
                if (count <= 0)
                {
                    return Results.BadRequest("Parameter 'count' must be a positive number.");
                }

                var user = await db.Users
                    .Include(u => u.WatchList)
                    .Where(u => u.Id == id)
                    .SingleAsync();

                if (user is null)
                {
                    return Results.NotFound($"User {id} not found.");
                }

                var existingMovieIds = user.WatchList.Select(w => w.Id).ToHashSet();
                var candidates = await db.Movies
                    .Where(m => !existingMovieIds.Contains(m.Id))
                    .ToListAsync();

                if (candidates.Count == 0)
                {
                    return Results.BadRequest("No movies available to add to this watch-list.");
                }

                var rand = new Random();
                var toInsert = new List<WatchlistItem>();
                var take = Math.Min(count, candidates.Count);

                for (int i = 0; i < take; i++)
                {
                    var movie = candidates[rand.Next(candidates.Count)];
                    candidates.Remove(movie);

                    toInsert.Add(new WatchlistItem
                    {
                        UserId  = id,
                        MovieId = movie.Id
                    });
                }

                db.WatchlistItems.AddRange(toInsert);
                await db.SaveChangesAsync();

                return TypedResults.Ok(new
                {
                    UserId = id,
                    ItemsInserted = toInsert.Count
                });
            })
            .WithOpenApi()
            .WithName("GenerateWatchList");

            // GET user details
            app.MapGet("user/{userId:guid}", async (Guid userId, AppDbContext db) =>
            {
                var user = await db.Users.Where(u => u.Id == userId).SingleAsync();
                return TypedResults.Ok(user.ToDto());
            })
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(15)))
            .WithOpenApi()
            .WithName("GetUser");
        }
    }
}
