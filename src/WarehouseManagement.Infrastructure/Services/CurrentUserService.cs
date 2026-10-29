
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WarehouseManagement.Application.Common.Interfaces;

namespace WarehouseManagement.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (Guid.TryParse(value, out var id))
            {
                return id;
            }

            return null;
        }
    }

    public string? Username =>
        User?.FindFirstValue(
            ClaimTypes.Name);

    public string? Email =>
        User?.FindFirstValue(
            ClaimTypes.Email);

    public IEnumerable<string> Roles =>
        User?.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
        ?? Enumerable.Empty<string>();

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) == true;
    }

    public bool IsAdmin =>
        IsInRole("Admin");

    public bool IsManager =>
        IsInRole("Manager");

    public bool IsStaff =>
        IsInRole("Staff");
}

