using System;
using System.Collections.Generic;
using FluentAssertions;
using HC.Core.Domain;
using HC.Core.UnitTests;
using HC.LIS.Modules.TestOrders.Domain.Physicians;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Rules;

namespace HC.LIS.Modules.TestOrders.UnitTests.Physicians;

public class PhysicianTests : TestBase
{
    readonly Physician _sut;

    public PhysicianTests()
    {
        _sut = PhysicianFactory.Create();
    }

    [Fact]
    public void RegisterPhysicianIsSuccessful()
    {
        PhysicianRegisteredDomainEvent @event = AssertPublishedDomainEvent<PhysicianRegisteredDomainEvent>(_sut);
        @event.PhysicianId.Should().Be(PhysicianSampleData.PhysicianId);
        @event.FullName.Should().Be(PhysicianSampleData.FullName);
        @event.LicenceNumber.Should().Be(PhysicianSampleData.LicenceNumber);
        @event.RegisteredAt.Should().Be(PhysicianSampleData.RegisteredAt);
    }

    [Fact]
    public void RegisterPhysicianWithoutLicenceNumberIsSuccessful()
    {
        Physician physician = Physician.Register(
            Guid.CreateVersion7(),
            PhysicianSampleData.FullName,
            null,
            PhysicianSampleData.RegisteredAt
        );

        PhysicianRegisteredDomainEvent @event = AssertPublishedDomainEvent<PhysicianRegisteredDomainEvent>(physician);
        @event.LicenceNumber.Should().BeNull();
    }

    [Fact]
    public void RegisterPhysicianThrowsWhenFullNameIsEmpty()
    {
        void action() => Physician.Register(
            Guid.CreateVersion7(),
            string.Empty,
            PhysicianSampleData.LicenceNumber,
            PhysicianSampleData.RegisteredAt
        );

        AssertBrokenRule<PhysicianMustHaveFullNameRule>(action);
    }

    [Fact]
    public void RegisterPhysicianThrowsWhenFullNameIsWhitespace()
    {
        void action() => Physician.Register(
            Guid.CreateVersion7(),
            "   ",
            PhysicianSampleData.LicenceNumber,
            PhysicianSampleData.RegisteredAt
        );

        AssertBrokenRule<PhysicianMustHaveFullNameRule>(action);
    }

    [Fact]
    public void UpdatePhysicianIsSuccessful()
    {
        DateTime updatedAt = SystemClock.Now;

        _sut.Update(PhysicianSampleData.UpdatedFullName, PhysicianSampleData.UpdatedLicenceNumber, updatedAt);

        PhysicianUpdatedDomainEvent @event = AssertPublishedDomainEvent<PhysicianUpdatedDomainEvent>(_sut);
        @event.PhysicianId.Should().Be(PhysicianSampleData.PhysicianId);
        @event.FullName.Should().Be(PhysicianSampleData.UpdatedFullName);
        @event.LicenceNumber.Should().Be(PhysicianSampleData.UpdatedLicenceNumber);
        @event.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdatePhysicianThrowsWhenFullNameIsEmpty()
    {
        void action() => _sut.Update(string.Empty, PhysicianSampleData.UpdatedLicenceNumber, SystemClock.Now);

        AssertBrokenRule<PhysicianMustHaveFullNameRule>(action);
    }

    [Fact]
    public void UpdatePhysicianThrowsWhenInactive()
    {
        _sut.Deactivate(SystemClock.Now);

        void action() => _sut.Update(
            PhysicianSampleData.UpdatedFullName,
            PhysicianSampleData.UpdatedLicenceNumber,
            SystemClock.Now
        );

        AssertBrokenRule<CannotUpdateInactivePhysicianRule>(action);
    }

    [Fact]
    public void DeactivatePhysicianIsSuccessful()
    {
        DateTime deactivatedAt = SystemClock.Now;

        _sut.Deactivate(deactivatedAt);

        PhysicianDeactivatedDomainEvent @event = AssertPublishedDomainEvent<PhysicianDeactivatedDomainEvent>(_sut);
        @event.PhysicianId.Should().Be(PhysicianSampleData.PhysicianId);
        @event.DeactivatedAt.Should().Be(deactivatedAt);
    }

    [Fact]
    public void DeactivatePhysicianThrowsWhenAlreadyInactive()
    {
        _sut.Deactivate(SystemClock.Now);

        void action() => _sut.Deactivate(SystemClock.Now);

        AssertBrokenRule<CannotDeactivateInactivePhysicianRule>(action);
    }

    [Fact]
    public void ReactivatePhysicianIsSuccessful()
    {
        _sut.Deactivate(SystemClock.Now);
        DateTime reactivatedAt = SystemClock.Now;

        _sut.Reactivate(reactivatedAt);

        PhysicianReactivatedDomainEvent @event = AssertPublishedDomainEvent<PhysicianReactivatedDomainEvent>(_sut);
        @event.PhysicianId.Should().Be(PhysicianSampleData.PhysicianId);
        @event.ReactivatedAt.Should().Be(reactivatedAt);
    }

    [Fact]
    public void ReactivatePhysicianThrowsWhenAlreadyActive()
    {
        void action() => _sut.Reactivate(SystemClock.Now);

        AssertBrokenRule<CannotReactivateActivePhysicianRule>(action);
    }

    [Fact]
    public void PhysicianRebuildsStateFromEventHistory()
    {
        _sut.Deactivate(SystemClock.Now);
        IReadOnlyCollection<IDomainEvent> history = _sut.GetDomainEvents();

        var replayed = (Physician)Activator.CreateInstance(typeof(Physician), nonPublic: true)!;
        replayed.Load(history);

        replayed.Id.Should().Be(PhysicianSampleData.PhysicianId);
        void action() => replayed.Update(
            PhysicianSampleData.UpdatedFullName,
            PhysicianSampleData.UpdatedLicenceNumber,
            SystemClock.Now
        );
        AssertBrokenRule<CannotUpdateInactivePhysicianRule>(action);
    }
}
