using System;

namespace HC.Core.Infrastructure.InternalCommands;

public interface IInternalCommandsMapper
{
    string? GetNameByType(Type type);

    Type? GetTypeByName(string name);
}
