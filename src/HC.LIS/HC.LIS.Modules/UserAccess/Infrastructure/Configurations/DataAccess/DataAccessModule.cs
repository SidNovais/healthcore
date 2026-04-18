using System.Reflection;
using Autofac;
using HC.Core.Infastructure;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations.DataAccess;

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
            .InstancePerLifetimeScope();

        builder
            .Register(c =>
            {
                var dbContextOptionsBuilder = new DbContextOptionsBuilder<UserAccessContext>();
                dbContextOptionsBuilder.UseNpgsql(_databaseConnectionString);
                dbContextOptionsBuilder
                    .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                return new UserAccessContext(dbContextOptionsBuilder.Options);
            })
            .AsSelf()
            .As<DbContext>()
            .InstancePerLifetimeScope();

        Assembly infrastructureAssembly = typeof(UserAccessContext).Assembly;
        builder.RegisterAssemblyTypes(infrastructureAssembly)
            .Where(type => type.Name.EndsWith("Repository", StringComparison.Ordinal))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(new AllConstructorFinder());
    }
}
