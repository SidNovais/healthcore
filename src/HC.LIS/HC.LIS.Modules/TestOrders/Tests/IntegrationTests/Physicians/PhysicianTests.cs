using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;
using HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;
using HC.LIS.Modules.TestOrders.Application.Physicians.ReactivatePhysician;
using HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;
using HC.LIS.Modules.TestOrders.Application.Physicians.SearchPhysicians;
using HC.LIS.Modules.TestOrders.Application.Physicians.UpdatePhysician;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Physicians;

public class PhysicianTests : TestBase
{
    public PhysicianTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task RegisterPhysicianIsSuccessful()
    {
        await PhysicianFactory.CreateAsync(TestOrdersModule).ConfigureAwait(true);

        PhysicianDetailsDto? details = await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Id.Should().Be(PhysicianSampleData.PhysicianId);
        details.FullName.Should().Be(PhysicianSampleData.FullName);
        details.LicenceNumber.Should().Be(PhysicianSampleData.LicenceNumber);
        details.Status.Should().Be("Active");
        details.RegisteredAt.Should().NotBe(default(DateTime));
        details.UpdatedAt.Should().BeNull();
        details.DeactivatedAt.Should().BeNull();
    }

    [Fact]
    public async Task RegisterPhysicianWritesAnOutboxMessage()
    {
        await PhysicianFactory.CreateAsync(TestOrdersModule).ConfigureAwait(true);

        PhysicianRegisteredNotification? notification =
            await GetLastOutboxMessage<PhysicianRegisteredNotification>().ConfigureAwait(true);

        notification.Should().NotBeNull();
        notification!.DomainEvent.PhysicianId.Should().Be(PhysicianSampleData.PhysicianId);
        notification.DomainEvent.FullName.Should().Be(PhysicianSampleData.FullName);
    }

