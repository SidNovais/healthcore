using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.SearchPhysicians;

public class SearchPhysiciansQuery(
    string? searchTerm,
    bool includeInactive
) : QueryBase<IReadOnlyCollection<PhysicianSearchResultDto>>
{
    public string? SearchTerm { get; } = searchTerm;
    public bool IncludeInactive { get; } = includeInactive;
}
