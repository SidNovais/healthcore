using FluentValidation;
using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

internal class ValidationCommandHandlerDecorator<T>(
    IList<IValidator<T>> validators,
    ICommandHandler<T> decorated
) : ICommandHandler<T>
    where T : ICommand
{
    private readonly IList<IValidator<T>> _validators = validators;
    private readonly ICommandHandler<T> _decorated = decorated;

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        var errors = _validators
            .Select(v => v.Validate(command))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (errors.Count > 0)
            throw new InvalidCommandException(string.Join(";", errors.Select(x => x.ErrorMessage).ToList()));

        await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);
    }
}
