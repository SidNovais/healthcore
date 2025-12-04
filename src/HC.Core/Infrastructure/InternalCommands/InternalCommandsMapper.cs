using System;

namespace HC.Core.Infrastructure.InternalCommands;

public class InternalCommandsMapper(
    BiMap internalCommandsMap
) : IInternalCommandsMapper
{
    private readonly BiMap _internalCommandsMap = internalCommandsMap;

    public string? GetNameByType(Type type)
    {
        return _internalCommandsMap.TryGetBySecond(type, out string? name) ? name : null;
    }

    public Type? GetTypeByName(string name)
    {
        return _internalCommandsMap.TryGetByFirst(name, out Type? type) ? type : null;
    }
}
