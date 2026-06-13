using System.ComponentModel.DataAnnotations;

namespace BlogCMS.Models;

public class PostQueryParameters : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [StringLength(200)]
    public string? Keyword { get; init; }

    public DateTime? PublishedFrom { get; init; }

    public DateTime? PublishedTo { get; init; }

    public PostSortBy SortBy { get; init; } = PostSortBy.Published;

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PublishedFrom > PublishedTo)
        {
            yield return new ValidationResult(
                "PublishedFrom cannot be later than PublishedTo.",
                [nameof(PublishedFrom), nameof(PublishedTo)]);
        }
    }
}

public enum PostSortBy
{
    Published,
    Title
}

public enum SortDirection
{
    Asc,
    Desc
}
