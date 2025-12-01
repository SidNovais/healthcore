using System.Data;

namespace HC.Core.Data;

public interface ISqlConnectionFactory
{
    IDbConnection GetConnection();
    IDbConnection CreateConnection();

}
