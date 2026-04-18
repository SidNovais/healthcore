namespace HC.Core.Application.Queries;

public interface IPagedQuery
{
    int? Page { get; }
    int? PerPage { get; }
}
