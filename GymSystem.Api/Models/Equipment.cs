namespace GymSystem.Api.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public string? Description { get; set; }
        public required DateTime InDate { get; set; }
        public bool Status { get; set; }

        public required int RoomId { get; set; }
        public required Room Room { get; set; }
    }
}
