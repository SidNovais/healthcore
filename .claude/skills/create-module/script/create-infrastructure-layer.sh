#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Use: ./create-infrastructure-layer.sh [ModuleName] [RootNamespace] [BaseModulesDir]"
  exit 1
fi

camel_to_snake() {
  local input="$1"
  echo "$input" | sed -E 's/([A-Z])/_\L\1/g' | sed 's/^_//'
}

MODULE_NAME=$1
ROOT_NS=$2
BASE_MODULES_DIR=$3
MODULE_NAME_SNAKE_FORMAT=$(camel_to_snake "$MODULE_NAME")
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}
CONFIGURATION_DIR=$BASE_MODULE_DIR/Infrastructure/Configurations
AUTHENTICATION_DIR=$CONFIGURATION_DIR/Authentication
AGGREGATE_STORE_DIR=$CONFIGURATION_DIR/AggregateStore
DATA_ACCESS_DIR=$CONFIGURATION_DIR/DataAccess
EVENTS_BUS_DIR=$CONFIGURATION_DIR/EventsBus
LOGGING_DIR=$CONFIGURATION_DIR/Logging
MEDIATION_DIR=$CONFIGURATION_DIR/Mediation
PROCESSING_DIR=$CONFIGURATION_DIR/Processing
QUARTZ_DIR=$CONFIGURATION_DIR/Quartz
INTERNAL_COMMANDS_DIR=$BASE_MODULE_DIR/Infrastructure/InternalCommands
OUTBOX_DIR=$BASE_MODULE_DIR/Infrastructure/Outbox

dotnet new classlib -n ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure -o ${BASE_MODULE_DIR}/Infrastructure

mkdir -p $CONFIGURATION_DIR
mkdir -p $AUTHENTICATION_DIR
mkdir -p $AGGREGATE_STORE_DIR
mkdir -p $DATA_ACCESS_DIR
mkdir -p $EVENTS_BUS_DIR
mkdir -p $LOGGING_DIR
mkdir -p $MEDIATION_DIR
mkdir -p $PROCESSING_DIR
mkdir -p $PROCESSING_DIR/Inbox
mkdir -p $PROCESSING_DIR/InternalCommands
mkdir -p $PROCESSING_DIR/Outbox
mkdir -p $QUARTZ_DIR
mkdir -p $INTERNAL_COMMANDS_DIR
mkdir -p $OUTBOX_DIR

# ── Root ──────────────────────────────────────────────────────────────────────

cat > "${BASE_MODULE_DIR}/Infrastructure/${MODULE_NAME}Context.cs" << EOF
using HC.Core.Infrastructure.InternalCommands;
using HC.Core.Infrastructure.Outbox;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.InternalCommands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure;

public class ${MODULE_NAME}Context(
    DbContextOptions options
) : DbContext(options)
{
    public DbSet<InternalCommand> InternalCommands { get; set; }
    internal DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InternalCommandEntityTypeConfiguration());
    }
}
EOF

cat > "${BASE_MODULE_DIR}/Infrastructure/${MODULE_NAME}Module.cs" << EOF
using Autofac;
using MediatR;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure;

public class ${MODULE_NAME}Module : I${MODULE_NAME}Module
{
    public async Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command)
    {
        return await CommandsExecutor.Execute(command).ConfigureAwait(false);
    }

    public async Task ExecuteCommandAsync(ICommand command)
    {
        await CommandsExecutor.Execute(command).ConfigureAwait(false);
    }

    public async Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = ${MODULE_NAME}CompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
EOF

# ── InternalCommands (EF entity type config) ──────────────────────────────────

cat > "${INTERNAL_COMMANDS_DIR}/InternalCommandEntityTypeConfiguration.cs" << EOF
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HC.Core.Infrastructure.InternalCommands;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.InternalCommands;

internal class InternalCommandEntityTypeConfiguration : IEntityTypeConfiguration<InternalCommand>
  {
    public void Configure(EntityTypeBuilder<InternalCommand> builder)
    {
      builder.ToTable("InternalCommands", "${MODULE_NAME_SNAKE_FORMAT}");

      builder.HasKey(b => b.Id);
      builder.Property(b => b.Id).ValueGeneratedNever();
    }
  }
EOF

# ── Outbox (EF entity type config) ───────────────────────────────────────────

cat > "${OUTBOX_DIR}/OutboxMessageEntityTypeConfiguration.cs" << EOF
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HC.Core.Infrastructure.Outbox;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Outbox;

  internal class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
  {
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
      builder.ToTable("OutboxMessages", "${MODULE_NAME_SNAKE_FORMAT}");

      builder.HasKey(b => b.Id);
      builder.Property(b => b.Id).ValueGeneratedNever();
    }
  }
EOF

# ── Configurations ────────────────────────────────────────────────────────────

cat > "${CONFIGURATION_DIR}/AllConstructorFinder.cs" << EOF
using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;

internal class AllConstructorFinder : IConstructorFinder
{
    private static readonly ConcurrentDictionary<Type, ConstructorInfo[]> s_cache =
     new();

    public ConstructorInfo[] FindConstructors(Type targetType)
    {
        ConstructorInfo[] result = s_cache.GetOrAdd(
            targetType,
            t => [.. t.GetTypeInfo().DeclaredConstructors]);
        return result.Length > 0 ? result : throw new NoConstructorsFoundException(targetType, this);
    }
}
EOF

cat > "${CONFIGURATION_DIR}/Assemblies.cs" << EOF
using System.Reflection;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;
using ${ROOT_NS}.Modules.${MODULE_NAME}.IntegrationEvents;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;
  internal static class Assemblies
  {
    public static readonly Assembly Application = typeof(I${MODULE_NAME}Module).Assembly;
    public static readonly Assembly IntegrationEvents = typeof(IntegrationEventsAssemblyInfo).Assembly;
  }
EOF

cat > "${CONFIGURATION_DIR}/${MODULE_NAME}CompositionRoot.cs" << EOF
using Autofac;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;

internal static class ${MODULE_NAME}CompositionRoot
{
    private static IContainer s_container = null!;
    public static void SetContainer(IContainer container) => s_container = container;
    public static ILifetimeScope BeginLifetimeScope() => s_container.BeginLifetimeScope();
}
EOF

