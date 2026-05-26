using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.PatientManagement.Domain.Patients;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.UnitTests.Patients;

public class PatientTests : TestBase
{
    readonly Patient _sut;

    public PatientTests()
    {
        _sut = PatientFactory.Create();
    }

    [Fact]
    public void RegisterPatientIsSuccessful()
    {
        PatientRegisteredDomainEvent @event = AssertPublishedDomainEvent<PatientRegisteredDomainEvent>(_sut);
        @event.PatientId.Should().Be(PatientSampleData.PatientId);
        @event.FullName.Should().Be(PatientSampleData.FullName);
        @event.DateOfBirth.Should().Be(PatientSampleData.DateOfBirth);
        @event.Gender.Should().Be(PatientSampleData.Gender);
        @event.MothersFullName.Should().Be(PatientSampleData.MothersFullName);
        @event.DocumentId.Should().Be(PatientSampleData.DocumentId);
        @event.Phone.Should().Be(PatientSampleData.Phone);
        @event.Email.Should().Be(PatientSampleData.Email);
        @event.RegisteredAt.Should().Be(PatientSampleData.RegisteredAt);
    }
}
