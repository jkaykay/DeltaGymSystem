using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Data transfer object representing a user (member, staff, or trainer).
public class UserDTO
{
    // Unique identity identifier for the user.
    public string Id { get; set; } = string.Empty;

    // User email address (max 256 characters).
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    // Display username (max 256 characters).
    [MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    // User first name (required, max 100 characters).
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    // User last name (required, max 100 characters).
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    // Date the member joined the gym.
    public DateTime? JoinDate { get; set; }

    // Whether the user account is currently active.
    public bool Active { get; set; }

    // Date the staff member or trainer was hired.
    public DateTime? HireDate { get; set; }

    // Optional employee identifier for staff and trainers (max 50 characters).
    [MaxLength(50)]
    public string? EmployeeId { get; set; }

    // Optional phone number (validated format, max 20 characters).
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    // Foreign key to the branch the user is assigned to.
    public int? BranchId { get; set; }

    // List of role names assigned to the user (e.g. Member, Trainer, Staff, Admin).
    public List<string> Roles { get; set; } = [];
}