cat > "${CONFIGURATION_DIR}/${MODULE_NAME}Startup.cs" << EOF
using Autofac;
using HC.Core.Application;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.EventBus;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Authentication;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.DataAccess;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.EventBus;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Logging;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Mediation;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Quartz;
using Serilog;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;

public class ${MODULE_NAME}Startup
{
    private static IContainer _container = null!;
    public static void Initialize(
      string databaseConnectionString,
      IExecutionContextAccessor executionContextAccessor,
      ILogger logger,
      IEventsBus? eventBus,
      long? internalProcessingPoolingInterval = null
    )
    {
        ILogger moduleLogger = logger.ForContext("Module", "${MODULE_NAME}");
        ConfigureContainer(
          databaseConnectionString,
          executionContextAccessor,
          moduleLogger,
          eventBus
        );
        QuartzStartup.Initialize(moduleLogger, internalProcessingPoolingInterval);
        EventsBusStartup.Initialize(moduleLogger);
    }

    private static void ConfigureContainer(
      string databaseConnectionString,
      IExecutionContextAccessor executionContextAccessor,
      ILogger logger,
      IEventsBus? eventsBus
    )
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterModule(new LoggingModule(logger));
        containerBuilder.RegisterModule(new DataAccessModule(databaseConnectionString));
        containerBuilder.RegisterModule(new ApplicationModule());
        containerBuilder.RegisterModule(new ProcessingModule());
        containerBuilder.RegisterModule(new EventsBusModule(eventsBus));
        containerBuilder.RegisterModule(new MediatorModule());
        containerBuilder.RegisterModule(new AuthenticationModule());
        var domainNotificationsMap = new BiMap();
        containerBuilder.RegisterModule(new OutboxModule(domainNotificationsMap));
        BiMap internalCommandsMap = new();
        containerBuilder.RegisterModule(new InternalCommandsModule(internalCommandsMap));
        containerBuilder.RegisterModule(new QuartzModule());
        containerBuilder.RegisterInstance(executionContextAccessor);
        _container = containerBuilder.Build();
        ${MODULE_NAME}CompositionRoot.SetContainer(_container);
    }
    public static void Stop()
    {
        QuartzStartup.StopQuartz();
    }
}
EOF

cat > "${CONFIGURATION_DIR}/ApplicationModule.cs" << EOF
using Autofac;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Queries;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;

internal class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<QueryExecutor>()
          .As<IQueryExecutor>()
          .InstancePerLifetimeScope()
        ;
    }
}
EOF

# ── Authentication ─────────────────────────────────────────────────────────────

cat > "${AUTHENTICATION_DIR}/AuthenticationModule.cs" << EOF
using Autofac;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Authentication;

internal class AuthenticationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    { }
}
EOF

# ── AggregateStore ─────────────────────────────────────────────────────────────

cat > "${AGGREGATE_STORE_DIR}/AggregateStoreDomainEventsAccessor.cs" << EOF
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.Core.Infrastructure.DomainEventsDispatching;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.AggregateStore;

public class AggregateStoreDomainEventsAccessor(
  IAggregateStore aggregateStore
) : IDomainEventsAccessor
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public IReadOnlyCollection<IDomainEvent> GetAllDomainEvents()
        => _aggregateStore.GetChanges().ToList().AsReadOnly();

    public void ClearAllDomainEvents() => _aggregateStore.ClearChanges();
}
EOF

cat > "${AGGREGATE_STORE_DIR}/DomainEventTypeMappings.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.AggregateStore;

internal static class DomainEventTypeMappings
{
    internal static IDictionary<string, Type> Dictionary { get; }

    static DomainEventTypeMappings()
    {
        Dictionary = new Dictionary<string, Type>
        {
            // Register domain event type mappings here, e.g.:
            // { "MyDomainEvent", typeof(MyDomainEvent) },
        };
    }
}
EOF

cat > "${AGGREGATE_STORE_DIR}/MartenAggregateStore.cs" << EOF
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.AggregateStore;

public class MartenAggregateStore(
  IDocumentSession documentSession
) : IAggregateStore
{
    private readonly IDocumentSession _documentSession = documentSession;
    private readonly List<IDomainEvent> _events = [];
    public void AppendChanges<T>(T aggregate) where T : AggregateRoot
    {
        IReadOnlyCollection<IDomainEvent> events = aggregate.GetDomainEvents();
        _documentSession.Events.Append(GetStreamId(aggregate), events);
        _events.AddRange(events);
    }

    public void Start<T>(T aggregate) where T : AggregateRoot
    {
        IReadOnlyCollection<IDomainEvent> events = aggregate.GetDomainEvents();
        _documentSession.Events.StartStream<T>(GetStreamId(aggregate), aggregate.GetDomainEvents());
        _events.AddRange(events);
    }

    public async Task<T?> Load<T>(AggregateId<T> aggregateId) where T : AggregateRoot
    {
        string streamId = GetStreamId(aggregateId);
        IReadOnlyList<IEvent> events = await _documentSession.Events.FetchStreamAsync(streamId).ConfigureAwait(false);
        IList<IDomainEvent> domainEvents = [];
        foreach (IEvent @event in events)
        {
            Type type = DomainEventTypeMappings.Dictionary[@event.EventType.Name];
            string json = JsonConvert.SerializeObject(@event.Data);
            var domainEvent = JsonConvert.DeserializeObject(json, type) as IDomainEvent;
            domainEvents.Add(domainEvent!);
        }
        if (!domainEvents.Any()) return null;
        T? aggregate = (T?)Activator.CreateInstance(typeof(T), true);
        if (aggregate is null) return null;
        aggregate.Load(domainEvents);
        return aggregate;
    }

    public IList<IDomainEvent> GetChanges() => _events;

    public void ClearChanges() => _events.Clear();

    private static string GetStreamId<T>(T aggregate)
      where T : AggregateRoot
      => \$"{aggregate.GetType().Name}-{aggregate.Id:N}";

    private static string GetStreamId<T>(AggregateId<T> aggregateId)
      where T : AggregateRoot
      => \$"{typeof(T).Name}-{aggregateId.Value:N}";
}
EOF

