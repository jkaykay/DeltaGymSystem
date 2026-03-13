namespace GymSystem.Shared.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? JoinDate { get; set; }
        public bool Active { get; set; }
        public DateTime? HireDate { get; set; }
        public string? EmployeeId { get; set; }
        public List<string> Roles { get; set; } = [];
    }
}

