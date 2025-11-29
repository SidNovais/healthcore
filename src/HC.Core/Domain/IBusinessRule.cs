namespace HC.Core.Domain;

public interface IBusinessRule
{
    void ThrowException();

    bool IsBroken();
}
