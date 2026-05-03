using HC.LIS.API.Common;
using HC.LIS.API.Modules.UserAccess.Users.ActivateUser;
using HC.LIS.API.Modules.UserAccess.Users.ChangeRole;
using HC.LIS.API.Modules.UserAccess.Users.CreateUser;
using HC.LIS.API.Modules.UserAccess.Users.GetUserDetails;
using HC.LIS.API.Modules.UserAccess.Users.GetUserList;
using HC.LIS.Modules.UserAccess.Application.Users.GetUserDetails;
using HC.LIS.Modules.UserAccess.Application.Users.GetUserList;

namespace HC.LIS.API.Modules.UserAccess.Users;

internal static class UsersEndpoints
{
    internal static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group)
    {
        group.WithTags("Users");

        group.MapPost("", CreateUserEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("CreateUser")
            .WithSummary("Create a new user.")
            .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("{userId:guid}/role", ChangeRoleEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("ChangeRole")
            .WithSummary("Change a user's role.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("{userId:guid}", GetUserDetailsEndpoint.Handle)
            .WithName("GetUserDetails")
            .WithSummary("Get user details by ID.")
            .Produces<UserDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("", GetUserListEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("GetUserList")
            .WithSummary("Get paginated list of users.")
            .Produces<IReadOnlyCollection<UserListItemDto>>();

        group.MapPost("{userId:guid}/activate", ActivateUserEndpoint.Handle)
            .AllowAnonymous()
            .WithName("ActivateUser")
            .WithSummary("Activate a user account using an invitation token.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
