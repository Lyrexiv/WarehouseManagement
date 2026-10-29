using FluentAssertions;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Infrastructure.Persistence;
using WarehouseManagement.Infrastructure.Persistence.Repositories;
using Xunit;

namespace WarehouseManagement.Tests.Integration;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _ctx;
    private readonly UserRepository _repo;

    public UserRepositoryTests()
    {
        _ctx = InMemoryDbContextFactory.Create("UserTest");
        _repo = new UserRepository(_ctx);

        var role = new Role { Name = "Admin" };
        var user = new User
        {
            Username = "testuser", Email = "test@test.com", FullName = "Test User",
            PasswordHash = "hash", IsActive = true
        };
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });

        _ctx.Roles.Add(role);
        _ctx.Users.Add(user);
        _ctx.SaveChanges();
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUser_ReturnsUser()
    {
        var result = await _repo.GetByUsernameAsync("testuser");
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        var result = await _repo.GetByEmailAsync("test@test.com");
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExistingUser_ReturnsNull()
    {
        var result = await _repo.GetByUsernameAsync("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllWithRolesAsync_ReturnsUsersWithRoles()
    {
        var result = await _repo.GetAllWithRolesAsync();
        result.Should().HaveCount(1);
        result[0].UserRoles.Should().HaveCount(1);
        result[0].UserRoles.First().Role.Name.Should().Be("Admin");
    }

    public void Dispose() => _ctx.Dispose();
}
