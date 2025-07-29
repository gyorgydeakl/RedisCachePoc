using RedisCachePocApi.Dal;

namespace RedisCachePocApi.MovieReviewer;

public record MovieSummaryDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string PlotShortened { get; set; }
}

public record ReviewDto
{
    public required Guid Id { get; set; }
    public required int Rating { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime Date { get; set; }
    public required string UserName { get; set; }
}

public record MovieDetailsDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string Plot { get; set; }
    public List<ReviewDto> Reviews { get; set; } = [];
}

public record CreateMovieDto
{
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string Plot { get; set; }
}

public record CreateReviewDto
{
    public required int Rating { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Guid UserId { get; set; }
}

public record UserDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Bio { get; set; }
    public required string Email { get; set; }
}

public record CreateUserDto
{
    public required string Username { get; set; }
    public required string Bio { get; set; }
    public required string Email { get; set; }
}

public record SeedDto
{
    public required int MovieCount { get; init; }
    public required int ReviewCount { get; init; }
    public required int UserCount { get; init; }
}
public static class DtoExtensions
{
    public static Movie ToMovie(this CreateMovieDto dto)
    {
        return new Movie()
        {
            Title = dto.Title,
            Genre = dto.Genre,
            Director = dto.Director,
            Plot = dto.Plot
        };
    }

    public static Review ToReview(this CreateReviewDto dto, Guid movieId, DateTime date)
    {
        return new Review()
        {
            Rating = dto.Rating,
            Title = dto.Title,
            Description = dto.Description,
            MovieId = movieId,
            UserId = dto.UserId,
            Date = date
        };
    }
    public static MovieSummaryDto ToSummaryDto(this Movie movie)
    {
        return new MovieSummaryDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Genre = movie.Genre,
            Director = movie.Director,
            PlotShortened = movie.Plot[..Math.Min(movie.Plot.Length, 500)]
        };
    }

    public static MovieDetailsDto ToDetailsDto(this Movie movie)
    {
        return new MovieDetailsDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Genre = movie.Genre,
            Director = movie.Director,
            Plot = movie.Plot,
            Reviews = movie.Reviews.Select(r => r.ToDto()).ToList()
        };
    }

    public static ReviewDto ToDto(this Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            Title = review.Title,
            Description = review.Description,
            Date = review.Date,
            UserName = review.User.Username
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
    public static User ToUser(this CreateUserDto dto)
    {
        return new User()
        {
            Username = dto.Username,
            Bio = dto.Bio,
            Email = dto.Email
        };
    }
}