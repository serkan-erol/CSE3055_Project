using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class TreasuryRepository : ITreasuryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TreasuryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Get all treasury entries
    /// </summary>
    public async Task<IReadOnlyList<TreasuryResponseDto>> GetAllTreasuryEntriesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Treasury.GetAllTreasuryEntries;

        var entries = await connection.QueryAsync<TreasuryResponseDto>(
            new CommandDefinition(query, cancellationToken: cancellationToken));
        return entries.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get treasury entry by ID
    /// </summary>
    public async Task<TreasuryResponseDto> GetTreasuryEntryByIdAsync(int treasuryId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Treasury.GetTreasuryEntryById;

        var entry = await connection.QuerySingleOrDefaultAsync<TreasuryResponseDto>(
            new CommandDefinition(query, new { TreasuryID = treasuryId }, cancellationToken: cancellationToken));
        return entry ?? throw new InvalidOperationException("Treasury entry not found");
    }

    /// <summary>
    /// Get current treasury balance
    /// </summary>
    public async Task<TreasuryBalanceDto> GetCurrentBalanceAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var balance = await connection.QuerySingleOrDefaultAsync<TreasuryBalanceDto>(
            new CommandDefinition(SqlQueries.Treasury.GetCurrentBalance, cancellationToken: cancellationToken));

        return balance ?? new TreasuryBalanceDto 
        { 
            CurrentBalance = 0, 
            LastUpdated = DateTimeOffset.UtcNow,
            TotalEntries = 0
        };
    }

    /// <summary>
    /// Get treasury entries by financial transaction ID
    /// </summary>
    public async Task<IReadOnlyList<TreasuryResponseDto>> GetTreasuryEntriesByTransactionIdAsync(int fTransactionId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Treasury.GetTreasuryEntriesByTransactionId;

        var entries = await connection.QueryAsync<TreasuryResponseDto>(
            new CommandDefinition(query, new { FTransactionID = fTransactionId }, cancellationToken: cancellationToken));
        return entries.ToList().AsReadOnly();
    }
}