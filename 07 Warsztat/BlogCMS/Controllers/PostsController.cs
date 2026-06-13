using BlogCMS.Models;
using BlogCMS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BlogCMS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class PostsController(IPostRepository postRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<PostResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PostResponse>>> GetAllPosts(
        [FromQuery] PostQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await postRepository.GetPagedAsync(
            parameters,
            cancellationToken);

        return Ok(new PagedResult<PostResponse>(
            result.Items.Select(ToResponse).ToArray(),
            result.Page,
            result.PageSize,
            result.TotalCount));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PostResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostResponse>> GetPostById(
        int id,
        CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(id, cancellationToken);
        return post is null ? NotFound() : Ok(ToResponse(post));
    }

    [HttpPost]
    [ProducesResponseType<PostResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PostResponse>> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var post = new Post
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            Published = request.Published ?? DateTime.UtcNow
        };

        await postRepository.AddAsync(post, cancellationToken);
        var response = ToResponse(post);

        return CreatedAtAction(
            nameof(GetPostById),
            new { id = post.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePost(
        int id,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(id, cancellationToken);
        if (post is null)
        {
            return NotFound();
        }

        post.Title = request.Title.Trim();
        post.Content = request.Content.Trim();
        post.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        post.Published = request.Published!.Value;

        await postRepository.UpdateAsync(post, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await postRepository.DeleteAsync(
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    private static PostResponse ToResponse(Post post) =>
        new(
            post.Id,
            post.Title,
            post.Content,
            post.ImageUrl,
            post.Published);
}
