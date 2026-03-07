using GymSystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.Api.Data
{
    public class GymDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options)
        {
        }
    }
}
