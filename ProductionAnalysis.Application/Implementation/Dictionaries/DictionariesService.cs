using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Repositories;

namespace ProductionAnalysis.Application.Implementation.Dictionaries;

public interface IDictionariesService
{
    Task<ICollection<DepartmentDto>> GetDepartmentsAsync();
    Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync();
    Task<ICollection<EmployeeDto>> GetEmployeesAsync();
    Task<ICollection<EnterpriseDto>> GetEnterpriseAsync();
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
        var dbos = await dictionariesRepository.SelectDepartmentsAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<DowntimeReasonGroupDto>> GetDowntimeReasonGroupsAsync()
    {
        var dbos = await dictionariesRepository.SelectDowntimeReasonGroupsAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<EmployeeDto>> GetEmployeesAsync()
    {
        var dbos = await dictionariesRepository.SelectEmployeesAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<ICollection<EnterpriseDto>> GetEnterpriseAsync()
    {
        var dbos = await dictionariesRepository.SelectEnterprisesAsync();
        return dbos.Select(e => e.ToDto()).ToList();
    }

    public async Task<ICollection<OperationDto>> GetOperationsAsync()
    {
        var dbos = await dictionariesRepository.SelectOperationsAsync();
        return dbos.Select(o => o.ToDto()).ToList();
    }

    public async Task<ICollection<PaTypeDto>> GetPaTypesAsync()
    {
        var dbos = await dictionariesRepository.SelectPaTypesAsync();
        return dbos.Select(p => p.ToDto()).ToList();
    }

    public async Task<ICollection<ProductDto>> GetProductsAsync()
    {
        var dbos = await dictionariesRepository.SelectProductsAsync();
        return dbos.Select(d => d.ToDto()).ToList();
    }

    public async Task<ICollection<ShiftDto>> GetShiftsAsync()
    {
        var dbos = await dictionariesRepository.SelectShiftsAsync();
        return dbos.Select(s => s.ToDto()).ToList();
    }
}