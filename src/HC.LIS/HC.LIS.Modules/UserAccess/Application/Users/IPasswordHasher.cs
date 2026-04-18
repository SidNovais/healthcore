namespace HC.LIS.Modules.UserAccess.Application.Users;

public interface IPasswordHasher
{
    string HashPassword(string plainText);
    bool VerifyHashedPassword(string hash, string plainText);
}
