using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCachePocApi.Dal;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Bogus;
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
        app.MapMovieReviewerEndpoints();
        app.Run();
    }
    
    
}

record CacheItem(string Key, string Value);