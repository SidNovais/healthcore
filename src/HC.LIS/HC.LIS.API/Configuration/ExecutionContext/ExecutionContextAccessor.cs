using System.Security.Claims;
using HC.Core.Application;

namespace HC.LIS.API.Configuration.ExecutionContext;

internal sealed class ExecutionContextAccessor(IHttpContextAccessor httpContextAccessor)
    : IExecutionContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            return claim is not null && Guid.TryParse(claim, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public string UserName =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public string CorrelationId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?
                .User.FindFirstValue("CorrelationId");

            return claim ?? Guid.NewGuid().ToString();
        }
    }

    public bool IsAvailable =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
