using HC.LIS.Modules.UserAccess.Domain.Users;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Users;

internal class UserRepository(UserAccessContext context) : IUserRepository
{
    private readonly UserAccessContext _context = context;

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
    }

    public async Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync([userId], cancellationToken).ConfigureAwait(false);
    }
}
