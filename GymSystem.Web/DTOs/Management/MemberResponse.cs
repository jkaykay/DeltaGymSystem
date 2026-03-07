namespace GymSystem.Web.DTOs.Management;

public class MemberResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public bool Active { get; set; }
}
