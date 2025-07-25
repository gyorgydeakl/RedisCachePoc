using RedisCachePocApi.Dal;

namespace MoviePlanner;

public record WatchListItemDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; } = null!;
    public required Guid MovieId { get; init; }
}

public record UserDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Bio { get; set; }
    public required string Email { get; set; }
}

public static class DtoExtensions
{
    public static WatchListItemDto ToDto(this WatchlistItem item)
    {
        return new WatchListItemDto
        {
            Id = item.Id,
            Title = item.Movie.Title,
            MovieId = item.MovieId
        };
    }
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Bio = user.Bio,
            Email = user.Email
        };
    }
}