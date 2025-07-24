namespace RedisCachePocApi.Dal;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required int Rating { get; set; }
    public required Guid MovieId { get; set; }
    public required Guid UserId { get; set; }
    public required DateTime Date { get; set; }
    
    public Movie Movie { get; set; } = null!;
    public User User { get; set; } = null!;
}