namespace RedisCachePocApi.Dal;

public record User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Bio { get; set; }
    public List<Review> Reviews { get; set; } = [];
    public List<Movie> WatchList { get; set; } = [];
}