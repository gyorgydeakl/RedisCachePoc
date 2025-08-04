using RedisCachePocApi.Dal;

namespace MoviePlanner;

public record CreateWatchlistItemDto
{
    public required Guid UserId { get; init; }
    public required Guid MovieId { get; init; }
}
public record WatchlistItemDto
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
public record MovieDetailsDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Genre { get; init; }
    public required string Director { get; init; }
    public required string Plot { get; init; }
    public required int ReviewCount { get; init; }
}


public static class DtoExtensions
{
    public static WatchlistItemDto ToDto(this WatchlistItem item)
    {
        return new WatchlistItemDto
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

    public static MovieDetailsDto ToDetailsDto(this Movie movie, int reviewCount)
    {
        return new MovieDetailsDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Genre = movie.Genre,
            Director = movie.Director,
            Plot = movie.Plot,
            ReviewCount = reviewCount
        };
    }

    public static WatchlistItem ToWatchlistItem(this CreateWatchlistItemDto dto)
    {
        return new WatchlistItem()
        {
            UserId = dto.UserId,
            MovieId = dto.MovieId
        };
    }
}