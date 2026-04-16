using GymSystem.Api.Controllers;
using GymSystem.Api.Data;
using GymSystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GymSystem.Tests
{
    [TestClass]
    public class AttendanceTests
    {
        //fake setup logics

        //helper method user DOEST NO EXIST 
        private UserManager<ApplicationUser> CreateUserManagerThatReturnsNoUser()
        {
            Mock<IUserStore<ApplicationUser>> store = new Mock<IUserStore<ApplicationUser>>();

            Mock<UserManager<ApplicationUser>> userManagerMock =
                new Mock<UserManager<ApplicationUser>>(
                    store.Object,
                    Options.Create(new IdentityOptions()),
                    new PasswordHasher<ApplicationUser>(),
                    new List<IUserValidator<ApplicationUser>>(),
                    new List<IPasswordValidator<ApplicationUser>>(),
                    null,
                    new IdentityErrorDescriber(),
                    null,
                    new Mock<ILogger<UserManager<ApplicationUser>>>().Object
                );

            userManagerMock
                .Setup(x => x.FindByIdAsync("missing-member"))
                .ReturnsAsync((ApplicationUser)null);

            return userManagerMock.Object;
        }


        //helper method user DOES EXIST
        private UserManager<ApplicationUser> CreateUserManagerReturningUser(ApplicationUser user)
        {
            Mock<IUserStore<ApplicationUser>> store = new Mock<IUserStore<ApplicationUser>>();

            Mock<UserManager<ApplicationUser>> userManagerMock =
                new Mock<UserManager<ApplicationUser>>(
                    store.Object,
                    Options.Create(new IdentityOptions()),
                    new PasswordHasher<ApplicationUser>(),
                    new List<IUserValidator<ApplicationUser>>(),
                    new List<IPasswordValidator<ApplicationUser>>(),
                    null,
                    new IdentityErrorDescriber(),
                    null,
                    new Mock<ILogger<UserManager<ApplicationUser>>>().Object
                );

            userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            return userManagerMock.Object;
        }


        //testing when admin tries to check in a member that does not exist in the database
        [TestMethod]
        public async Task CheckIn_When_Member_Not_Exist_Return_NotFound()
        {
            // Arrange
            DbContextOptions<GymDbContext> options =
                new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase("AttendanceTestDb1")
                .Options;

            GymDbContext context = new GymDbContext(options);

            UserManager<ApplicationUser> userManager = CreateUserManagerThatReturnsNoUser();

            AttendanceController controller = new AttendanceController(context, userManager, null);

            // Act
            IActionResult result = await controller.CheckIn("missing-member");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }


        //check in a member who is not active on any memberships 
        [TestMethod]
        public async Task CheckIn_When_Member_Is_Not_Active_Returns_Forbid()
        {
            // Arrange
            DbContextOptions<GymDbContext> options =
                new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase("AttendanceTestDb3")
                .Options;

            GymDbContext context = new GymDbContext(options);

            ApplicationUser inactiveMember = new ApplicationUser
            {
                Id = "member-1",
                UserName = "member1",
                Email = "member@example.com",
                FirstName = "TestName",
                LastName = "TestLastName",
                Active = false
            };

            UserManager<ApplicationUser> userManager =
                CreateUserManagerReturningUser(inactiveMember);

            AttendanceController controller =
                new AttendanceController(context, userManager, null);

            // Act
            IActionResult result = await controller.CheckIn("member-1");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }


    }


}