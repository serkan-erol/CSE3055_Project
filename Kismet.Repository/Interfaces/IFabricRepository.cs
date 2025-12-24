using Kismet.Entities.DTOs;

namespace Kismet.Repository.Interfaces;

public interface IFabricRepository
{
    Task<IReadOnlyList<FabricResponseDto>> GetAllFabricsAsync(CancellationToken cancellationToken = default);
    Task<FabricResponseDto> GetFabricByIdAsync(int fabricId, CancellationToken cancellationToken = default);
    Task<FabricResponseDto> CreateFabricAsync(CreateFabricDto dto, CancellationToken cancellationToken = default);
    Task<FabricResponseDto> UpdateFabricStockAsync(UpdateFabricStockDto dto, CancellationToken cancellationToken = default);
}