cat > "${AGGREGATE_STORE_DIR}/SqlOutboxAccessor.cs" << EOF
using Marten;
using HC.Core.Infrastructure.Outbox;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.AggregateStore;

public class SqlOutboxAccessor(
  IDocumentSession documentSession
) : IOutbox
{
    private readonly IDocumentSession _documentSession = documentSession;
    private readonly List<OutboxMessage> _messages = [];
    public void Add(OutboxMessage message)
    {
        _messages.Add(message);
    }

    public Task Save()
    {
        if (_messages.Count != 0)
        {
            const string sql = @"INSERT INTO ""${MODULE_NAME_SNAKE_FORMAT}"".""OutboxMessages""
                      (""Id"", ""OccurredAt"", ""Type"", ""Data"")
                      VALUES (?::uuid, ?::timestamptz, ?, ?::jsonb)";
            foreach (OutboxMessage message in _messages)
                _documentSession.QueueSqlCommand(sql,
                  message.Id,
                  message.OccurredAt,
                  message.Type!,
                  message.Data!
                );
            _messages.Clear();
        }
        return Task.CompletedTask;
    }
}
EOF

# ── DataAccess ────────────────────────────────────────────────────────────────

cat > "${DATA_ACCESS_DIR}/DataAccessModule.cs" << EOF
using System.Reflection;
using Autofac;
using HC.Core.Application.Projections;
using HC.Core.Domain.EventSourcing;
using HC.Core.Infastructure;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.Data;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.AggregateStore;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.DataAccess;

internal class DataAccessModule(
  string databaseConnectionString
) : Autofac.Module
{
    private readonly string _databaseConnectionString = databaseConnectionString;
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SqlConnectionFactory>()
          .As<ISqlConnectionFactory>()
          .WithParameter("connectionString", _databaseConnectionString)
          .InstancePerLifetimeScope()
        ;
        builder
          .Register(c =>
          {
              var dbContextOptionsBuilder = new DbContextOptionsBuilder<${MODULE_NAME}Context>();
              dbContextOptionsBuilder.UseNpgsql(_databaseConnectionString);
              dbContextOptionsBuilder
              .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
              return new ${MODULE_NAME}Context(dbContextOptionsBuilder.Options);
          })
          .AsSelf()
          .As<DbContext>()
          .InstancePerLifetimeScope()
        ;
        Assembly applicationAssembly = typeof(ApplicationAssemblyInfo).Assembly;
        builder.RegisterAssemblyTypes(applicationAssembly)
          .Where(type => type.Name.EndsWith("Projector", StringComparison.Ordinal))
          .AsImplementedInterfaces()
          .InstancePerLifetimeScope()
          .FindConstructorsWith(new AllConstructorFinder())
        ;
        builder.Register(ctx => MartenConfig.BuildDocumentStore(_databaseConnectionString))
        .As<IDocumentStore>()
        .SingleInstance()
        ;
        builder.Register(ctx =>
        {
            IDocumentStore store = ctx.Resolve<IDocumentStore>();
            return store.LightweightSession();
        })
        .As<IDocumentSession>()
        .InstancePerLifetimeScope()
        ;
        builder.RegisterType<MartenAggregateStore>()
        .As<IAggregateStore>()
        .InstancePerLifetimeScope()
        ;
    }

}
EOF

cat > "${DATA_ACCESS_DIR}/MartenConfig.cs" << EOF
using JasperFx;
using JasperFx.Events;
using Marten;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.DataAccess;

public static class MartenConfig
{
    public static IDocumentStore BuildDocumentStore(string connectionString)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "${MODULE_NAME_SNAKE_FORMAT}";
            options.AutoCreateSchemaObjects = AutoCreate.None;
            options.Events.StreamIdentity = StreamIdentity.AsString;
            // Register domain event types here, e.g.:
            // options.Events.AddEventType<MyDomainEvent>();
        });
        return store;
    }
}
EOF

# ── EventsBus ─────────────────────────────────────────────────────────────────

cat > "${EVENTS_BUS_DIR}/EventsBusModule.cs" << EOF
using Autofac;
using HC.Core.Infrastructure.EventBus;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.EventBus;

internal class EventsBusModule(IEventsBus? eventsBus) : Autofac.Module
{
    private readonly IEventsBus? _eventsBus = eventsBus;

    protected override void Load(ContainerBuilder builder)
    {
        if (_eventsBus != null)
        {
            builder.RegisterInstance(_eventsBus).SingleInstance();
        }
        else
        {
            builder.RegisterType<InMemoryEventBusClient>()
                .As<IEventsBus>()
                .SingleInstance();
        }
    }
}
EOF

cat > "${EVENTS_BUS_DIR}/EventsBusStartup.cs" << EOF
using Autofac;
using Serilog;
using HC.Core.Infrastructure.EventBus;
using HC.COre.Infrastructure.EventBus;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.EventBus;

internal static class EventsBusStartup
{
    internal static void Initialize(
        ILogger logger
    )
    {
        SubscribeToIntegrationEvents(logger);
    }

    private static void SubscribeToIntegrationEvents(ILogger logger)
    {
        IEventsBus eventBus = ${MODULE_NAME}CompositionRoot.BeginLifetimeScope().Resolve<IEventsBus>();
    }

    private static void SubscribeToIntegrationEvent<T>(IEventsBus eventBus, ILogger logger)
        where T : IntegrationEvent
    {
        logger.Information("Subscribe to {@IntegrationEvent}", typeof(T).FullName);
        eventBus.Subscribe(
            new IntegrationEventGenericHandler<T>());
    }
}
EOF

cat > "${EVENTS_BUS_DIR}/IntegrationEventGenericHandler.cs" << EOF
using System.Data;
using Autofac;
using Dapper;
using Newtonsoft.Json;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.EventBus;
using HC.Core.Infrastructure.Serialization;
using HC.COre.Infrastructure.EventBus;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.EventBus;

