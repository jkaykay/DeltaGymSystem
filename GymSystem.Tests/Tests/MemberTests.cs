using GymSystem.Api.Controllers;
using GymSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Security.Claims;


namespace GymSystem.Tests
{
    [TestClass]
    public class MemberTests
    {
        //helper method to create a logged in member
        private ClaimsPrincipal CreateMemberUser(string userId)
        {
            List<Claim> claims = new List<Claim>();

            
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Role, "Member"));

            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal user = new ClaimsPrincipal(identity);

            return user;
        }



        //Testing if a logged in member is able to view another member page
        [TestMethod]
        public async Task Get_If_Member_Tries_To_View_Another_Member_Return_Forbid()
        {
            // Arrange
            //3 nulls as it returns forbid
            MemberController controller = new MemberController(null, null, null);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = CreateMemberUser("member-1");

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContext;

            // Act
            // try to get profile of member2
            IActionResult result = await controller.Get("member-2");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task Update_When_Member_Tries_To_Update_Another_Member_Returns_Forbid()
        {
            // Arrange
            MemberController controller = new MemberController(null, null, null);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = CreateMemberUser("member-1");

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContext;

            UpdateMemberRequest request = new UpdateMemberRequest(FirstName: "Changed");
            
            // Act
            IActionResult result = await controller.Update("member-2", request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        //testing if delete action is only authorised for admin
        [TestMethod]
        public void Delete_Has_Authorize_Role_Admin()
        {
            // Arrange
            Type controllerType = typeof(MemberController);
            MethodInfo method = controllerType.GetMethod("Delete");

            // Act
            AuthorizeAttribute attribute =
                (AuthorizeAttribute)method.GetCustomAttribute(typeof(AuthorizeAttribute));

            // Assert
            Assert.IsNotNull(attribute);
            Assert.AreEqual("Admin", attribute.Roles);
        }
    }
}