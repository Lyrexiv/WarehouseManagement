using FluentAssertions;
using Moq;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Auth.Commands.Login;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using Xunit;

namespace WarehouseManagement.Tests.Unit.Features.Auth;

public class AuthCommandTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IPasswordService> _pwd;
    private readonly Mock<IJwtService> _jwt;

    public AuthCommandTests()
    {
        _uow      = new Mock<IUnitOfWork>();
        _userRepo = new Mock<IUserRepository>();
        _pwd      = new Mock<IPasswordService>();
        _jwt      = new Mock<IJwtService>();
        _uow.Setup(u => u.Users).Returns(_userRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "admin", Email = "admin@test.com", FullName = "Admin", IsActive = true, PasswordHash = "hash" };
        user.UserRoles.Add(new UserRole { Role = new Role { Name = "Admin" } });

        _userRepo.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.GetWithRolesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _pwd.Setup(p => p.VerifyPassword("password", "hash")).Returns(true);
        _jwt.Setup(j => j.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>())).Returns("access-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");

        var handler = new LoginCommandHandler(_uow.Object, _pwd.Object, _jwt.Object);
        var result  = await handler.Handle(new LoginCommand("admin", "password"), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Username.Should().Be("admin");
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsUnauthorizedException()
    {
        var user = new User { Username = "admin", PasswordHash = "hash", IsActive = true };
        _userRepo.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword("wrong", "hash")).Returns(false);

        var handler = new LoginCommandHandler(_uow.Object, _pwd.Object, _jwt.Object);
        var act = async () => await handler.Handle(new LoginCommand("admin", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*Invalid*");
    }

    [Fact]
    public async Task Login_InactiveUser_ThrowsUnauthorizedException()
    {
        var user = new User { Username = "admin", PasswordHash = "hash", IsActive = false };
        _userRepo.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword("pass", "hash")).Returns(true);

        var handler = new LoginCommandHandler(_uow.Object, _pwd.Object, _jwt.Object);
        var act = async () => await handler.Handle(new LoginCommand("admin", "pass"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*deactivated*");
    }

    [Fact]
    public async Task Login_UserNotFound_ThrowsUnauthorizedException()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(_uow.Object, _pwd.Object, _jwt.Object);
        var act = async () => await handler.Handle(new LoginCommand("nouser", "pass"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
