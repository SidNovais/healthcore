using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

internal class LoggingCommandHandlerWithResultDecorator<T, TResult>(
    ILogger logger,
    IExecutionContextAccessor executionContextAccessor,
    ICommandHandler<T, TResult> decorated
) : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly ILogger _logger = logger;
    private readonly IExecutionContextAccessor _executionContextAccessor = executionContextAccessor;
    private readonly ICommandHandler<T, TResult> _decorated = decorated;

    public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        if (command is IRecurringCommand)
        {
            return await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);
        }

        using (
            LogContext.Push(
                new RequestLogEnricher(_executionContextAccessor),
                new CommandLogEnricher(command)))
        {
            try
            {
                _logger.Information(
                    "Executing command {@Command}",
                    command);

                TResult? result = await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

                _logger.Information("Command {Command} processed successful, result {Result}", command.GetType().Name, result);

                return result;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Command processing failed");
                throw;
            }
        }
    }

    private class CommandLogEnricher(ICommand<TResult> command) : ILogEventEnricher
    {
        private readonly ICommand<TResult> _command = command;

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
