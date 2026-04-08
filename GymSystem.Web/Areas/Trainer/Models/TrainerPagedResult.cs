namespace GymSystem.Web.Areas.Trainer.Models
{
    /// <summary>
    /// A generic model that represents a single "page" of results returned by the API.
    /// Used in the Trainer area to deserialise paged API responses (sessions, rooms, classes).
    /// T is the type of item in the list (e.g. SessionDTO, RoomDTO).
    /// </summary>
    public class TrainerPagedResult<T>
    {
        /// <summary>The items on the current page.</summary>
        public List<T> Items { get; set; } = new();
        /// <summary>The current page number (1-based).</summary>
        public int Page { get; set; }
        /// <summary>How many items were requested per page.</summary>
        public int PageSize { get; set; }
        /// <summary>The total number of matching items across all pages.</summary>
        public int TotalCount { get; set; }
        /// <summary>The total number of pages available.</summary>
        public int TotalPages { get; set; }
    }
}