internal class IntegrationEventGenericHandler<T> : IIntegrationEventListener<T>
    where T : IntegrationEvent
{
    public async Task Handle(T integrationEvent)
    {
        using ILifetimeScope scope = ${MODULE_NAME}CompositionRoot.BeginLifetimeScope();
        using IDbConnection? connection = scope.Resolve<ISqlConnectionFactory>().GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to insert inbox messages");
        string? type = integrationEvent.GetType().FullName;
        var data = JsonConvert.SerializeObject(integrationEvent, new JsonSerializerSettings
        {
            ContractResolver = new AllPropertiesContractResolver()
        });

        string sql = @\$"INSERT INTO ""${MODULE_NAME_SNAKE_FORMAT}"".""InboxMessages"" (""Id"", ""OccurredAt"", ""Type"", ""Data"") " +
                  "VALUES (@Id, @OccurredAt, @Type, @Data)";

        await connection.ExecuteScalarAsync(sql, new
        {
            integrationEvent.Id,
            integrationEvent.OccurredAt,
            type,
            data
        }).ConfigureAwait(false);
    }
}
EOF

# ── Logging ───────────────────────────────────────────────────────────────────

cat > "${LOGGING_DIR}/LoggingModule.cs" << EOF
using Autofac;
using Serilog;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Logging;

internal class LoggingModule(ILogger logger) : Autofac.Module
{
    private readonly ILogger _logger = logger;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(_logger)
          .As<ILogger>()
          .SingleInstance()
        ;
    }
}
EOF

# ── Mediation ─────────────────────────────────────────────────────────────────

cat > "${MEDIATION_DIR}/MediatorModule.cs" << EOF
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Features.Variance;
using FluentValidation;
using MediatR;
using MediatR.Pipeline;
using HC.Core.Infrastructure;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Queries;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Mediation;

public class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ServiceProviderWrapper>()
        .As<IServiceProvider>()
        .InstancePerDependency()
        .IfNotRegistered(typeof(IServiceProvider));

        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        var mediatorOpenTypes = new[]
        {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>),
            typeof(INotificationHandler<>),
            typeof(IValidator<>),
            typeof(IRequestPreProcessor<>),
            typeof(IStreamRequestHandler<,>),
            typeof(IRequestPostProcessor<,>),
            typeof(IRequestExceptionHandler<,,>),
            typeof(IRequestExceptionAction<,>),
        };
        builder.RegisterSource(new ScopedContravariantRegistrationSource(
            mediatorOpenTypes));
        foreach (Type? mediatorOpenType in mediatorOpenTypes)
        {
            builder
                .RegisterAssemblyTypes(Assemblies.Application, ThisAssembly)
                .AsClosedTypesOf(mediatorOpenType)
                .AsImplementedInterfaces()
                .FindConstructorsWith(new AllConstructorFinder());
        }

        builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
    }

    private class ScopedContravariantRegistrationSource : IRegistrationSource
    {
        private readonly ContravariantRegistrationSource _source = new();
        private readonly List<Type> _types = [];

        public ScopedContravariantRegistrationSource(params Type[] types)
        {
            ArgumentNullException.ThrowIfNull(types);

            if (!types.All(x => x.IsGenericTypeDefinition))
            {
                throw new ArgumentException("Supplied types should be generic type definitions");
            }

            _types.AddRange(types);
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            IEnumerable<IComponentRegistration> components = _source.RegistrationsFor(service, registrationAccessor);
            foreach (IComponentRegistration c in components)
            {
                IEnumerable<Type> defs = c.Target.Services
                    .OfType<TypedService>()
                    .Select(x => x.ServiceType.GetGenericTypeDefinition());

                if (defs.Any(_types.Contains))
                {
                    yield return c;
                }
            }
        }

        public bool IsAdapterForIndividualComponents => _source.IsAdapterForIndividualComponents;
    }
}
EOF

# ── Processing ────────────────────────────────────────────────────────────────

cat > "${PROCESSING_DIR}/CommandsExecutor.cs" << EOF
using Autofac;
using MediatR;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

internal static class CommandsExecutor
{
    internal static async Task Execute(ICommand command)
    {
        using (ILifetimeScope scope = ${MODULE_NAME}CompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            await mediator.Send(command).ConfigureAwait(false);
        }
    }

    internal static async Task<TResult> Execute<TResult>(ICommand<TResult> command)
    {
        using (ILifetimeScope scope = ${MODULE_NAME}CompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(command).ConfigureAwait(false);
        }
    }
}
EOF

cat > "${PROCESSING_DIR}/IRecurringCommand.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

public interface IRecurringCommand { }
EOF

cat > "${PROCESSING_DIR}/LoggingCommandHandlerDecorator.cs" << EOF
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using HC.Core.Application;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

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
            logEvent.AddOrUpdateProperty(new LogEventProperty("Context", new ScalarValue(\$"Command:{_command.Id}")));
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
EOF

cat > "${PROCESSING_DIR}/LoggingCommandHandlerWithResultDecorator.cs" << EOF
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using HC.Core.Application;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

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
            logEvent.AddOrUpdateProperty(new LogEventProperty("Context", new ScalarValue(\$"Command:{_command.Id}")));
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
EOF

cat > "${PROCESSING_DIR}/ProcessingModule.cs" << EOF
using Autofac;
using MediatR;
using HC.Core.Application.Events;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.DomainEventsDispatching;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.AggregateStore;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

internal class ProcessingModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DomainEventsDispatcher>()
            .As<IDomainEventsDispatcher>()
            .InstancePerLifetimeScope();

        builder.RegisterType<AggregateStoreDomainEventsAccessor>()
            .As<IDomainEventsAccessor>()
            .InstancePerLifetimeScope();

        builder.RegisterType<${MODULE_NAME}UnitOfWork>()
              .As<IUnitOfWork>()
              .InstancePerLifetimeScope();

        builder.RegisterType<CommandsScheduler>()
            .As<ICommandsScheduler>()
            .InstancePerLifetimeScope();

        builder.RegisterGenericDecorator(
            typeof(UnitOfWorkCommandHandlerDecorator<>),
            typeof(ICommandHandler<>));

        builder.RegisterGenericDecorator(
            typeof(UnitOfWorkCommandHandlerWithResultDecorator<,>),
            typeof(ICommandHandler<,>));

        builder.RegisterGenericDecorator(
            typeof(ValidationCommandHandlerDecorator<>),
            typeof(ICommandHandler<>));

        builder.RegisterGenericDecorator(
            typeof(ValidationCommandHandlerWithResultDecorator<,>),
            typeof(ICommandHandler<,>));

        builder.RegisterGenericDecorator(
            typeof(LoggingCommandHandlerDecorator<>),
            typeof(IRequestHandler<>));

        builder.RegisterGenericDecorator(
            typeof(LoggingCommandHandlerWithResultDecorator<,>),
            typeof(IRequestHandler<,>));

        builder.RegisterGenericDecorator(
            typeof(DomainEventsDispatcherNotificationHandlerDecorator<>),
            typeof(INotificationHandler<>));

        builder.RegisterAssemblyTypes(Assemblies.Application)
            .AsClosedTypesOf(typeof(IDomainEventNotification<>))
            .InstancePerDependency()
            .FindConstructorsWith(new AllConstructorFinder());
    }
}
EOF

