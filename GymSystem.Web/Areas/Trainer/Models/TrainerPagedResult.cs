namespace GymSystem.Web.Areas.Trainer.Models
{
    // A generic model that represents a single "page" of results returned by the API.
    // Used in the Trainer area to deserialise paged API responses (sessions, rooms, classes).
    // T is the type of item in the list (e.g. SessionDTO, RoomDTO).
    public class TrainerPagedResult<T>
    {
        // The items on the current page.
        public List<T> Items { get; set; } = new();
        // The current page number (1-based).
        public int Page { get; set; }
        // How many items were requested per page.
        public int PageSize { get; set; }
        // The total number of matching items across all pages.
        public int TotalCount { get; set; }
        // The total number of pages available.
        public int TotalPages { get; set; }
    }
}
