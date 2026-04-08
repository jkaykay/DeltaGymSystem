namespace GymSystem.Shared.DTOs
{
    // Generic wrapper for paginated query results.
    // T: The type of items in the result set.
    public class PagedResult<T>
    {
        // Collection of items for the current page.
        public List<T> Items { get; set; } = [];

        // Current page number (1-based).
        public int Page { get; set; }

        // Maximum number of items per page.
        public int PageSize { get; set; }

        // Total number of items across all pages.
        public int TotalCount { get; set; }

        // Calculated total number of pages.
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Indicates whether a previous page exists.
        public bool HasPrevious => Page > 1;

        // Indicates whether a next page exists.
        public bool HasNext => Page < TotalPages;
    }
}

