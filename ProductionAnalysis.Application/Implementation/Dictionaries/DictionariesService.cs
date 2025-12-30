using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IDictionariesService
{
    Task<ICollection<DepartmentDto>> GetDepartmentsAsync();
    Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync();
    Task<ICollection<EmployeeDto>> GetEmployeesAsync();
    Task<ICollection<EnterpriseDto>> GetEnterprisesAsync();
    Task<ICollection<AdditionalOperationDto>> GetAdditionalOperationsAsync();
    Task<ICollection<OperationDto>> GetOperationsAsync();
    Task<ICollection<PaTypeDto>> GetPaTypesAsync();
    Task<ICollection<ProductDto>> GetProductsAsync();
    Task<ICollection<ShiftDto>> GetShiftsAsync();
}

[RegisterScoped]
public class DictionariesService(IPaUnitOfWork unitOfWork) : IDictionariesService
{
    public async Task<ICollection<DepartmentDto>> GetDepartmentsAsync()
    {
        return await unitOfWork.Dictionaries.SelectDepartmentsAsync();
    }

    public async Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync()
    {
        return await unitOfWork.Dictionaries.SelectDowntimeReasonGroupsAsync();
    }

    public async Task<ICollection<EmployeeDto>> GetEmployeesAsync()
    {
        return await unitOfWork.Dictionaries.SelectEmployeesAsync();
    }

    public async Task<ICollection<EnterpriseDto>> GetEnterprisesAsync()
    {
        return await unitOfWork.Dictionaries.SelectEnterprisesAsync();
    }

    public async Task<ICollection<AdditionalOperationDto>> GetAdditionalOperationsAsync()
    {
        return await unitOfWork.Dictionaries.SelectAdditionalOperationsAsync();
    }

    public async Task<ICollection<OperationDto>> GetOperationsAsync()
    {
        return await unitOfWork.Dictionaries.SelectOperationsAsync();
    }

    public async Task<ICollection<PaTypeDto>> GetPaTypesAsync()
    {
        return await unitOfWork.Dictionaries.SelectPaTypesAsync();
    }

    public async Task<ICollection<ProductDto>> GetProductsAsync()
    {
        return await unitOfWork.Dictionaries.SelectProductsAsync();
    }

    public async Task<ICollection<ShiftDto>> GetShiftsAsync()
    {
        return await unitOfWork.Dictionaries.SelectShiftsAsync();
    }
}