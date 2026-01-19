using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IPositionsService
{
    Task<Result<PositionDto>> CreatePositionAsync(CreatePositionRequest request);
    Task<Result<PositionDto>> UpdatePositionAsync(int positionId, UpdatePositionRequest request);
    Task<Result> DeletePositionAsync(int positionId);
}

[RegisterScoped]
public class PositionsService(IPaUnitOfWork unitOfWork) : IPositionsService
{
    public async Task<Result<PositionDto>> CreatePositionAsync(CreatePositionRequest request)
    {
        var position = await unitOfWork.Dictionaries.CreatePositionAsync(request);
        return position;
    }

    public async Task<Result<PositionDto>> UpdatePositionAsync(int positionId, UpdatePositionRequest request)
    {
        var existingPosition = await unitOfWork.Dictionaries.FindPositionByIdAsync(positionId);
        if (existingPosition == null)
        {
            return ServiceError.NotFound($"Position with id {positionId} not found");
        }

        var updatedPosition = await unitOfWork.Dictionaries.UpdatePositionAsync(positionId, request);
        if (updatedPosition == null)
        {
            return ServiceError.NotFound($"Position with id {positionId} not found");
        }

        return updatedPosition;
    }

    public async Task<Result> DeletePositionAsync(int positionId)
    {
        var position = await unitOfWork.Dictionaries.FindPositionByIdAsync(positionId);
        if (position == null)
        {
            return ServiceError.NotFound($"Position with id {positionId} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeletePositionAsync(positionId);
        if (!deleted)
        {
            return ServiceError.NotFound($"Position with id {positionId} not found");
        }

        return Result.Success;
    }
}