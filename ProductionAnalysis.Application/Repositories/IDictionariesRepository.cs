using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Repositories;

public interface IDictionariesRepository
{
    Task<ICollection<DepartmentDto>> SelectDepartmentsAsync();
    Task<ICollection<DowntimeReasonGroupDto>> SelectDowntimeReasonGroupsAsync();
    Task<ICollection<EmployeeDto>> SelectEmployeesAsync();
    Task<EmployeeDto?> FindEmployeeByUserIdAsync(Guid userId);
    Task<ICollection<EnterpriseDto>> SelectEnterprisesAsync();
    Task<ICollection<AuxiliaryOperationDto>> SelectAuxiliaryOperationsAsync();
    Task<ICollection<OperationDto>> SelectOperationsAsync();
    Task<ICollection<ProductDto>> SelectProductsAsync();
    Task<ICollection<ShiftDto>> SelectShiftsAsync();
    Task<ShiftDto?> SelectShiftByIdAsync(int shiftId);
    Task<ICollection<ShiftScheduleDto>> SelectShiftSchedulesByShiftIdAsync(int shiftId);
}