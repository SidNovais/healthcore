using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.SearchPhysicians;

// One query serves both the order-form typeahead (active only) and the ITAdmin
// registry page (which also lists inactive physicians).
public class SearchPhysiciansQuery(
    string? searchTerm,
    bool includeInactive
) : QueryBase<IReadOnlyCollection<PhysicianSearchResultDto>>
{
    public string? SearchTerm { get; } = searchTerm;
    public bool IncludeInactive { get; } = includeInactive;
}
