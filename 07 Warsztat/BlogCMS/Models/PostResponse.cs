namespace BlogCMS.Models;

public record PostResponse(
    int Id,
    string Title,
    string Content,
    string ImageUrl,
    DateTime Published);
