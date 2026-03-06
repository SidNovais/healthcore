using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

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
