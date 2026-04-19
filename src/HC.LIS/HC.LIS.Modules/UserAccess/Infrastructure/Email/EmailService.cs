using HC.LIS.Modules.UserAccess.Application.Users;
using Serilog;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Email;

internal class EmailService(ILogger logger) : IEmailService
{
    private readonly ILogger _logger = logger.ForContext<EmailService>();

    public Task SendInvitationEmailAsync(string toEmail, string invitationToken)
    {
        _logger.Information(
            "Invitation email (stub): To={Email} Token={Token}", toEmail, invitationToken);
        return Task.CompletedTask;
    }
}
