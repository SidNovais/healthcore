using Autofac;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.InternalCommands;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations;

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
            throw new NotMappedInternalCommandsException($"Internal Commands {notMappedInternalCommands.Select(x => x.FullName).Aggregate((x, y) => x + "," + y)} not mapped");
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

