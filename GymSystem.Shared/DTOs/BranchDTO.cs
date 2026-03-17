namespace GymSystem.Shared.DTOs
{
    public class BranchDTO
    {
        public int BranchId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
        public DateTime OpenDate { get; set; }
    }
}
