using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Core.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Kismet.Infrastructure.Data;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("KismetDatabase")
            ?? throw new InvalidOperationException("Connection string 'KismetDatabase' is missing.");
    }

    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        DbConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}

