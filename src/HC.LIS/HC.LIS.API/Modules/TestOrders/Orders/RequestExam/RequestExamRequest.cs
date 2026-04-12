namespace HC.LIS.API.Modules.TestOrders.Orders.RequestExam;

internal sealed record RequestExamRequest(
    Guid ItemId,
    string ExamMnemonic,
    string SpecimenMnemonic,
    string MaterialType,
    string ContainerType,
    string Additive,
    string ProcessingType,
    string StorageCondition);
