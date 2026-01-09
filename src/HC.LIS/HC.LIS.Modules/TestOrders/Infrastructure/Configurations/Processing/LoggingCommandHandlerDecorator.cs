using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

internal class LoggingCommandHandlerDecorator<T>(
    ILogger logger,
    IExecutionContextAccessor executionContextAccessor,
    ICommandHandler<T> decorated
) : ICommandHandler<T>
    where T : ICommand
{
    private readonly ILogger _logger = logger;
    private readonly IExecutionContextAccessor _executionContextAccessor = executionContextAccessor;
    private readonly ICommandHandler<T> _decorated = decorated;

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        if (command is IRecurringCommand)
        {
            await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);
        }

        using (
            LogContext.Push(
                new RequestLogEnricher(_executionContextAccessor),
                new CommandLogEnricher(command)))
        {
            try
            {
                _logger.Information(
                    "Executing command {Command}",
                    command.GetType().Name);

                await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

                _logger.Information("Command {Command} processed successful", command.GetType().Name);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Command {Command} processing failed", command.GetType().Name);
                throw;
            }
        }
    }

    private class CommandLogEnricher(ICommand command) : ILogEventEnricher
    {
        private readonly ICommand _command = command;
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty("Context", new ScalarValue($"Command:{_command.Id}")));
        }
    }

    private class RequestLogEnricher(IExecutionContextAccessor executionContextAccessor) : ILogEventEnricher
    {
        private readonly IExecutionContextAccessor _executionContextAccessor = executionContextAccessor;
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_executionContextAccessor.IsAvailable)
                logEvent.AddOrUpdateProperty(
                    new LogEventProperty("CorrelationId",
                    new ScalarValue(_executionContextAccessor.CorrelationId))
                );
        }
    }
}
