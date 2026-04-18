namespace HC.LIS.Modules.UserAccess.Application.Users;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string toEmail, string invitationToken);
}
