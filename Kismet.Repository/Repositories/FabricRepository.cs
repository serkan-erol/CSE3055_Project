using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;
using System.Text.Json;

namespace Kismet.Repository.Repositories;

public class FabricRepository : IFabricRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FabricRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<FabricResponseDto>> GetAllFabricsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var fabrics = await connection.QueryAsync<FabricResponseDto>(
            new CommandDefinition(SqlQueries.Fabric.GetAllFabrics, cancellationToken: cancellationToken));
        return fabrics.ToList().AsReadOnly();
    }

    public async Task<FabricResponseDto> GetFabricByIdAsync(int fabricId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var fabric = await connection.QuerySingleOrDefaultAsync<FabricResponseDto>(
            new CommandDefinition(
                SqlQueries.Fabric.GetFabricById, 
                new { FabricID = fabricId }, 
                cancellationToken: cancellationToken));
        return fabric ?? throw new InvalidOperationException("Fabric not found");
    }

    public async Task<FabricResponseDto> CreateFabricAsync(CreateFabricDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var fabricId = await connection.QuerySingleAsync<int>(
            new CommandDefinition(SqlQueries.Fabric.InsertFabric, dto, cancellationToken: cancellationToken));

        return await GetFabricByIdAsync(fabricId, cancellationToken);
    }

    public async Task<FabricResponseDto> UpdateFabricStockAsync(UpdateFabricStockDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(SqlQueries.Fabric.UpdateFabricStock, dto, cancellationToken: cancellationToken));

        return await GetFabricByIdAsync(dto.FabricID, cancellationToken);
    }
}
