namespace RedisCachePocApi.Dal;

public record Movie
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string Plot { get; set; }

    public List<Review> Reviews { get; set; } = [];
}