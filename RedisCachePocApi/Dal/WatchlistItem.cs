namespace RedisCachePocApi.Dal;

public class WatchlistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public required Guid MovieId { get; set; }
}