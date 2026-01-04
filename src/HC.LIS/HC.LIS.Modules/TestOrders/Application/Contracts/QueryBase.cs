namespace HC.LIS.Modules.TestOrders.Application.Contracts;

public abstract class QueryBase<TResult> : IQuery<TResult>
{
    public Guid Id { get; }
    protected QueryBase()
    {
        Id = Guid.CreateVersion7();
    }
    protected QueryBase(Guid id)
    {
        Id = id;
    }
}
