namespace HC.Core.Domain;

public interface IBusinessRule
{
    string Message { get; }

    void ThrowException();

    bool IsBroken();
}
