using System;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.PatientManagement.Domain.Patients;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Rules;

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

    [Fact]
    public void UpdatePatientIsSuccessful()
    {
        DateTime updatedAt = SystemClock.Now;
        string newFullName = "Jane Doe";
        DateTime newDateOfBirth = new DateTime(1985, 6, 20, 0, 0, 0, DateTimeKind.Utc);

        _sut.Update(newFullName, newDateOfBirth, null, null, null, null, null, updatedAt);

        PatientUpdatedDomainEvent @event = AssertPublishedDomainEvent<PatientUpdatedDomainEvent>(_sut);
        @event.PatientId.Should().Be(PatientSampleData.PatientId);
        @event.FullName.Should().Be(newFullName);
        @event.DateOfBirth.Should().Be(newDateOfBirth);
        @event.Gender.Should().BeNull();
        @event.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void AnonymizePatientIsSuccessful()
    {
        DateTime anonymizedAt = SystemClock.Now;

        _sut.Anonymize(anonymizedAt);

        PatientAnonymizedDomainEvent @event = AssertPublishedDomainEvent<PatientAnonymizedDomainEvent>(_sut);
        @event.PatientId.Should().Be(PatientSampleData.PatientId);
        @event.AnonymizedAt.Should().Be(anonymizedAt);
    }

    [Fact]
    public void AnonymizePatientThrowsWhenAlreadyAnonymized()
    {
        _sut.Anonymize(SystemClock.Now);

        void action() => _sut.Anonymize(SystemClock.Now);

        AssertBrokenRule<CannotAnonymizeAlreadyAnonymizedPatientRule>(action);
    }

    [Fact]
    public void UpdatePatientThrowsWhenAnonymized()
    {
        _sut.Anonymize(SystemClock.Now);

        void action() => _sut.Update(
            PatientSampleData.FullName,
            PatientSampleData.DateOfBirth,
            null, null, null, null, null,
            SystemClock.Now
        );

        AssertBrokenRule<CannotUpdateAnonymizedPatientRule>(action);
    }
}
