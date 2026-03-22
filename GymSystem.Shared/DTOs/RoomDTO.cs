namespace GymSystem.Shared.DTOs;

public class RoomDTO
{
    public int RoomId { get; set; }
    public int RoomNumber { get; set; }
    public int BranchId { get; set; }
    public int MaxCapacity { get; set; }
    public int SessionCount { get; set; }
    public int EquipmentCount { get; set; }
}