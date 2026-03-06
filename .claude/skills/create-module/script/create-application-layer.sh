#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Use: ./create-application-layer.sh [ModuleName] [RootNamespace] [BaseModulesDir]"
  exit 1
fi

MODULE_NAME=$1
ROOT_NS=$2
BASE_MODULES_DIR=$3
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}
CONFIGURATION_DIR=$BASE_MODULE_DIR/Application/Configuration
COMMANDS_DIR=$CONFIGURATION_DIR/Commands
QUERIES_DIR=$CONFIGURATION_DIR/Queries
CONTRACTS_DIR=$BASE_MODULE_DIR/Application/Contracts

dotnet new classlib -n ${ROOT_NS}.Modules.${MODULE_NAME}.Application -o ${BASE_MODULE_DIR}/Application

mkdir -p $CONFIGURATION_DIR
mkdir -p $CONTRACTS_DIR
mkdir -p $COMMANDS_DIR
mkdir -p $QUERIES_DIR

cat > "${BASE_MODULE_DIR}/Application/ApplicationAssemblyInfo.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application;

public class ApplicationAssemblyInfo
{
}
EOF

cat > "${CONTRACTS_DIR}/CommandBase.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

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
EOF

cat > "${CONTRACTS_DIR}/I${MODULE_NAME}Module.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

public interface I${MODULE_NAME}Module
{
    Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command);
    Task ExecuteCommandAsync(ICommand command);
    Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query);
}
EOF

cat > "${CONTRACTS_DIR}/ICommand.cs" << EOF
using MediatR;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

public interface ICommand<out TResult> : IRequest<TResult>
{
    Guid Id { get; }
}

public interface ICommand : IRequest
{
    Guid Id { get; }
}
EOF

cat > "${CONTRACTS_DIR}/IQuery.cs" << EOF
using MediatR;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
EOF

cat > "${CONTRACTS_DIR}/IRecurringCommand.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

public interface IRecurringCommand {}
EOF

cat > "${CONTRACTS_DIR}/QueryBase.cs" << EOF
namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

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
EOF

cat > "${COMMANDS_DIR}/ICommandHandler.cs" << EOF
using MediatR;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}

public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}
EOF

cat > "${COMMANDS_DIR}/ICommandsScheduler.cs" << EOF
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;

public interface ICommandsScheduler
{
    Task EnqueueAsync(ICommand command);
    Task EnqueueAsync<T>(ICommand<T> command);
}
EOF

cat > "${COMMANDS_DIR}/InternalCommandBase.cs" << EOF
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Commands;

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
EOF

cat > "${QUERIES_DIR}/IQueryExecutor.cs" << EOF
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Queries;

public interface IQueryExecutor
{
    Task<TResult> GetAsync<TResult>(IQuery<TResult> query);
}
EOF

cat > "${QUERIES_DIR}/IQueryHandler.cs" << EOF
using MediatR;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Configuration.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
EOF
