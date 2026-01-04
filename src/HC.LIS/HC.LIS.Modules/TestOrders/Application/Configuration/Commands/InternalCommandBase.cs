using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

public abstract class InternalCommandBase(Guid id) : ICommand
{
    public Guid Id { get; } = id;
}

public abstract class InternalCommandBase<TResult> : ICommand<TResult>
{
    protected InternalCommandBase()
    {
        Id = Guid.CreateVersion7();
    }

    protected InternalCommandBase(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
