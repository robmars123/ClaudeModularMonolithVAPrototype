using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Shared.Common;

public static class QueryablePagingExtensions
{
    public const int MaxPageSize = 200;

    /// <summary>
    /// Counts, pages, and projects a filtered+ordered query in one call — the count/skip/take/wrap
    /// boilerplate every "GetX" list handler otherwise repeats. Filtering, ordering, and the projection
    /// shape stay handler-specific; only the paging mechanics are shared.
    /// Page/pageSize are clamped here so an out-of-range value (0, negative, or oversized) from a client
    /// can never reach Skip/Take as a negative or unbounded value.
    /// </summary>
    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> orderedQuery,
        int page,
        int pageSize,
        Expression<Func<TSource, TResult>> selector,
        CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        int total = await orderedQuery.CountAsync(ct);
        List<TResult> items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(ct);

        return new PagedResult<TResult>(items, total, page, pageSize);
    }
}
