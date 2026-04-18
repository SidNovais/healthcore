using MediatR;

namespace HC.LIS.Modules.UserAccess.Application.Users.CreateUser;

public class UserCreatedNotificationHandler(IEmailService emailService)
    : INotificationHandler<UserCreatedNotification>
{
    private readonly IEmailService _emailService = emailService;

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        await _emailService.SendInvitationEmailAsync(
            notification.DomainEvent.Email,
            notification.DomainEvent.InvitationToken
        ).ConfigureAwait(false);
    }
}
