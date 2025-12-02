using System;
using Autofac;

namespace HC.Core.Infrastructure;

public class ServiceProviderWrapper(ILifetimeScope scope) : IServiceProvider
{
    private readonly ILifetimeScope _scope = scope;
    public object? GetService(Type serviceType) => _scope.ResolveOptional(serviceType);
}