cat > "${PROCESSING_DIR}/QueryExecutor.cs" << EOF
using Autofac;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Queries;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;
using MediatR;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

public class QueryExecutor : IQueryExecutor
{
    public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
    {
        using (ILifetimeScope scope = ${MODULE_NAME}CompositionRoot.BeginLifetimeScope())
        {
            IMediator mediator = scope.Resolve<IMediator>();
            return await mediator.Send(query).ConfigureAwait(false);
        }
    }
}
EOF

cat > "${PROCESSING_DIR}/${MODULE_NAME}UnitOfWork.cs" << EOF
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.DomainEventsDispatching;
using HC.Core.Infrastructure.Outbox;
using Marten;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

public class ${MODULE_NAME}UnitOfWork(
  IDomainEventsDispatcher domainEventsDispatcher,
  IDocumentSession documentSession,
  IOutbox outbox
) : IUnitOfWork
{
    private readonly IDomainEventsDispatcher _domainEventsDispatcher = domainEventsDispatcher;
    private readonly IDocumentSession _documentSession = documentSession;
    private readonly IOutbox _outbox = outbox;
    public async Task<int> CommitAsync(
    Guid? internalCommandId = null,
    CancellationToken cancellationToken = default
  )
    {
        await _domainEventsDispatcher.DispatchEventsAsync().ConfigureAwait(false);
        await _outbox.Save().ConfigureAwait(false);
        if (internalCommandId.HasValue)
        {
            _documentSession.QueueSqlCommand(
              @\$"UPDATE ""${MODULE_NAME_SNAKE_FORMAT}"".""InternalCommands""
                SET ""ProcessedDate"" = ?
                WHERE ""Id"" = ?",
              DateTimeOffset.UtcNow,
              internalCommandId.Value
            );
        }
        await _documentSession.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return 0;
    }
}
EOF

cat > "${PROCESSING_DIR}/UnitOfWorkCommandHandlerDecorator.cs" << EOF
using HC.Core.Infrastructure;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

internal class UnitOfWorkCommandHandlerDecorator<T>(
    ICommandHandler<T> decorated,
    IUnitOfWork unitOfWork
) : ICommandHandler<T>
    where T : ICommand
{
    private readonly ICommandHandler<T> _decorated = decorated;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

        Guid? internalCommandId = null;
        if (command is InternalCommandBase internalCommandBase)
        {
            internalCommandId = internalCommandBase.Id;
        }

        await _unitOfWork.CommitAsync(
          internalCommandId,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }
}
EOF

cat > "${PROCESSING_DIR}/UnitOfWorkCommandHandlerWithResultDecorator.cs" << EOF
using HC.Core.Infrastructure;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

internal class UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>(
    ICommandHandler<T, TResult> decorated,
    IUnitOfWork unitOfWork
) : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly ICommandHandler<T, TResult> _decorated = decorated;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        TResult? result = await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);

        Guid? internalCommandId = null;
        if (command is InternalCommandBase<TResult> internalCommandBase)
        {
            internalCommandId = internalCommandBase.Id;
        }

        await _unitOfWork.CommitAsync(
          internalCommandId,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return result;
    }
}
EOF

cat > "${PROCESSING_DIR}/ValidationCommandHandlerDecorator.cs" << EOF
using FluentValidation;
using HC.Core.Application;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

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
EOF

cat > "${PROCESSING_DIR}/ValidationCommandHandlerWithResultDecorator.cs" << EOF
using FluentValidation;
using HC.Core.Application;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

internal class ValidationCommandHandlerWithResultDecorator<T, TResult>(
    IList<IValidator<T>> validators,
    ICommandHandler<T, TResult> decorated
) : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly IList<IValidator<T>> _validators = validators;
    private readonly ICommandHandler<T, TResult> _decorated = decorated;

    public Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        var errors = _validators
            .Select(v => v.Validate(command))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (errors.Count > 0)
            throw new InvalidCommandException(string.Join(";", errors.Select(x => x.ErrorMessage).ToList()));

        return _decorated.Handle(command, cancellationToken);
    }
}
EOF

# ── Processing/Inbox ──────────────────────────────────────────────────────────

cat > "${PROCESSING_DIR}/Inbox/InboxMessageDto.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Inbox;

public class InboxMessageDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
}
EOF

cat > "${PROCESSING_DIR}/Inbox/ProcessInboxCommand.cs" << EOF
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Inbox;

public class ProcessInboxCommand : CommandBase, IRecurringCommand
{
}
EOF

cat > "${PROCESSING_DIR}/Inbox/ProcessInboxCommandHandler.cs" << EOF
using System.Data;
using System.Reflection;
using Dapper;
using MediatR;
using Newtonsoft.Json;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Inbox;

