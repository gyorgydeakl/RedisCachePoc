using Microsoft.EntityFrameworkCore;

namespace RedisCachePocApi.Dal;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(ub =>
        {
            ub.HasMany(u => u.Reviews).WithOne(r => r.User);
            ub.HasMany(u => u.WatchList).WithMany();
        });
        modelBuilder.Entity<Movie>().HasMany(m => m.Reviews).WithOne(r => r.Movie);
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Review> Reviews => Set<Review>();
}