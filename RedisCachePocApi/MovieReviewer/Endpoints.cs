using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCachePocApi.Dal;
using StackExchange.Redis;

namespace RedisCachePocApi.MovieReviewer;

public static class Endpoints
{
    public static void MapMovieReviewerEndpoints(this WebApplication app)
    {
        app.MapGet("/movies", async (AppDbContext db, IConnectionMultiplexer redis) =>
        {
            const string key = "movies:all";
            var cache = redis.GetDatabase();
            string? cachedJson = await cache.StringGetAsync(key);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                var dtoList = JsonSerializer.Deserialize<List<MovieSummaryDto>>(cachedJson)!;
                return TypedResults.Ok(dtoList);
            }

            var dtoListFromDb = await db.Movies.Select(m => m.ToSummaryDto()).ToListAsync();
            await cache.StringSetAsync(
                key,
                JsonSerializer.Serialize(dtoListFromDb),
                TimeSpan.FromMinutes(1),
                flags: CommandFlags.FireAndForget);

            return TypedResults.Ok(dtoListFromDb);
        })
        .WithOpenApi();

        app.MapPost("/movies", async ([FromBody] CreateMovieDto dto, AppDbContext db) =>
        {
            var newMovie = db.Movies.Add(dto.ToMovie()).Entity;
            await db.SaveChangesAsync();
            return TypedResults.Ok(newMovie.ToDetailsDto());
        })
        .WithOpenApi();

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

            var dtoFromDb = await db.Movies
                .Where(m => m.Id == id)
                .Include(m => m.Reviews)
                .ThenInclude(r => r.User)
                .Select(m => m.ToDetailsDto())
                .SingleOrDefaultAsync();

            if (dtoFromDb is null)
            {
                return Results.NotFound();
            }

            await cache.StringSetAsync(
                key,
                JsonSerializer.Serialize(dtoFromDb),
                TimeSpan.FromMinutes(1),
                flags: CommandFlags.FireAndForget);

