using Autofac;
using HC.LIS.Modules.PatientManagement.Application.Contracts;
using HC.LIS.Modules.PatientManagement.Infrastructure;

namespace HC.LIS.API.Modules.PatientManagement;

internal sealed class PatientManagementAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PatientManagementModule>()
            .As<IPatientManagementModule>()
            .InstancePerLifetimeScope();
    }
}
