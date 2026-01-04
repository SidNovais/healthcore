namespace HC.LIS.Modules.TestOrders.Application.Contracts;

public abstract class CommandBase : ICommand
{
    public Guid Id { get; }

    protected CommandBase()
    {
        Id = Guid.CreateVersion7();
    }

    protected CommandBase(Guid id)
    {
        Id = id;
    }
}

public abstract class CommandBase<TResult> : ICommand<TResult>
{
    protected CommandBase()
    {
        Id = Guid.CreateVersion7();
    }

    protected CommandBase(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
