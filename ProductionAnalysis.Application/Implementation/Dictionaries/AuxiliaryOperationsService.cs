using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IAuxiliaryOperationsService
{
    Task<Result<AuxiliaryOperationDto>> CreateAuxiliaryOperationAsync(CreateAuxiliaryOperationRequest request);
    Task<Result<AuxiliaryOperationDto>> UpdateAuxiliaryOperationAsync(int id, UpdateAuxiliaryOperationRequest request);
    Task<Result> DeleteAuxiliaryOperationAsync(int id);
}

[RegisterScoped]
public class AuxiliaryOperationsService(IPaUnitOfWork unitOfWork) : IAuxiliaryOperationsService
{
    public async Task<Result<AuxiliaryOperationDto>> CreateAuxiliaryOperationAsync(
        CreateAuxiliaryOperationRequest request)
    {
        var auxiliaryOperation = await unitOfWork.Dictionaries.CreateAuxiliaryOperationAsync(request);
        return auxiliaryOperation;
    }

    public async Task<Result<AuxiliaryOperationDto>> UpdateAuxiliaryOperationAsync(int id,
        UpdateAuxiliaryOperationRequest request)
    {
        var existing = await unitOfWork.Dictionaries.FindAuxiliaryOperationByIdAsync(id);
        if (existing == null)
        {
            return ServiceError.NotFound($"AuxiliaryOperation with id {id} not found");
        }

        var updated = await unitOfWork.Dictionaries.UpdateAuxiliaryOperationAsync(id, request);
        if (updated == null)
        {
            return ServiceError.NotFound($"AuxiliaryOperation with id {id} not found");
        }

        return updated;
    }

    public async Task<Result> DeleteAuxiliaryOperationAsync(int id)
    {
        var auxiliaryOperation = await unitOfWork.Dictionaries.FindAuxiliaryOperationByIdAsync(id);
        if (auxiliaryOperation == null)
        {
            return ServiceError.NotFound($"AuxiliaryOperation with id {id} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteAuxiliaryOperationAsync(id);
        if (!deleted)
        {
            return ServiceError.NotFound($"AuxiliaryOperation with id {id} not found");
        }

        return Result.Success;
    }
}