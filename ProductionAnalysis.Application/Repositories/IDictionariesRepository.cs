using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Repositories;

public interface IDictionariesRepository
{
    // Departments
    Task<ICollection<DepartmentDto>> SelectDepartmentsAsync();
    Task<DepartmentDto?> FindDepartmentByIdAsync(int departmentId);
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request);
    Task<DepartmentDto?> UpdateDepartmentAsync(int departmentId, UpdateDepartmentRequest request);
    Task<bool> DeleteDepartmentAsync(int departmentId);
    Task<bool> DepartmentExistsAsync(int departmentId);

    // DowntimeReasonGroups
    Task<ICollection<DowntimeReasonGroupDto>> SelectDowntimeReasonGroupsAsync();
    Task<DowntimeReasonGroupDto?> FindDowntimeReasonGroupByIdAsync(int id);
    Task<DowntimeReasonGroupDto> CreateDowntimeReasonGroupAsync(CreateDowntimeReasonGroupRequest request);
    Task<DowntimeReasonGroupDto?> UpdateDowntimeReasonGroupAsync(int id, UpdateDowntimeReasonGroupRequest request);
    Task<bool> DeleteDowntimeReasonGroupAsync(int id);

    // Employees
    Task<ICollection<EmployeeDto>> SelectEmployeesAsync();
    Task<ICollection<EmployeeDto>> SelectEmployeesByDepartmentIdAsync(int departmentId);
    Task<EmployeeDto?> FindEmployeeByUserIdAsync(Guid userId);
    Task<EmployeeDto?> FindEmployeeByIdAsync(int employeeId);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request);
    Task<EmployeeDto?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request);
    Task<bool> DeleteEmployeeAsync(int employeeId);

    // Enterprises
    Task<ICollection<EnterpriseDto>> SelectEnterprisesAsync();
    Task<EnterpriseDto?> FindEnterpriseByIdAsync(int enterpriseId);
    Task<EnterpriseDto> CreateEnterpriseAsync(CreateEnterpriseRequest request);
    Task<EnterpriseDto?> UpdateEnterpriseAsync(int enterpriseId, UpdateEnterpriseRequest request);
    Task<bool> DeleteEnterpriseAsync(int enterpriseId);
    Task<bool> EnterpriseExistsAsync(int enterpriseId);

    // Positions
    Task<ICollection<PositionDto>> SelectPositionsAsync();
    Task<PositionDto?> FindPositionByIdAsync(int positionId);
    Task<PositionDto> CreatePositionAsync(CreatePositionRequest request);
    Task<PositionDto?> UpdatePositionAsync(int positionId, UpdatePositionRequest request);
    Task<bool> DeletePositionAsync(int positionId);
    Task<bool> PositionExistsAsync(int positionId);

    // AuxiliaryOperations
    Task<ICollection<AuxiliaryOperationDto>> SelectAuxiliaryOperationsAsync();
    Task<AuxiliaryOperationDto?> FindAuxiliaryOperationByIdAsync(int id);
    Task<AuxiliaryOperationDto> CreateAuxiliaryOperationAsync(CreateAuxiliaryOperationRequest request);
    Task<AuxiliaryOperationDto?> UpdateAuxiliaryOperationAsync(int id, UpdateAuxiliaryOperationRequest request);
    Task<bool> DeleteAuxiliaryOperationAsync(int id);

    // Operations
    Task<ICollection<OperationDto>> SelectOperationsAsync();
    Task<ICollection<OperationDto>> SelectAllOperationsAsync();
    Task<OperationDto?> FindOperationByIdAsync(int operationId);
    Task<OperationDto> CreateOperationAsync(CreateOperationRequest request);
    Task<OperationDto?> UpdateOperationAsync(int operationId, UpdateOperationRequest request);
    Task<bool> DeleteOperationAsync(int operationId);
    Task<bool> OperationExistsAsync(int operationId);

    // Products
    Task<ICollection<ProductDto>> SelectProductsAsync();
    Task<ProductDto?> FindProductByIdAsync(int productId);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto?> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int productId);
    Task<bool> ProductExistsAsync(int productId);

    // Shifts
    Task<ICollection<ShiftDto>> SelectShiftsAsync();
    Task<ShiftDto?> SelectShiftByIdAsync(int shiftId);
    Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request);
    Task<ShiftDto?> UpdateShiftAsync(int shiftId, UpdateShiftRequest request);
    Task<bool> DeleteShiftAsync(int shiftId);
    Task<ICollection<ShiftScheduleDto>> SelectShiftSchedulesByShiftIdAsync(int shiftId);
}