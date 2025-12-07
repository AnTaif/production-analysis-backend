using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Repositories;

public interface IDictionariesRepository
{
    Task<ICollection<DepartmentDto>> SelectDepartmentsAsync();
    Task<ICollection<DowntimeReasonGroupDto>> SelectDowntimeReasonGroupsAsync();
    Task<ICollection<EmployeeDto>> SelectEmployeesAsync();
    Task<ICollection<EnterpriseDto>> SelectEnterprisesAsync();
    Task<ICollection<AdditionalOperationDto>> SelectAdditionalOperationsAsync();
    Task<ICollection<OperationDto>> SelectOperationsAsync();
    Task<ICollection<PaTypeDto>> SelectPaTypesAsync();
    Task<ICollection<ProductDto>> SelectProductsAsync();
    Task<ICollection<ShiftDto>> SelectShiftsAsync();
}