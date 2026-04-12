using HC.LIS.API.Common;
using HC.LIS.API.Modules.TestOrders.Orders.AcceptExam;
using HC.LIS.API.Modules.TestOrders.Orders.CancelExam;
using HC.LIS.API.Modules.TestOrders.Orders.CreateOrder;
using HC.LIS.API.Modules.TestOrders.Orders.GetOrderDetails;
using HC.LIS.API.Modules.TestOrders.Orders.GetOrderItemDetails;
using HC.LIS.API.Modules.TestOrders.Orders.PartiallyCompleteExam;
using HC.LIS.API.Modules.TestOrders.Orders.PlaceExamOnHold;
using HC.LIS.API.Modules.TestOrders.Orders.RejectExam;
using HC.LIS.API.Modules.TestOrders.Orders.RequestExam;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

namespace HC.LIS.API.Modules.TestOrders.Orders;

internal static class OrdersEndpoints
{
    internal static RouteGroupBuilder MapOrdersEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("Orders");

        group.MapPost("", CreateOrderEndpoint.Handle)
            .WithName("CreateOrder")
            .WithSummary("Create a new test order.")
            .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("{orderId:guid}", GetOrderDetailsEndpoint.Handle)
            .WithName("GetOrderDetails")
            .WithSummary("Get order details by ID.")
            .Produces<OrderDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("{orderId:guid}/exams", RequestExamEndpoint.Handle)
            .WithName("RequestExam")
            .WithSummary("Request a new exam on an existing order.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("exams/{itemId:guid}", GetOrderItemDetailsEndpoint.Handle)
            .WithName("GetOrderItemDetails")
            .WithSummary("Get order item details by ID.")
            .Produces<OrderItemDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("{orderId:guid}/exams/{itemId:guid}/accept", AcceptExamEndpoint.Handle)
            .WithName("AcceptExam")
            .WithSummary("Accept an exam on an order.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{orderId:guid}/exams/{itemId:guid}/cancel", CancelExamEndpoint.Handle)
            .WithName("CancelExam")
            .WithSummary("Cancel an exam on an order.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{orderId:guid}/exams/{itemId:guid}/partially-complete", PartiallyCompleteExamEndpoint.Handle)
            .WithName("PartiallyCompleteExam")
            .WithSummary("Partially complete an exam on an order.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{orderId:guid}/exams/{itemId:guid}/place-on-hold", PlaceExamOnHoldEndpoint.Handle)
            .WithName("PlaceExamOnHold")
            .WithSummary("Place an exam on hold.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("{orderId:guid}/exams/{itemId:guid}/reject", RejectExamEndpoint.Handle)
            .WithName("RejectExam")
            .WithSummary("Reject an exam on an order.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
