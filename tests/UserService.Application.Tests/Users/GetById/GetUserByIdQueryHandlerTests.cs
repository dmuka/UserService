﻿using Application.Abstractions.Authentication;
using Application.Users.GetById;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.RoleNames;
using Moq;
using Shouldly;

namespace UserService.Application.Tests.Users.GetById;

[TestFixture]
public class GetUserByIdQueryHandlerTests
{
    private const string AdminName = "Admin";
    
    private static readonly Guid AuthorizedUserId = Guid.CreateVersion7();
    private static readonly Guid UnauthorizedUserId = Guid.CreateVersion7();
    private static readonly RoleName AdminRoleName = RoleName.Create(AdminName);
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private readonly UserId _userId = new(AuthorizedUserId);
    private IList<Role> _roles;
    
    private Mock<IUserRepository> _mockRepository;
    private Mock<IRoleRepository> _mockRoleRepository;
    private Mock<IUserContext> _mockUserContext;
    private GetUserByIdQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _roles = new List<Role> { Role.Create(Guid.CreateVersion7(), AdminName).Value };
        
        _mockRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockRoleRepository.Setup(repository => repository.GetRolesByUserIdAsync(AuthorizedUserId, _cancellationToken))
            .ReturnsAsync(_roles);
        
        _mockUserContext = new Mock<IUserContext>();
        _handler = new GetUserByIdQueryHandler(_mockRepository.Object, _mockRoleRepository.Object, _mockUserContext.Object);
    }

    [Test]
    public async Task Handle_UserMismatch_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetUserByIdQuery(AuthorizedUserId);
        _mockUserContext.Setup(x => x.UserId).Returns(UnauthorizedUserId);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("UserUnauthorized"));
        }
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var query = new GetUserByIdQuery(AuthorizedUserId);
        _mockUserContext.Setup(x => x.UserId).Returns(AuthorizedUserId);    
        _mockUserContext.Setup(x => x.UserRole).Returns("Admin");
        _mockRepository.Setup(x => x.GetUserByIdAsync(AuthorizedUserId, _cancellationToken))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("UserNotFound"));
            Assert.That(result.Error.Description, Is.EqualTo($"The user with the id = '{AuthorizedUserId}' was not found."));
        }
    }

    [Test]
    public async Task Handle_UserFound_ReturnsUserResponse()
    {
        // Arrange
        var query = new GetUserByIdQuery(AuthorizedUserId);
        _mockUserContext.Setup(x => x.UserId).Returns(AuthorizedUserId);   
        _mockUserContext.Setup(x => x.UserRole).Returns(AdminName);
        var user = User.Create(
            _userId.Value,
            "userName", 
            "John",
            "Doe", 
            "hash", 
            "email@email.com", 
            new List<RoleName> { AdminRoleName },
            new List<UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;

        var expected = new UserResponse
        {
            Id = user.Id.Value,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            IsMfaEnabled = "no",
            Roles = _roles.Select(role => (role.Name, role.Id.Value)).ToArray()
        };
        
        _mockRepository.Setup(x => x.GetUserByIdAsync(AuthorizedUserId, _cancellationToken))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            result.Value.ShouldBeEquivalentTo(expected);
        }
    }
}