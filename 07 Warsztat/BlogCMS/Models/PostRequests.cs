using System.ComponentModel.DataAnnotations;

namespace BlogCMS.Models;

public class CreatePostRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(20000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;

    [Url]
    [StringLength(2048)]
    public string? ImageUrl { get; init; }

    public DateTime? Published { get; init; }
}

public class UpdatePostRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(20000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;

    [Url]
    [StringLength(2048)]
    public string? ImageUrl { get; init; }

    [Required]
    public DateTime? Published { get; init; }
}
