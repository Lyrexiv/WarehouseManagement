using WarehouseManagement.Application.Common.Interfaces;
namespace WarehouseManagement.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, 12);
    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
