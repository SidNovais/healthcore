namespace HC.LIS.API.Configuration.RealTime;

/// <summary>
/// The real-time topics a browser can subscribe to and the role → topics mapping used to
/// scope each SSE connection. Mirrors the SPA route guards (orders: Receptionist/Physician/ITAdmin;
/// triage: LabTechnician/ITAdmin; worklist: Physician/ITAdmin).
/// </summary>
internal static class UiTopics
{
    internal const string Orders = "orders";
    internal const string Triage = "triage";
    internal const string Worklist = "worklist";

    private static readonly Dictionary<string, string[]> s_topicsByRole =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ITAdmin"] = [Orders, Triage, Worklist],
            ["Receptionist"] = [Orders],
            ["Physician"] = [Orders, Worklist],
            ["LabTechnician"] = [Triage],
        };

    /// <summary>The topics the given role is allowed to receive; empty if the role sees none.</summary>
    internal static IReadOnlyCollection<string> ForRole(string role) =>
        s_topicsByRole.TryGetValue(role, out string[]? topics) ? topics : [];
}
