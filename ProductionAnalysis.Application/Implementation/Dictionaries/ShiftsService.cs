using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IShiftsService
{
    Task<Result<ShiftDto>> CreateShiftAsync(CreateShiftRequest request);
    Task<Result<ShiftDto>> UpdateShiftAsync(int shiftId, UpdateShiftRequest request);
    Task<Result> DeleteShiftAsync(int shiftId);
}

[RegisterScoped]
public class ShiftsService(IPaUnitOfWork unitOfWork) : IShiftsService
{
    public async Task<Result<ShiftDto>> CreateShiftAsync(CreateShiftRequest request)
    {
        var shift = await unitOfWork.Dictionaries.CreateShiftAsync(request);
        return shift;
    }

    public async Task<Result<ShiftDto>> UpdateShiftAsync(int shiftId, UpdateShiftRequest request)
    {
        var existingShift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(shiftId);
        if (existingShift == null)
        {
            return ServiceError.NotFound($"Shift with id {shiftId} not found");
        }

        var updatedShift = await unitOfWork.Dictionaries.UpdateShiftAsync(shiftId, request);
        if (updatedShift == null)
        {
            return ServiceError.NotFound($"Shift with id {shiftId} not found");
        }

        return updatedShift;
    }

    public async Task<Result> DeleteShiftAsync(int shiftId)
    {
        var shift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(shiftId);
        if (shift == null)
        {
            return ServiceError.NotFound($"Shift with id {shiftId} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteShiftAsync(shiftId);
        if (!deleted)
        {
            return ServiceError.NotFound($"Shift with id {shiftId} not found");
        }

        return Result.Success;
    }
}