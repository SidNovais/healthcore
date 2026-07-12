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

    // Returns a fresh, independent connection that the CALLER owns and must dispose
    // (typically via `using`). Used where a separate physical connection is required —
    // e.g. projectors running while Marten holds the scope-managed connection for an
    // event write. Must NOT touch _connection, or it would orphan the scope-managed
    // connection and leak it out of the pool.
    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    // Returns the scope-managed connection, opening it on first use. Owned by this
    // factory and released when the lifetime scope disposes it (see Dispose).
    public IDbConnection? GetConnection()
    {
        if (_connection is null || _connection.State != ConnectionState.Open)
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
        }
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
