// ============================================================
// QueryableExtensions.cs — Pagination helper for database queries.
// This extension method adds a reusable ".ToPagedResultAsync()"
// to any IQueryable, so every controller can paginate results
// without duplicating the skip/take logic.
// ============================================================

using GymSystem.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Extensions;

public static class QueryableExtensions
{
    // Converts an IQueryable into a paged result with metadata.
    // "page" is the 1-based page number; "pageSize" is how many items per page.
    // Returns a PagedResult containing the items, current page, and total count.
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize)
    {
        // Ensure page is at least 1 and pageSize is between 1 and 100.
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Count all matching rows (before pagination) for the "total" metadata.
        var totalCount = await query.CountAsync();

        // Skip rows for previous pages, then take only this page's worth.
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}