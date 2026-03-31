using System.ComponentModel.DataAnnotations;

namespace GymSystem.Api.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        [Range(1, 100)]
        public required int RoomNumber { get; set; }

        [Range(1, 100)]
        public required int MaxCapacity { get; set; }
        public required int BranchId { get; set; }
        public required Branch Branch { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
