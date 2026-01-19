using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IDowntimeReasonGroupsService
{
    Task<Result<DowntimeReasonGroupDto>> CreateDowntimeReasonGroupAsync(CreateDowntimeReasonGroupRequest request);

    Task<Result<DowntimeReasonGroupDto>> UpdateDowntimeReasonGroupAsync(int id,
        UpdateDowntimeReasonGroupRequest request);

    Task<Result> DeleteDowntimeReasonGroupAsync(int id);
}

[RegisterScoped]
public class DowntimeReasonGroupsService(IPaUnitOfWork unitOfWork) : IDowntimeReasonGroupsService
{
    public async Task<Result<DowntimeReasonGroupDto>> CreateDowntimeReasonGroupAsync(
        CreateDowntimeReasonGroupRequest request)
    {
        var group = await unitOfWork.Dictionaries.CreateDowntimeReasonGroupAsync(request);
        return group;
    }

    public async Task<Result<DowntimeReasonGroupDto>> UpdateDowntimeReasonGroupAsync(int id,
        UpdateDowntimeReasonGroupRequest request)
    {
        var existing = await unitOfWork.Dictionaries.FindDowntimeReasonGroupByIdAsync(id);
        if (existing == null)
        {
            return ServiceError.NotFound($"DowntimeReasonGroup with id {id} not found");
        }

        var updated = await unitOfWork.Dictionaries.UpdateDowntimeReasonGroupAsync(id, request);
        if (updated == null)
        {
            return ServiceError.NotFound($"DowntimeReasonGroup with id {id} not found");
        }

        return updated;
    }

    public async Task<Result> DeleteDowntimeReasonGroupAsync(int id)
    {
        var group = await unitOfWork.Dictionaries.FindDowntimeReasonGroupByIdAsync(id);
        if (group == null)
        {
            return ServiceError.NotFound($"DowntimeReasonGroup with id {id} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteDowntimeReasonGroupAsync(id);
        if (!deleted)
        {
            return ServiceError.NotFound($"DowntimeReasonGroup with id {id} not found");
        }

        return Result.Success;
    }
}