using System.ComponentModel.DataAnnotations;

namespace GymSystem.Web.Areas.Member.ViewModels;

public class ProfileViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public string? MembershipName { get; set; }
    public double? MembershipPrice { get; set; }
    public string? QrCodeBase64 { get; set; }
}