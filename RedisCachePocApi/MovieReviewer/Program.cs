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
        var multiplexer = ConnectionMultiplexer
            .Connect(builder.Configuration.GetConnectionString("RedisCache")!);
        builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
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
        
        app.MapGet("/movies", async (AppDbContext db) =>
        {
            var result = await db.Movies.Select(m => m.ToSummaryDto()).ToListAsync();
            return Results.Ok(result);
        });

        app.MapGet("/movies/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var result = await db.Movies.Where(m => m.Id == id).Select(m => m.ToDetailsDto()).SingleOrDefaultAsync();
            return Results.Ok(result);
        });

        app.MapGet("/user/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var result = await db.Users.Where(u => u.Id == id).Select(u => u.ToDto()).SingleOrDefaultAsync();
            return Results.Ok(result);
        });
        
        app.Run();
    }
}

record CacheItem(string Key, string Value);