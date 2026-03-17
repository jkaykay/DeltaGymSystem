namespace GymSystem.Api.Models
{
    public class Branch
    {
        public int BranchId { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string Province { get; set; }
        public required string PostCode { get; set; }
        public DateTime OpenDate { get; set; } = DateTime.UtcNow;

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