internal class ProcessInboxCommandHandler(
  IMediator mediator,
  ISqlConnectionFactory sqlConnectionFactory
) : ICommandHandler<ProcessInboxCommand>
{
    private readonly IMediator _mediator = mediator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Handle(ProcessInboxCommand command, CancellationToken cancellationToken)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to update internal commands");
        string sql = @\$"SELECT
                   ""InboxMessage"".""Id"" AS ""{nameof(InboxMessageDto.Id)}"",
                   ""InboxMessage"".""Type"" AS ""{nameof(InboxMessageDto.Type)}"",
                   ""InboxMessage"".""Data"" AS ""{nameof(InboxMessageDto.Data)}""
                   FROM ""${MODULE_NAME_SNAKE_FORMAT}"".""InboxMessages"" AS ""InboxMessage""
                   WHERE ""InboxMessage"".""ProcessedDate"" IS NULL
                   ORDER BY ""InboxMessage"".""OccurredAt""";

        IEnumerable<InboxMessageDto> messages =
            await connection.QueryAsync<InboxMessageDto>(sql).ConfigureAwait(false);

        const string sqlUpdateProcessedDate = @\$"UPDATE ""${MODULE_NAME_SNAKE_FORMAT}"".""InboxMessages""
                                            SET ""ProcessedDate"" = @Date
                                            WHERE ""Id"" = @Id";

        foreach (InboxMessageDto? message in messages)
        {
            Assembly? messageAssembly;
            if (message is not null && message.Type is not null)
            {
                messageAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .SingleOrDefault(
                        assembly => message.Type.Contains(assembly.GetName().Name ?? string.Empty, StringComparison.Ordinal)
                    );
                Type? type = null;
                if (messageAssembly is not null && message.Type is not null)
                    type = messageAssembly.GetType(message.Type);
                if (type is not null)
                {
                    object? request = JsonConvert.DeserializeObject(message.Data, type);

                    if (request is not null)
                        try
                        {
                            await _mediator.Publish((INotification)request, cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    await connection.ExecuteScalarAsync(sqlUpdateProcessedDate, new
                    {
                        Date = SystemClock.Now,
                        message.Id
                    })
                    .ConfigureAwait(false);
                }
            }
        }
    }
}
EOF

cat > "${PROCESSING_DIR}/Inbox/ProcessInboxJob.cs" << EOF
using Quartz;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Inbox;

[DisallowConcurrentExecution]
public class ProcessInboxJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await CommandsExecutor.Execute(new ProcessInboxCommand()).ConfigureAwait(false);
    }
}
EOF

# ── Processing/InternalCommands ───────────────────────────────────────────────

cat > "${PROCESSING_DIR}/InternalCommands/CommandsScheduler.cs" << EOF
using Dapper;
using System.Data;
using Newtonsoft.Json;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.InternalCommands;
using HC.Core.Infrastructure.Serialization;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;

public class CommandsScheduler(
    ISqlConnectionFactory sqlConnectionFactory,
    IInternalCommandsMapper internalCommandsMapper
) : ICommandsScheduler
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    private readonly IInternalCommandsMapper _internalCommandsMapper = internalCommandsMapper;

    public async Task EnqueueAsync(ICommand command)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to insert internal commands");

        const string sqlInsert = @\$"INSERT INTO ""${MODULE_NAME_SNAKE_FORMAT}"".""InternalCommands"" (""Id"", ""EnqueueDate"" , ""Type"", ""Data"") VALUES " +
                                 "(@Id, @EnqueueDate, @Type, @Data)";

        await connection.ExecuteAsync(sqlInsert, new
        {
            command.Id,
            EnqueueDate = SystemClock.Now,
            Type = _internalCommandsMapper.GetNameByType(command.GetType()),
            Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            })
        }).ConfigureAwait(false);
    }

    public async Task EnqueueAsync<T>(ICommand<T> command)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to insert internal commands");

        const string sqlInsert = @\$"INSERT INTO ""${MODULE_NAME_SNAKE_FORMAT}"".""InternalCommands"" (""Id"", ""EnqueueDate"" , ""Type"", ""Data"") VALUES " +
                                 "(@Id, @EnqueueDate, @Type, @Data)";

        await connection.ExecuteAsync(sqlInsert, new
        {
            command.Id,
            EnqueueDate = SystemClock.Now,
            Type = _internalCommandsMapper.GetNameByType(command.GetType()),
            Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            })
        }).ConfigureAwait(false);
    }
}
EOF

cat > "${PROCESSING_DIR}/InternalCommands/InternalCommandsModule.cs" << EOF
using Autofac;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.InternalCommands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;

internal class InternalCommandsModule(BiMap internalCommandsMap) : Autofac.Module
{
    private readonly BiMap _internalCommandsMap = internalCommandsMap;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<InternalCommandsMapper>()
            .As<IInternalCommandsMapper>()
            .FindConstructorsWith(new AllConstructorFinder())
            .WithParameter("internalCommandsMap", _internalCommandsMap)
            .SingleInstance();

        CheckMappings();
    }

    private void CheckMappings()
    {
        var internalCommands = Assemblies.Application
            .GetTypes()
            .Where(x => x.BaseType != null &&
                        (
                            (x.BaseType.IsGenericType &&
                            x.BaseType.GetGenericTypeDefinition() == typeof(InternalCommandBase<>)) ||
                            x.BaseType == typeof(InternalCommandBase)))
            .ToList();

        List<Type> notMappedInternalCommands = new List<Type>();
        foreach (Type? internalCommand in internalCommands)
        {
            _internalCommandsMap.TryGetBySecond(internalCommand, out var name);

            if (name == null)
            {
                notMappedInternalCommands.Add(internalCommand);
            }
        }

        if (notMappedInternalCommands.Count > 0)
            throw new NotMappedInternalCommandsException(\$"Internal Commands {notMappedInternalCommands.Select(x => x.FullName).Aggregate((x, y) => x + "," + y)} not mapped");
    }
    public class NotMappedInternalCommandsException : Exception
    {
        public NotMappedInternalCommandsException()
        {
        }
        public NotMappedInternalCommandsException(string message) : base(message)
        {
        }

        public NotMappedInternalCommandsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
EOF

cat > "${PROCESSING_DIR}/InternalCommands/ProcessInternalCommandsCommand.cs" << EOF
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;

internal class ProcessInternalCommandsCommand : CommandBase, IRecurringCommand
{
}
EOF

cat > "${PROCESSING_DIR}/InternalCommands/ProcessInternalCommandsCommandHandler.cs" << EOF
using Dapper;
using Polly;
using System.Data;
using Newtonsoft.Json;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.InternalCommands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;

internal class ProcessInternalCommandsCommandHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IInternalCommandsMapper internalCommandsMapper
) : ICommandHandler<ProcessInternalCommandsCommand>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    private readonly IInternalCommandsMapper _internalCommandsMapper = internalCommandsMapper;

    public async Task Handle(ProcessInternalCommandsCommand command, CancellationToken cancellationToken)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to update internal commands");
        string sql = @\$"SELECT
                    ""Command"".""Id"" AS ""{nameof(InternalCommandDto.Id)}"",
                    ""Command"".""Type"" AS ""{nameof(InternalCommandDto.Type)}"",
                    ""Command"".""Data"" AS ""{nameof(InternalCommandDto.Data)}""
                    FROM ""${MODULE_NAME_SNAKE_FORMAT}"".""InternalCommands"" AS ""Command""
                    WHERE ""Command"".""ProcessedDate"" IS NULL
                    ORDER BY ""Command"".""EnqueueDate""";

        IEnumerable<InternalCommandDto> commands =
            await connection.QueryAsync<InternalCommandDto>(sql).ConfigureAwait(false);

        var internalCommandsList = commands.AsList();
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
            [
              TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3)
            ]);
        foreach (InternalCommandDto? internalCommand in internalCommandsList)
        {
            PolicyResult result = await policy.ExecuteAndCaptureAsync(() => ProcessCommand(
                internalCommand)).ConfigureAwait(false);

            if (result.Outcome == OutcomeType.Failure)
            {
                const string updateOnErrorSql = @\$"UPDATE ""${MODULE_NAME_SNAKE_FORMAT}"".""InternalCommands""
                                          SET ""ProcessedDate"" = @NowDate,
                                          ""Error"" = @Error
                                          WHERE ""Id"" = @Id";

                await connection.ExecuteScalarAsync(
                    updateOnErrorSql,
                    new
                    {
                        NowDate = SystemClock.Now,
                        Error = result.FinalException?.ToString(),
                        internalCommand.Id
                    }
                ).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessCommand(
        InternalCommandDto internalCommand)
    {
        Type? type = _internalCommandsMapper.GetTypeByName(internalCommand.Type);
        if (type is not null)
        {
            dynamic? commandToProcess = JsonConvert.DeserializeObject(internalCommand.Data, type);
            await CommandsExecutor.Execute(commandToProcess).ConfigureAwait(false);
        }
    }

    private class InternalCommandDto
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Data { get; set; } = string.Empty;
    }
}
EOF