    [Fact]
    public async Task UpdatePhysicianIsSuccessful()
    {
        await PhysicianFactory.CreateAsync(TestOrdersModule).ConfigureAwait(true);
        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule),
            15000
        ).ConfigureAwait(true);

        await TestOrdersModule.ExecuteCommandAsync(
            new UpdatePhysicianCommand(
                PhysicianSampleData.PhysicianId,
                PhysicianSampleData.UpdatedFullName,
                PhysicianSampleData.UpdatedLicenceNumber,
                SystemClock.Now
            )
        ).ConfigureAwait(true);

        PhysicianDetailsDto? details = await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule,
                dto => dto?.FullName == PhysicianSampleData.UpdatedFullName),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.FullName.Should().Be(PhysicianSampleData.UpdatedFullName);
        details.LicenceNumber.Should().Be(PhysicianSampleData.UpdatedLicenceNumber);
        details.Status.Should().Be("Active");
        details.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeactivatePhysicianIsSuccessful()
    {
        await PhysicianFactory.CreateAsync(TestOrdersModule).ConfigureAwait(true);
        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule),
            15000
        ).ConfigureAwait(true);

        await TestOrdersModule.ExecuteCommandAsync(
            new DeactivatePhysicianCommand(PhysicianSampleData.PhysicianId, SystemClock.Now)
        ).ConfigureAwait(true);

        PhysicianDetailsDto? details = await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule,
                dto => dto?.Status == "Inactive"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("Inactive");
        details.DeactivatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ReactivatePhysicianIsSuccessful()
    {
        await PhysicianFactory.CreateAsync(TestOrdersModule).ConfigureAwait(true);
        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule),
            15000
        ).ConfigureAwait(true);

        await TestOrdersModule.ExecuteCommandAsync(
            new DeactivatePhysicianCommand(PhysicianSampleData.PhysicianId, SystemClock.Now)
        ).ConfigureAwait(true);
        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule,
                dto => dto?.Status == "Inactive"),
            15000
        ).ConfigureAwait(true);

        await TestOrdersModule.ExecuteCommandAsync(
            new ReactivatePhysicianCommand(PhysicianSampleData.PhysicianId, SystemClock.Now)
        ).ConfigureAwait(true);

        PhysicianDetailsDto? details = await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule,
                dto => dto?.Status == "Active"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("Active");
        details.DeactivatedAt.Should().BeNull();
    }

    [Fact]
    public async Task SearchPhysiciansMatchesNamePrefixAndLicencePrefix()
    {
        await RegisterBothPhysiciansAsync().ConfigureAwait(true);

        IReadOnlyCollection<PhysicianSearchResultDto> byName = await TestOrdersModule
            .ExecuteQueryAsync(new SearchPhysiciansQuery("dr. ana", includeInactive: false))
            .ConfigureAwait(true);

        byName.Should().ContainSingle();
        byName.Single().Id.Should().Be(PhysicianSampleData.PhysicianId);

        IReadOnlyCollection<PhysicianSearchResultDto> byLicence = await TestOrdersModule
            .ExecuteQueryAsync(new SearchPhysiciansQuery("CRM-RJ", includeInactive: false))
            .ConfigureAwait(true);

        byLicence.Should().ContainSingle();
        byLicence.Single().Id.Should().Be(PhysicianSampleData.OtherPhysicianId);
    }

    [Fact]
    public async Task SearchPhysiciansDoesNotMatchMidNameTerms()
    {
        await RegisterBothPhysiciansAsync().ConfigureAwait(true);

        IReadOnlyCollection<PhysicianSearchResultDto> results = await TestOrdersModule
            .ExecuteQueryAsync(new SearchPhysiciansQuery("Lima", includeInactive: false))
            .ConfigureAwait(true);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPhysiciansWithoutTermListsEveryActivePhysician()
    {
        await RegisterBothPhysiciansAsync().ConfigureAwait(true);

        IReadOnlyCollection<PhysicianSearchResultDto> results = await TestOrdersModule
            .ExecuteQueryAsync(new SearchPhysiciansQuery(null, includeInactive: false))
            .ConfigureAwait(true);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchPhysiciansExcludesInactiveUnlessRequested()
    {
        await RegisterBothPhysiciansAsync().ConfigureAwait(true);

        await TestOrdersModule.ExecuteCommandAsync(
            new DeactivatePhysicianCommand(PhysicianSampleData.OtherPhysicianId, SystemClock.Now)
        ).ConfigureAwait(true);
        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.OtherPhysicianId,
                TestOrdersModule,
                dto => dto?.Status == "Inactive"),
            15000
        ).ConfigureAwait(true);

        IReadOnlyCollection<PhysicianSearchResultDto> activeOnly = await TestOrdersModule
            .ExecuteQueryAsync(new SearchPhysiciansQuery("Dr.", includeInactive: false))
            .ConfigureAwait(true);

        activeOnly.Should().ContainSingle();
        activeOnly.Single().Id.Should().Be(PhysicianSampleData.PhysicianId);

        IReadOnlyCollection<PhysicianSearchResultDto> all = await TestOrdersModule
            .ExecuteQueryAsync(new SearchPhysiciansQuery("Dr.", includeInactive: true))
            .ConfigureAwait(true);

        all.Should().HaveCount(2);
    }

    private async Task RegisterBothPhysiciansAsync()
    {
        await PhysicianFactory.CreateAsync(TestOrdersModule).ConfigureAwait(false);
        await PhysicianFactory.CreateAsync(
            TestOrdersModule,
            PhysicianSampleData.OtherPhysicianId,
            PhysicianSampleData.OtherFullName,
            PhysicianSampleData.OtherLicenceNumber
        ).ConfigureAwait(false);

        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.PhysicianId,
                TestOrdersModule),
            15000
        ).ConfigureAwait(false);
        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                PhysicianSampleData.OtherPhysicianId,
                TestOrdersModule),
            15000
        ).ConfigureAwait(false);
    }
}
