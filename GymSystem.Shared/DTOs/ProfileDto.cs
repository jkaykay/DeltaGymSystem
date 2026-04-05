using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public class ProfileDto
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Telephone { get; set; }

    public string? EmergencyContact { get; set; }

    public double? Weight { get; set; }

    public string MembershipName { get; set; } = string.Empty;

    public double MembershipPrice { get; set; }

    public string MemberCode { get; set; } = string.Empty;
}