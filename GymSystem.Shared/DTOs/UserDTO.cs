using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public class UserDTO
{
    public string Id { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime? JoinDate { get; set; }
    public bool Active { get; set; }
    public DateTime? HireDate { get; set; }

    [MaxLength(50)]
    public string? EmployeeId { get; set; }

    public int? BranchId { get; set; }
    public List<string> Roles { get; set; } = [];
}