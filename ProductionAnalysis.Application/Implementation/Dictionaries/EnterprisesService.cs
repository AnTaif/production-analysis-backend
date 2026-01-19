using Core.Results;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IEnterprisesService
{
    Task<Result<EnterpriseDto>> CreateEnterpriseAsync(CreateEnterpriseRequest request);
    Task<Result<EnterpriseDto>> UpdateEnterpriseAsync(int enterpriseId, UpdateEnterpriseRequest request);
    Task<Result> DeleteEnterpriseAsync(int enterpriseId);
}

[RegisterScoped]
public class EnterprisesService(IPaUnitOfWork unitOfWork) : IEnterprisesService
{
    public async Task<Result<EnterpriseDto>> CreateEnterpriseAsync(CreateEnterpriseRequest request)
    {
        var enterprise = await unitOfWork.Dictionaries.CreateEnterpriseAsync(request);
        return enterprise;
    }

    public async Task<Result<EnterpriseDto>> UpdateEnterpriseAsync(int enterpriseId, UpdateEnterpriseRequest request)
    {
        var existingEnterprise = await unitOfWork.Dictionaries.FindEnterpriseByIdAsync(enterpriseId);
        if (existingEnterprise == null)
        {
            return ServiceError.NotFound($"Enterprise with id {enterpriseId} not found");
        }

        var updatedEnterprise = await unitOfWork.Dictionaries.UpdateEnterpriseAsync(enterpriseId, request);
        if (updatedEnterprise == null)
        {
            return ServiceError.NotFound($"Enterprise with id {enterpriseId} not found");
        }

        return updatedEnterprise;
    }

    public async Task<Result> DeleteEnterpriseAsync(int enterpriseId)
    {
        var enterprise = await unitOfWork.Dictionaries.FindEnterpriseByIdAsync(enterpriseId);
        if (enterprise == null)
        {
            return ServiceError.NotFound($"Enterprise with id {enterpriseId} not found");
        }

        var deleted = await unitOfWork.Dictionaries.DeleteEnterpriseAsync(enterpriseId);
        if (!deleted)
        {
            return ServiceError.NotFound($"Enterprise with id {enterpriseId} not found");
        }

        return Result.Success;
    }
}