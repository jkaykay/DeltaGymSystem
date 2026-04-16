using GymSystem.Api.Controllers;
using GymSystem.Api.Data;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;


namespace GymSystem.Tests
{
    [TestClass]
    public class BookingTests
    {
        //helper method to create a member
        private ClaimsPrincipal CreateMemberUser(string userId)
        {
            List<Claim> claims = new List<Claim>();


            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Role, "Member"));

            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal user = new ClaimsPrincipal(identity);

            return user;
        }


        //helper method fake setup logic
        private UserManager<ApplicationUser> CreateUserManagerMock(ApplicationUser appUser)
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
                .Setup(x => x.FindByIdAsync(appUser.Id))
                .ReturnsAsync(appUser);

            return userManagerMock.Object;
        }


        //testing when a user books a session does not exist
        [TestMethod]
        public async Task When_Session_Does_Not_Exist_return_BadRequest() {

            //Arrange

            DbContextOptions<GymDbContext> options =
                new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase(databaseName: "BookingTestDb1")
                .Options;

            GymDbContext context = new GymDbContext(options);

            ApplicationUser member = new ApplicationUser
            {
                Id = "member-1",
                UserName = "member1",
                Email = "member@example.com",
                FirstName = "TestMemberName",
                LastName = "TestMemberLastName"

            };

            UserManager<ApplicationUser> userManager =
                CreateUserManagerMock(member);

            Mock<IOutputCacheStore> outputCacheMock = new Mock<IOutputCacheStore>();

            BookingController controller =
                new BookingController(context, userManager, outputCacheMock.Object);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = CreateMemberUser("member-1");

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContext;

            AddMyBookingRequest request = new AddMyBookingRequest(100);

            // Act
            IActionResult result = await controller.CreateMy(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

        
        }



        //testing if a member tries cancelling a session that doesnt exist
        [TestMethod]
        public async Task Delete_When_Booking_Does_Not_Exist_Returns_NotFound()
        {
            // Arrange
            DbContextOptions<GymDbContext> options =
                new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase("DeleteMissingBookingTest")
                .Options;

            GymDbContext context = new GymDbContext(options);

            BookingController controller = new BookingController(context, null, null);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = CreateMemberUser("member-1");

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContext;

            // Act
            IActionResult result = await controller.DeleteMy(100);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }



    }
}