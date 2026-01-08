using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core.Activators.Reflection;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations;

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
