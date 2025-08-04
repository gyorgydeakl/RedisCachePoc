using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCachePocApi.Dal;
using Scalar.AspNetCore;

namespace MoviePlanner;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var useAutoCache = builder.Services.AddCache(builder.Configuration);

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

        app.UseOutputCache();

        // --- Minimal‑API endpoints -----------------------------------------
        if (useAutoCache)
        {
            app.MapAppEndpointsWithAutoCache();
        }
        else
        {
            app.MapAppEndpoints();
        }
        app.MapPost("/watchlist", async ([FromBody] CreateWatchlistItemDto input, AppDbContext db) =>
            {
                var userExists = await db.Users.AnyAsync(u => u.Id == input.UserId);
                if (!userExists)
                {
                    return Results.NotFound($"User {input.UserId} not found.");
                }

                var movieExists = await db.Movies.AnyAsync(m => m.Id == input.MovieId);
                if (!movieExists)
                {
                    return Results.NotFound($"movie {input.MovieId} not found.");
                }

                var watchlistItemExists = await db.WatchlistItems.AnyAsync(w => w.UserId == input.UserId && w.MovieId == input.MovieId);
                if (watchlistItemExists)
                {
                    return Results.Conflict($"This user already has that movie on the watchlist");
                }

                var item = input.ToWatchlistItem();
                db.WatchlistItems.Add(item);
                await db.SaveChangesAsync();

                return TypedResults.Ok(item);
            })
            .WithOpenApi()
            .WithName("AddWatchListItem");
        app.Run();
    }
}