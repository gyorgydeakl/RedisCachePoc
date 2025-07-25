using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCachePocApi.Dal;
using Scalar.AspNetCore;
using StackExchange.Redis;

namespace RedisCachePocApi.MovieReviewer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Services ------------------------------------------------------
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("RedisCache")!);
            options.AllowAdmin = true;
            return ConnectionMultiplexer.Connect(options);
        });
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularDev",
                policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
        
        var app = builder.Build();

        // --- Pipeline ------------------------------------------------------
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAngularDev");
        app.UseAuthorization();
        // --- Minimal‑API endpoints -----------------------------------------
        app.MapPost("/cache",
            async (CacheItem item, IConnectionMultiplexer mux) =>
            {
                await mux.GetDatabase().StringSetAsync(item.Key, item.Value);
                return Results.Ok(new { item.Key, Stored = true });
            })
            .WithOpenApi();

        app.MapGet("/cache/{key}",
            async (string key, IConnectionMultiplexer mux) =>
            {
                var val = await mux.GetDatabase().StringGetAsync(key);
                return val.HasValue
                    ? Results.Ok(new { key, value = val.ToString() })
                    : Results.NotFound($"Key '{key}' not found.");
            })
            .WithOpenApi();
        
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
            
        app.MapGet("/users", async (AppDbContext db) =>
        {
            var result = await db.Users.Select(u => u.ToDto()).ToListAsync();
            return TypedResults.Ok(result);
        })
        .WithOpenApi();

        app.MapPost("/users", async ([FromBody] CreateUserDto command, AppDbContext db) =>
        {
            var user = db.Users.Add(command.ToUser()).Entity;
            await db.SaveChangesAsync();
            return TypedResults.Ok(user.ToDto());
        })
        .WithOpenApi();

        app.MapDelete("cache/clear", async (IConnectionMultiplexer mux) =>
        {
            const int dbNumber = 0;                         

            foreach (var endpoint in mux.GetEndPoints())
            {
                await mux.GetServer(endpoint).FlushDatabaseAsync(dbNumber);
            }

            return TypedResults.NoContent(); 
        })
        .WithOpenApi();
    
        app.Run();
    }
}

record CacheItem(string Key, string Value);