cat > "${PROCESSING_DIR}/InternalCommands/ProcessInternalCommandsJob.cs" << EOF
using Quartz;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;

[DisallowConcurrentExecution]
public class ProcessInternalCommandsJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await CommandsExecutor.Execute(new ProcessInternalCommandsCommand()).ConfigureAwait(false);
    }
}
EOF

# ── Processing/Outbox ─────────────────────────────────────────────────────────

cat > "${PROCESSING_DIR}/Outbox/OutboxMessageDto.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;

public class OutboxMessageDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
}
EOF

cat > "${PROCESSING_DIR}/Outbox/OutboxModule.cs" << EOF
using Autofac;
using HC.Core.Application;
using HC.Core.Application.Events;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.DomainEventsDispatching;
using HC.Core.Infrastructure.Outbox;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.AggregateStore;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;

internal class OutboxModule(BiMap domainNotificationsMap) : Autofac.Module
{
    private readonly BiMap _domainNotificationsMap = domainNotificationsMap;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SqlOutboxAccessor>()
            .As<IOutbox>()
            .FindConstructorsWith(new AllConstructorFinder())
            .InstancePerLifetimeScope();

        CheckMappings();

        builder.RegisterType<DomainNotificationsMapper>()
            .As<IDomainNotificationsMapper>()
            .FindConstructorsWith(new AllConstructorFinder())
            .WithParameter("domainNotificationsMap", _domainNotificationsMap)
            .SingleInstance();
    }

    private void CheckMappings()
    {
        var domainEventNotifications = Assemblies.Application
            .GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(IDomainEventNotification)))
            .ToList();

        List<Type> notMappedNotifications = [];
        foreach (Type? domainEventNotification in domainEventNotifications)
        {
            _domainNotificationsMap.TryGetBySecond(domainEventNotification, out var name);

            if (name == null)
            {
                notMappedNotifications.Add(domainEventNotification);
            }
        }

        if (notMappedNotifications.Count > 0)
            throw new NotMappedNotificationsException(\$"Domain Event Notifications {notMappedNotifications.Select(x => x.FullName).Aggregate(static (x, y) => x + "," + y)} not mapped");
    }
}

public class NotMappedNotificationsException : Exception
{
    public NotMappedNotificationsException()
    {
    }
    public NotMappedNotificationsException(string message) : base(message)
    {
    }

