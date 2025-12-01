using System.Data;

namespace HC.Core.Infrastructure.Data;

public interface ISqlConnectionFactory
{
    IDbConnection? GetConnection();
    IDbConnection CreateConnection();

}