            return TypedResults.Ok(dtoFromDb);
        })
        .WithOpenApi();

        app.MapPost("/movies/{id:guid}/reviews", async (Guid id, [FromBody] CreateReviewDto dto, AppDbContext db) =>
        {
            var newReview = db.Reviews.Add(dto.ToReview(id, DateTime.UtcNow)).Entity;
            await db.SaveChangesAsync();
            await db.Entry(newReview).Reference(r => r.User).LoadAsync();
            return TypedResults.Ok(newReview.ToDto());
        })
        .WithOpenApi();

        app.MapGet("/user/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var result = await db.Users.Where(u => u.Id == id).Select(u => u.ToDto()).SingleOrDefaultAsync();
            return TypedResults.Ok(result);
        })
        .WithOpenApi();
            
        app.MapGet("/users", async (AppDbContext db, IConnectionMultiplexer redis) =>
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

            var ttl = TimeSpan.FromMinutes(10);
            await cache.StringSetAsync(
                key,
                JsonSerializer.Serialize(dtoListFromDb),
                ttl,
                flags: CommandFlags.FireAndForget);

            return TypedResults.Ok(dtoListFromDb);
        })
        .WithOpenApi();

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
                    return Results.NotFound();

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

        app.MapPost("/users", async ([FromBody] CreateUserDto command, AppDbContext db) =>
        {
            var user = db.Users.Add(command.ToUser()).Entity;
            await db.SaveChangesAsync();
            return TypedResults.Ok(user.ToDto());
        })
        .WithOpenApi();

        app.MapPost("movies/generate", async ([FromQuery] int count, AppDbContext db, IConnectionMultiplexer redis) =>
            {
                if (count <= 0)
                {
                    return Results.BadRequest("Parameter 'count' must be a positive number.");
                }

                List<string> genres = ["Action", "Comedy", "Drama", "Sci‑Fi", "Thriller", "Fantasy"];

                var movieFaker = new Faker<Movie>()
                    .RuleFor(m => m.Title, f => f.Lorem.Sentence(3, 2))
                    .RuleFor(m => m.Genre, f => f.PickRandom(genres))
                    .RuleFor(m => m.Director, f => f.Name.FullName())
                    .RuleFor(m => m.Plot, f => f.Lorem.Paragraphs(1, 3));

                var movies = movieFaker.Generate(count);

                db.Movies.AddRange(movies);
                await db.SaveChangesAsync();

                await redis.GetDatabase().KeyDeleteAsync("movies:all");

                return TypedResults.Ok(new
                {
                    MoviesInserted = movies.Count
                });
            })
            .WithOpenApi()
            .WithName("GenerateMovies");

        app.MapPost("users/generate", async ([FromQuery] int count, AppDbContext db, IConnectionMultiplexer redis) =>
            {
                if (count <= 0)
                {
                    return Results.BadRequest("Parameter 'count' must be a positive number.");
                }
                var userFaker = new Bogus.Faker<User>()
                    .RuleFor(u => u.Email,    f => f.Internet.Email())
                    .RuleFor(u => u.Username, f => f.Internet.UserName())
                    .RuleFor(u => u.Bio,      f => f.Lorem.Paragraph());

                var users = userFaker.Generate(count);

                db.Users.AddRange(users);
                await db.SaveChangesAsync();

                await redis.GetDatabase().KeyDeleteAsync("users:all");

                return TypedResults.Ok(new
                {
                    UsersInserted = users.Count
                });
            })
            .WithOpenApi()
            .WithName("GenerateUsers");

        app.MapPost("/movies/{id:guid}/generate-reviews", 
            async (Guid id, [FromQuery] int count, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound($"Movie {id} not found.");
            }

            var users = await db.Users.ToListAsync();
            if (users.Count == 0)
            {
                var userFaker = new Faker<User>()
                    .RuleFor(u => u.Email, f => f.Internet.Email())
                    .RuleFor(u => u.Username, f => f.Internet.UserName())
                    .RuleFor(u => u.Bio, f => f.Lorem.Sentences(2));
                
                users = userFaker.Generate(Math.Max(5, count / 2));
                db.Users.AddRange(users);
                await db.SaveChangesAsync();          // so they get primary keys
            }

            var rand         = new Random();
            var reviewFaker  = new Faker<Review>()
                .RuleFor(r => r.Title,       f => f.Lorem.Sentence(4))
                .RuleFor(r => r.Description, f => f.Lorem.Paragraph())
                .RuleFor(r => r.Rating,      f => f.Random.Int(1, 5))
                .RuleFor(r => r.Date,        f => f.Date.Recent(60, DateTime.UtcNow));

            var reviewList = new List<Review>(count);
            for (var i = 0; i < count; i++)
            {
                var review   = reviewFaker.Generate();
                var user     = users[rand.Next(users.Count)];

                review.MovieId = movie.Id;
                review.UserId  = user.Id;
                reviewList.Add(review);
            }

            db.Reviews.AddRange(reviewList);
            await db.SaveChangesAsync();

            await redis.GetDatabase().KeyDeleteAsync($"movie:{id:N}");

            return TypedResults.Ok(new
            {
                MovieId         = id,
                ReviewsInserted = reviewList.Count
            });
        })
        .WithOpenApi()
        .WithName("GenerateReviewsForMovie");
        app.MapDelete("/db/clear", async (AppDbContext db) =>
        {
            db.WatchlistItems.RemoveRange(db.WatchlistItems);
            db.Reviews.RemoveRange(db.Reviews);
            await db.SaveChangesAsync();

            db.Movies.RemoveRange(db.Movies);
            db.Users.RemoveRange(db.Users);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithOpenApi()
        .WithName("ClearDatabase");

        app.MapDelete("cache/clear", async (IConnectionMultiplexer mux) =>
            {
                const int dbNumber = 0;

                foreach (var endpoint in mux.GetEndPoints())
                {
                    await mux.GetServer(endpoint).FlushDatabaseAsync(dbNumber);
                }

                return TypedResults.NoContent();
            })
            .WithOpenApi()
            .WithName("ClearCache");

        app.MapGet("cache", async (IConnectionMultiplexer redis) =>
        {
            var endpoint = redis.GetEndPoints().First();
            var server   = redis.GetServer(endpoint);
            var db       = redis.GetDatabase();

            // 2) Iterate all keys in the selected Redis database
            var allEntries = new Dictionary<string, string?>();
            await foreach (var key in server.KeysAsync(database: db.Database, pattern: "*"))
            {
                // 3) For each key, read back the string value (or null)
                var value = await db.StringGetAsync(key);
                allEntries[key!] = value.HasValue
                    ? value.ToString()
                    : null;
            }

            // 4) Return as JSON
            return Results.Ok(allEntries);
        })
        .WithOpenApi()
        .WithName("GetFullCache");
    }
    
}