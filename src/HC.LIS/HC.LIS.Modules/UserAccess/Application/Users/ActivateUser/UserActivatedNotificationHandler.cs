using MediatR;

namespace HC.LIS.Modules.UserAccess.Application.Users.ActivateUser;

public class UserActivatedNotificationHandler : INotificationHandler<UserActivatedNotification>
{
    public Task Handle(UserActivatedNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
