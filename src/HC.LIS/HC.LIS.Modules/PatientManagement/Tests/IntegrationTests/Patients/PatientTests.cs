using System;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;
using HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;
using HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

namespace HC.LIS.Modules.PatientManagement.IntegrationTests.Patients;

public class PatientTests : TestBase
{
    public PatientTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task RegisterPatientIsSuccessful()
    {
        await PatientFactory.CreateAsync(PatientManagementModule).ConfigureAwait(true);

        PatientDetailsDto? details = await GetEventually(
            new GetPatientDetailsFromPatientManagementProbe(
                PatientSampleData.PatientId,
                PatientManagementModule),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Id.Should().Be(PatientSampleData.PatientId);
        details.FullName.Should().Be(PatientSampleData.FullName);
        details.DateOfBirth.Should().Be(PatientSampleData.DateOfBirth);
        details.Gender.Should().Be(PatientSampleData.Gender);
        details.MothersFullName.Should().Be(PatientSampleData.MothersFullName);
        details.DocumentId.Should().Be(PatientSampleData.DocumentId);
        details.Phone.Should().Be(PatientSampleData.Phone);
        details.Email.Should().Be(PatientSampleData.Email);
        details.Status.Should().Be("Active");
        details.RegisteredAt.Should().NotBe(default(DateTime));
        details.AnonymizedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePatientIsSuccessful()
    {
        await PatientFactory.CreateAsync(PatientManagementModule).ConfigureAwait(true);

        await GetEventually(
            new GetPatientDetailsFromPatientManagementProbe(
                PatientSampleData.PatientId,
                PatientManagementModule),
            15000
        ).ConfigureAwait(true);

        await PatientManagementModule.ExecuteCommandAsync(
            new UpdatePatientCommand(
                PatientSampleData.PatientId,
                PatientSampleData.UpdatedFullName,
                PatientSampleData.UpdatedDateOfBirth,
                PatientSampleData.UpdatedGender,
                PatientSampleData.UpdatedMothersFullName,
                PatientSampleData.UpdatedDocumentId,
                PatientSampleData.UpdatedPhone,
                PatientSampleData.UpdatedEmail,
                SystemClock.Now
            )
        ).ConfigureAwait(true);

        PatientDetailsDto? details = await GetEventually(
            new GetPatientDetailsFromPatientManagementProbe(
                PatientSampleData.PatientId,
                PatientManagementModule,
                dto => dto?.FullName == PatientSampleData.UpdatedFullName),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Id.Should().Be(PatientSampleData.PatientId);
        details.FullName.Should().Be(PatientSampleData.UpdatedFullName);
        details.DateOfBirth.Should().Be(PatientSampleData.UpdatedDateOfBirth);
        details.Gender.Should().Be(PatientSampleData.UpdatedGender);
        details.MothersFullName.Should().Be(PatientSampleData.UpdatedMothersFullName);
        details.DocumentId.Should().Be(PatientSampleData.UpdatedDocumentId);
        details.Phone.Should().Be(PatientSampleData.UpdatedPhone);
        details.Email.Should().Be(PatientSampleData.UpdatedEmail);
        details.Status.Should().Be("Active");
        details.AnonymizedAt.Should().BeNull();
    }

    [Fact]
    public async Task AnonymizePatientIsSuccessful()
    {
        await PatientFactory.CreateAsync(PatientManagementModule).ConfigureAwait(true);

        await GetEventually(
            new GetPatientDetailsFromPatientManagementProbe(
                PatientSampleData.PatientId,
                PatientManagementModule),
            15000
        ).ConfigureAwait(true);

        await PatientManagementModule.ExecuteCommandAsync(
            new AnonymizePatientCommand(
                PatientSampleData.PatientId,
                SystemClock.Now
            )
        ).ConfigureAwait(true);

        PatientDetailsDto? details = await GetEventually(
            new GetPatientDetailsFromPatientManagementProbe(
                PatientSampleData.PatientId,
                PatientManagementModule,
                dto => dto?.Status == "Anonymized"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("Anonymized");
        details.FullName.Should().Be("ANONYMIZED");
        details.DocumentId.Should().Be("ANONYMIZED");
        details.AnonymizedAt.Should().NotBeNull();
    }
}
