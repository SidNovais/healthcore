using System;
using Amazon;
using Amazon.S3;
using Autofac;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Reports;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Storage;

internal class StorageModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        string bucketName = Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_S3_BUCKET_NAME") ?? string.Empty;
        string region = Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_S3_REGION") ?? "us-east-1";

        var s3Options = new S3ReportStorageOptions { BucketName = bucketName, Region = region };
        builder.RegisterInstance(s3Options).AsSelf().SingleInstance();

        builder.Register(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(region)))
            .As<IAmazonS3>()
            .SingleInstance();

        builder.RegisterType<S3ReportStorage>()
            .As<IReportStorage>()
            .InstancePerLifetimeScope();

        builder.RegisterType<QuestPdfGenerator>()
            .As<IPdfGenerator>()
            .InstancePerLifetimeScope();

        builder.RegisterType<WorklistItemForSigningProvider>()
            .As<IWorklistItemForSigningProvider>()
            .InstancePerLifetimeScope();
    }
}
