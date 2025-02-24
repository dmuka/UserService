using System.Security.Claims;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;

namespace UserService.Infrastructure.Tests.Authentication
{
    [TestFixture]
    public class UserContextTests
    {
        private const string AuthenticationType = "TestAuthType";
        
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private UserContext _userContext;

        [SetUp]
        public void SetUp()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _userContext = new UserContext(_httpContextAccessorMock.Object);
        }

        private void SetHttpContext(ClaimsPrincipal user)
        {
            var httpContext = new DefaultHttpContext
            {
                User = user
            };
            
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        }

        [Test]
        public void UserId_WhenUserIsAuthenticated_ReturnsUserId()
        {
            // Arrange
            var userId = Guid.CreateVersion7();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, AuthenticationType);
            SetHttpContext(new ClaimsPrincipal(identity));

            // Act
            var result = _userContext.UserId;

            // Assert
            Assert.That(result, Is.EqualTo(userId));
        }

        [Test]
        public void UserName_WhenUserIsAuthenticated_ReturnsUserName()
        {
            // Arrange
            const string userName = "testuser";
            var claims = new[] { new Claim(ClaimTypes.Name, userName) };
            var identity = new ClaimsIdentity(claims, AuthenticationType);
            SetHttpContext(new ClaimsPrincipal(identity));

            // Act
            var result = _userContext.UserName;

            // Assert
            Assert.That(result, Is.EqualTo(userName));
        }

        [Test]
        public void Email_WhenUserIsAuthenticated_ReturnsEmail()
        {
            // Arrange
            const string email = "testuser@example.com";
            var claims = new[] { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims, AuthenticationType);
            SetHttpContext(new ClaimsPrincipal(identity));

            // Act
            var result = _userContext.Email;

            // Assert
            Assert.That(result, Is.EqualTo(email));
        }

        [Test]
        public void UserRole_WhenUserIsAuthenticated_ReturnsUserRole()
        {
            // Arrange
            const string role = "Admin";
            var claims = new[] { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, AuthenticationType);
            SetHttpContext(new ClaimsPrincipal(identity));

            // Act
            var result = _userContext.UserRole;

            // Assert
            Assert.That(result, Is.EqualTo(role));
        }

        [Test]
        public void IsAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
        {
            // Arrange
            var identity = new ClaimsIdentity(Array.Empty<Claim>(), AuthenticationType);
            SetHttpContext(new ClaimsPrincipal(identity));

            // Act
            var result = _userContext.IsAuthenticated;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void UserId_WhenUserIsNotAuthenticated_ThrowsApplicationException()
        {
            // Arrange
            SetHttpContext(new ClaimsPrincipal(new ClaimsIdentity()));

            // Act & Assert
            Assert.That(() => _userContext.UserId, Throws.TypeOf<ApplicationException>().With.Message.EqualTo("User id is unavailable."));
        }
    }
}