namespace GymSystem.Shared.DTOs;

public class ProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MembershipName { get; set; } = string.Empty;
    public double MembershipPrice { get; set; }
    public string? QrCodeBase64 { get; set; }
}