using BlogCMS.Models;

namespace BlogCMS.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<PagedResult<Post>> GetPagedAsync(
        PostQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
