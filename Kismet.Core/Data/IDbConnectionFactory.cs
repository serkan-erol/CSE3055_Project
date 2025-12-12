using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Kismet.Core.Data;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}