    public NotMappedNotificationsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
EOF

cat > "${PROCESSING_DIR}/Outbox/ProcessOutboxCommand.cs" << EOF
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;

public class ProcessOutboxCommand : CommandBase, IRecurringCommand
{
}
EOF

cat > "${PROCESSING_DIR}/Outbox/ProcessOutboxCommandHandler.cs" << EOF
using MediatR;
using Newtonsoft.Json;
using System.Data;
using Dapper;
using Serilog.Context;
using Serilog.Events;
using Serilog.Core;
using HC.Core.Domain;
using HC.Core.Application;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.DomainEventsDispatching;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;
using HC.Core.Application.Events;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;

internal class ProcessOutboxCommandHandler(
    IMediator mediator,
    ISqlConnectionFactory sqlConnectionFactory,
    IDomainNotificationsMapper domainNotificationsMapper
) : ICommandHandler<ProcessOutboxCommand>
{
    private readonly IMediator _mediator = mediator;

    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    private readonly IDomainNotificationsMapper _domainNotificationsMapper = domainNotificationsMapper;

    public async Task Handle(
        ProcessOutboxCommand command,
        CancellationToken cancellationToken
    )
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to update outbox messages");
        string sql = @\$"SELECT
                    ""OutboxMessage"".""Id"" AS ""{nameof(OutboxMessageDto.Id)}"",
                    ""OutboxMessage"".""Type"" AS ""{nameof(OutboxMessageDto.Type)}"",
                    ""OutboxMessage"".""Data"" AS ""{nameof(OutboxMessageDto.Data)}""
                    FROM ""${MODULE_NAME_SNAKE_FORMAT}"".""OutboxMessages"" AS ""OutboxMessage""
                    WHERE ""OutboxMessage"".""ProcessedDate"" IS NULL
                    ORDER BY ""OutboxMessage"".""OccurredAt""";

        var messages = await connection.QueryAsync<OutboxMessageDto>(sql).ConfigureAwait(false);
        var messagesList = messages.AsList();

        const string sqlUpdateProcessedDate = @\$"UPDATE ""${MODULE_NAME_SNAKE_FORMAT}"".""OutboxMessages""
                                            SET ""ProcessedDate"" = @Date
                                            WHERE ""Id"" = @Id";
        if (messagesList.Count > 0)
        {
            foreach (OutboxMessageDto? message in messagesList)
            {
                Type? type = _domainNotificationsMapper.GetTypeByName(message.Type);
                if (type is not null)
                {
                    var @event = JsonConvert.DeserializeObject(message.Data, type) as IDomainEventNotification;

                    if (@event is not null)
                        using (LogContext.Push(new OutboxMessageContextEnricher(@event)))
                        {
                            await _mediator.Publish(@event, cancellationToken).ConfigureAwait(false);

                            await connection.ExecuteAsync(sqlUpdateProcessedDate, new
                            {
                                Date = SystemClock.Now,
                                message.Id
                            }).ConfigureAwait(false);
                        }
                }
            }
        }
    }

    private class OutboxMessageContextEnricher(IDomainEventNotification notification) : ILogEventEnricher
    {
        private readonly IDomainEventNotification _notification = notification;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty("Context", new ScalarValue(\$"OutboxMessage:{_notification.Id}")));
        }
    }
}
EOF

cat > "${PROCESSING_DIR}/Outbox/ProcessOutboxJob.cs" << EOF
using Quartz;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;

[DisallowConcurrentExecution]
public class ProcessOutboxJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await CommandsExecutor.Execute(new ProcessOutboxCommand()).ConfigureAwait(false);
    }
}
EOF

# ── Quartz ────────────────────────────────────────────────────────────────────

cat > "${QUARTZ_DIR}/QuartzModule.cs" << EOF
using Autofac;
using Quartz;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Quartz;

public class QuartzModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(x => typeof(IJob).IsAssignableFrom(x)).InstancePerDependency();
    }
}
EOF

cat > "${QUARTZ_DIR}/QuartzStartup.cs" << EOF
using System.Collections.Specialized;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Inbox;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.InternalCommands;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Serilog;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Quartz;

internal static class QuartzStartup
{
    private static IScheduler _scheduler = null!;

    internal static void Initialize(ILogger logger, long? internalProcessingPoolingInterval)
    {
        logger.Information("Quartz starting...");

        var schedulerConfiguration = new NameValueCollection
      {
          { "quartz.scheduler.instanceName", "${MODULE_NAME}" }
      };

        StdSchedulerFactory schedulerFactory = new(schedulerConfiguration);
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

        LogProvider.SetCurrentLogProvider(new SerilogLogProvider(logger));

        _scheduler.Start().GetAwaiter().GetResult();

        IJobDetail processOutboxJob = JobBuilder.Create<ProcessOutboxJob>().Build();

        ITrigger trigger;
        if (internalProcessingPoolingInterval.HasValue)
        {
            trigger =
                TriggerBuilder
                    .Create()
                    .StartNow()
                    .WithSimpleSchedule(x =>
                        x.WithInterval(TimeSpan.FromMilliseconds(internalProcessingPoolingInterval.Value))
                            .RepeatForever())
                    .Build();
        }
        else
        {
            trigger =
                TriggerBuilder
                    .Create()
                    .StartNow()
                    .WithCronSchedule("0/2 * * ? * *")
                    .Build();
        }

        _scheduler
            .ScheduleJob(processOutboxJob, trigger)
            .GetAwaiter().GetResult();

        IJobDetail processInboxJob = JobBuilder.Create<ProcessInboxJob>().Build();
        ITrigger processInboxTrigger =
            TriggerBuilder
                .Create()
                .StartNow()
                .WithCronSchedule("0/2 * * ? * *")
                .Build();

        _scheduler
            .ScheduleJob(processInboxJob, processInboxTrigger)
            .GetAwaiter().GetResult();

        IJobDetail processInternalCommandsJob = JobBuilder.Create<ProcessInternalCommandsJob>().Build();
        ITrigger triggerCommandsProcessing =
            TriggerBuilder
                .Create()
                .StartNow()
                .WithCronSchedule("0/2 * * ? * *")
                .Build();
        _scheduler.ScheduleJob(processInternalCommandsJob, triggerCommandsProcessing).GetAwaiter().GetResult();
        logger.Information("Quartz started.");
    }

    internal static void StopQuartz()
    {
        _scheduler.Shutdown();
    }
}
EOF

cat > "${QUARTZ_DIR}/SerilogLogProvider.cs" << EOF
using Quartz.Logging;
using Serilog;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Quartz;

internal class SerilogLogProvider : ILogProvider
{
    private readonly ILogger _logger;

    internal SerilogLogProvider(ILogger logger)
    {
        _logger = logger;
    }

    public Logger GetLogger(string name)
    {
        return (level, func, exception, parameters) =>
        {
            if (func == null)
            {
                return true;
            }

            if (level == LogLevel.Debug || level == LogLevel.Trace)
            {
                _logger.Debug(exception, func(), parameters);
            }

            if (level == LogLevel.Info)
            {
                _logger.Information(exception, func(), parameters);
            }

            if (level == LogLevel.Warn)
            {
                _logger.Warning(exception, func(), parameters);
            }

            if (level == LogLevel.Error)
            {
                _logger.Error(exception, func(), parameters);
            }

            if (level == LogLevel.Fatal)
            {
                _logger.Fatal(exception, func(), parameters);
            }

            return true;
        };
    }

    public IDisposable OpenNestedContext(string message)
      => throw new NotImplementedException();

    public IDisposable OpenMappedContext(string key, string value)
      => throw new NotImplementedException();

    public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
      => throw new NotImplementedException();
}
EOF
