using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IDictionariesService
{
    Task<ICollection<DepartmentDto>> GetDepartmentsAsync();
    Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync();
    Task<ICollection<EmployeeDto>> GetEmployeesAsync();
    Task<ICollection<EnterpriseDto>> GetEnterpriseAsync();
    Task<ICollection<AdditionalOperationDto>> GetAdditionalOperationsAsync();
    Task<ICollection<OperationDto>> GetOperationsAsync();
    Task<ICollection<PaTypeDto>> GetPaTypesAsync();
    Task<ICollection<ProductDto>> GetProductsAsync();
    Task<ICollection<ShiftDto>> GetShiftsAsync();
}

[RegisterScoped]
public class DictionariesService(IDictionariesRepository dictionariesRepository) : IDictionariesService
{
    public async Task<ICollection<DepartmentDto>> GetDepartmentsAsync()
    {
        return await dictionariesRepository.SelectDepartmentsAsync();
    }

    public async Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync()
    {
        return await dictionariesRepository.SelectDowntimeReasonGroupsAsync();
    }

    public async Task<ICollection<EmployeeDto>> GetEmployeesAsync()
    {
        return await dictionariesRepository.SelectEmployeesAsync();
    }

    public async Task<ICollection<EnterpriseDto>> GetEnterpriseAsync()
    {
        return await dictionariesRepository.SelectEnterprisesAsync();
    }

    public async Task<ICollection<AdditionalOperationDto>> GetAdditionalOperationsAsync()
    {
        return await dictionariesRepository.SelectAdditionalOperationsAsync();
    }

    public async Task<ICollection<OperationDto>> GetOperationsAsync()
    {
        return await dictionariesRepository.SelectOperationsAsync();
    }

    public async Task<ICollection<PaTypeDto>> GetPaTypesAsync()
    {
        return await dictionariesRepository.SelectPaTypesAsync();
    }

    public async Task<ICollection<ProductDto>> GetProductsAsync()
    {
        return await dictionariesRepository.SelectProductsAsync();
    }

    public async Task<ICollection<ShiftDto>> GetShiftsAsync()
    {
        return await dictionariesRepository.SelectShiftsAsync();
    }
}