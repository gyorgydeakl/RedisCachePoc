using RedisCachePocApi.Dal;

namespace RedisCachePocApi.MovieReviewer;

public record MovieSummaryDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string Plot { get; set; }
}

public record ReviewDto
{
    public required Guid Id { get; set; }
    public required int Rating { get; set; }
    public required string Text { get; set; }
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

public record UserDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Bio { get; set; }
}

public static class DtoExtensions
{
    public static MovieSummaryDto ToSummaryDto(this Movie movie)
    {
        return new MovieSummaryDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Genre = movie.Genre,
            Director = movie.Director,
            Plot = movie.Plot
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
            Text = review.Text,
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
            Bio = user.Bio
        };
    }
}