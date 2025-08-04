using StackExchange.Redis;

namespace MoviePlanner;

public static class CacheExtensions
{
    public static bool AddCache(this IServiceCollection services, IConfiguration config)
    {
        var useAutoCache = config.GetValue<bool>("UseAutoCache");
        if (useAutoCache)
        {
            services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = config.GetConnectionString("RedisCache");
            });
            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(30)));
            });
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(config.GetConnectionString("RedisCache")!);
            options.AllowAdmin = true;
            return ConnectionMultiplexer.Connect(options);
        });

        return useAutoCache;
    }
}