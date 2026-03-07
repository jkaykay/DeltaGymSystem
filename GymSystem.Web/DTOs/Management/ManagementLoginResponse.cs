namespace GymSystem.Web.DTOs.Management
{
    // Matches the API's LoginResponse shape
    public class ManagementLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
    }
}
