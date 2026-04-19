using Autofac;
using HC.LIS.Modules.UserAccess.Application.Users;
using HC.LIS.Modules.UserAccess.Infrastructure.AuditLog;
using HC.LIS.Modules.UserAccess.Infrastructure.Authentication;
using HC.LIS.Modules.UserAccess.Infrastructure.Email;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Authentication;

internal class AuthenticationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PasswordHasher>()
            .As<IPasswordHasher>()
            .InstancePerLifetimeScope();

        builder.RegisterType<JwtTokenService>()
            .As<IJwtTokenService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<EmailService>()
            .As<IEmailService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<AuditLogWriter>()
            .As<IAuditLogWriter>()
            .InstancePerLifetimeScope();
    }
}
