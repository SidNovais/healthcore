using HC.LIS.API.Common;
using HC.LIS.API.Modules.TestOrders.Physicians.DeactivatePhysician;
using HC.LIS.API.Modules.TestOrders.Physicians.GetPhysicianDetails;
using HC.LIS.API.Modules.TestOrders.Physicians.ReactivatePhysician;
using HC.LIS.API.Modules.TestOrders.Physicians.RegisterPhysician;
using HC.LIS.API.Modules.TestOrders.Physicians.SearchPhysicians;
using HC.LIS.API.Modules.TestOrders.Physicians.UpdatePhysician;
using HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;
using HC.LIS.Modules.TestOrders.Application.Physicians.SearchPhysicians;

namespace HC.LIS.API.Modules.TestOrders.Physicians;

internal static class PhysiciansEndpoints
{
    internal static RouteGroupBuilder MapPhysiciansEndpoints(this RouteGroupBuilder group)
    {
        group.WithTags("Physicians");

        group.MapGet("", SearchPhysiciansEndpoint.Handle)
            .RequireAuthorization("OrderEntry")
            .WithName("SearchPhysicians")
            .WithSummary("Search referring physicians by name or licence number.")
            .Produces<IReadOnlyCollection<PhysicianSearchResultDto>>();

        group.MapPost("", RegisterPhysicianEndpoint.Handle)
            .RequireAuthorization("OrderEntry")
            .WithName("RegisterPhysician")
            .WithSummary("Register a new referring physician.")
            .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("{id:guid}", GetPhysicianDetailsEndpoint.Handle)
            .RequireAuthorization("OrderEntry")
            .WithName("GetPhysicianDetails")
            .WithSummary("Get referring physician details by ID.")
            .Produces<PhysicianDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("{id:guid}", UpdatePhysicianEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("UpdatePhysician")
            .WithSummary("Update a referring physician.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/deactivate", DeactivatePhysicianEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("DeactivatePhysician")
            .WithSummary("Deactivate a referring physician.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{id:guid}/reactivate", ReactivatePhysicianEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("ReactivatePhysician")
            .WithSummary("Reactivate a deactivated referring physician.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
