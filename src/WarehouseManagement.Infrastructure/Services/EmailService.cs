using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces;
namespace WarehouseManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    public EmailService(ILogger<EmailService> logger) => _logger = logger;

    public Task SendPasswordResetEmailAsync(string email, string token, CancellationToken ct = default)
    {
        _logger.LogInformation("Password reset email sent to {Email} with token {Token}", email, token);
        return Task.CompletedTask;
    }
    public Task SendWelcomeEmailAsync(string email, string username, CancellationToken ct = default)
    {
        _logger.LogInformation("Welcome email sent to {Email} for user {Username}", email, username);
        return Task.CompletedTask;
    }
}
