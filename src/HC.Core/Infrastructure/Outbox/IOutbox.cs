using System.Threading.Tasks;

namespace HC.Core.Infrastructure.Outbox;

public interface IOutbox
{
    void Add(OutboxMessage message);
    Task Save();
}
