using Microsoft.EntityFrameworkCore;

namespace RedisCachePocApi;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}