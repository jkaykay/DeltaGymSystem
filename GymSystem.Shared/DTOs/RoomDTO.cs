using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public class RoomDTO
{
    public int RoomId { get; set; }

    [Range(1, 100)]
    public int RoomNumber { get; set; }

    public int BranchId { get; set; }

    [Range(1, 100)]
    public int MaxCapacity { get; set; }

    public int SessionCount { get; set; }
    public int EquipmentCount { get; set; }
}
