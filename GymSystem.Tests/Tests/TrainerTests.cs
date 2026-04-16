using GymSystem.Api.Controllers;
using GymSystem.Api.Models;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;

namespace GymSystem.Tests
{
    [TestClass]
    public class TrainerControllerTests
    {
        private ClaimsPrincipal CreateTrainerUser(string userId)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Role, "Trainer"));

            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal user = new ClaimsPrincipal(identity);

            return user;
        }

        //moq 
        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
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

            return userManagerMock;
        }



        [TestMethod]
        public async Task UpdateSelf_When_Trainer_Updates_Own_Profile_Returns_NoContent()
        {
            // Arrange
            ApplicationUser trainer = new ApplicationUser
            {
                Id = "trainer-1",
                UserName = "trainer1",
                Email = "old@example.com",
                FirstName = "Old",
                LastName = "Name",
                Active = true
            };

            Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();

            userManagerMock
                .Setup(x => x.FindByIdAsync("trainer-1"))
                .ReturnsAsync(trainer);

            userManagerMock
                .Setup(x => x.FindByEmailAsync("new@example.com"))
                .ReturnsAsync((ApplicationUser)null);

            userManagerMock
                .Setup(x => x.UpdateAsync(trainer))
                .ReturnsAsync(IdentityResult.Success);

            Mock<IOutputCacheStore> outputCacheMock = new Mock<IOutputCacheStore>();

            TrainerController controller =
                new TrainerController(userManagerMock.Object, null, outputCacheMock.Object);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = CreateTrainerUser("trainer-1");

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContext;

            UpdateTrainerProfileRequest request =
                new UpdateTrainerProfileRequest(Email: "new@example.com", FirstName: "New", LastName: "Trainer");

            // Act
            IActionResult result = await controller.UpdateSelf(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            Assert.AreEqual("new@example.com", trainer.Email);
            Assert.AreEqual("New", trainer.FirstName);
            Assert.AreEqual("Trainer", trainer.LastName);
        }


        
        [TestMethod]
        public async Task Get_When_Trainer_Tries_To_View_Another_Trainer_Returns_Forbid()
        {
            // Arrange
            TrainerController controller = new TrainerController(null, null, null);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = CreateTrainerUser("trainer-1");

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContext;

            // Act
            IActionResult result = await controller.Get("trainer-2");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }
    }
}