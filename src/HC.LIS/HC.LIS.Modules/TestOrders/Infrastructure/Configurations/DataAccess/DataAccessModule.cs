using System.Reflection;
using Autofac;
using HC.Core.Application.Projections;
using HC.Core.Domain.EventSourcing;
using HC.Core.Infastructure;
using HC.Core.Infrastructure;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.AggregateStore;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.DataAccess;

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
              var dbContextOptionsBuilder = new DbContextOptionsBuilder<TestOrdersContext>();
              dbContextOptionsBuilder.UseNpgsql(_databaseConnectionString);
              dbContextOptionsBuilder
              .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
              return new TestOrdersContext(dbContextOptionsBuilder.Options);
          })
          .AsSelf()
          .As<DbContext>()
          .InstancePerLifetimeScope()
        ;
        Assembly applicationAssembly = typeof(IProjector).Assembly;
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
