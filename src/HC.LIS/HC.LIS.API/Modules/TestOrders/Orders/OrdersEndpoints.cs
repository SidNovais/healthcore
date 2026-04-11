namespace HC.LIS.API.Modules.TestOrders.Orders;

internal static class OrdersEndpoints
{
    internal static RouteGroupBuilder MapOrdersEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("Orders");

        // TODO: add endpoint registrations using /create-api add
        // Example:
        // group.MapGet("{id:guid}", GetOrderEndpoint.Handle)
        //     .WithName("GetOrder")
        //     .WithSummary("Get an order by ID.")
        //     .Produces<OrderDto>()
        //     .ProducesProblem(401)
        //     .ProducesProblem(404);

        return group;
    }
}
