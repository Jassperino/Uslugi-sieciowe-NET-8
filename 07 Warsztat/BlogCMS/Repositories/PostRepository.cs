using BlogCMS.Data;
using BlogCMS.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogCMS.Repositories;

public class PostRepository : EfCoreRepository<Post>, IPostRepository
{
    private readonly BlogDbContext _context;

    public PostRepository(BlogDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Post>> GetPagedAsync(
        PostQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Posts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim();
            query = query.Where(post =>
                post.Title.Contains(keyword) ||
                post.Content.Contains(keyword));
        }

        if (parameters.PublishedFrom.HasValue)
        {
            query = query.Where(post =>
                post.Published >= parameters.PublishedFrom.Value);
        }

        if (parameters.PublishedTo.HasValue)
        {
            query = query.Where(post =>
                post.Published <= parameters.PublishedTo.Value);
        }

        query = (parameters.SortBy, parameters.SortDirection) switch
        {
            (PostSortBy.Title, SortDirection.Asc) =>
                query.OrderBy(post => post.Title).ThenBy(post => post.Id),
            (PostSortBy.Title, SortDirection.Desc) =>
                query.OrderByDescending(post => post.Title).ThenBy(post => post.Id),
            (PostSortBy.Published, SortDirection.Asc) =>
                query.OrderBy(post => post.Published).ThenBy(post => post.Id),
            _ => query.OrderByDescending(post => post.Published)
                .ThenByDescending(post => post.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Post>(
            items,
            parameters.Page,
            parameters.PageSize,
            totalCount);
    }
}
