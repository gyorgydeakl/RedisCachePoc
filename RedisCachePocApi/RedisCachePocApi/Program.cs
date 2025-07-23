using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StackExchange.Redis;

namespace RedisCachePocApi;

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
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

        app.Run();
    }
}

record CacheItem(string Key, string Value);
