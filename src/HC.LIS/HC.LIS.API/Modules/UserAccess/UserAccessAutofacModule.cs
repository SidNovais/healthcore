using Autofac;
using HC.LIS.Modules.UserAccess.Application.Contracts;
using HC.LIS.Modules.UserAccess.Infrastructure;

namespace HC.LIS.API.Modules.UserAccess;

internal sealed class UserAccessAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UserAccessModule>()
            .As<IUserAccessModule>()
            .InstancePerLifetimeScope();
    }
}
