namespace GymSystem.Web.DTOs.Member
{
    public class MemberLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public DateTime JoinDate { get; set; }
        public bool Active { get; set; }
    }
}
