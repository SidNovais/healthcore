using System;
using Npgsql;
using System.Data;
using HC.Core.Infrastructure.Data;

namespace HC.Core.Infastructure;

public class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory, IDisposable
{
    private string _connectionString { get; } = connectionString;
    private NpgsqlConnection? _connection;
    #pragma warning disable CA1805
    private bool _disposed = false;
    #pragma warning restore CA1805

    public IDbConnection CreateConnection()
    {
        _connection = new NpgsqlConnection(_connectionString);
        _connection.Open();
        return _connection;
    }

    public IDbConnection? GetConnection()
    {
        if (_connection is null || _connection.State != ConnectionState.Open)
            CreateConnection();
        return _connection;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _connection is not null && _connection.State == ConnectionState.Open)
                _connection.Dispose();
            _disposed = true;
        }
    }
    ~SqlConnectionFactory()
    {
        Dispose(false);
    }
}
