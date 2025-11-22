using ProductionAnalysis.Application.Domain.Common.ValueTypes;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Application.Converters;

public static class DictionariesConverter
{
    public static DepartmentDto ToDto(this DepartmentDbo dbo)
    {
        return new DepartmentDto(
            dbo.Id,
            dbo.Name,
            dbo.EnterpriseId
        );
    }

    public static DowntimeReasonGroupDto ToDto(this DowntimeReasonGroupDbo dbo)
    {
        return new DowntimeReasonGroupDto(
            dbo.Id,
            dbo.Name,
            dbo.Description
        );
    }

    public static EmployeeDto ToDto(this EmployeeDbo dbo)
    {
        var fullName = new FullName(dbo.LastName, dbo.FirstName, dbo.MiddleName);

        return new EmployeeDto(
            dbo.Id,
            fullName.ToString(),
            dbo.Position,
            dbo.DepartmentId
        );
    }

    public static EnterpriseDto ToDto(this EnterpriseDbo dbo)
    {
        return new EnterpriseDto(
            dbo.Id,
            dbo.Name
        );
    }

    public static OperationDto ToDto(this OperationDbo dbo)
    {
        return new OperationDto(
            dbo.Id,
            dbo.Name,
            dbo.DurationInSeconds == null ? null : TimeSpan.FromSeconds(dbo.DurationInSeconds.Value),
            (OperationBasedOnType)dbo.BasedOnType,
            dbo.BasedOperationId,
            dbo.BasedProductId
        );
    }

    public static PaTypeDto ToDto(this PaTypeDbo dbo)
    {
        return new PaTypeDto(
            dbo.Id,
            dbo.Name
        );
    }

    public static ProductDto ToDto(this ProductDbo dbo)
    {
        return new ProductDto(
            dbo.Id,
            dbo.Name,
            TimeSpan.FromSeconds(dbo.TactTimeInSeconds),
            dbo.EnterpriseId
        );
    }

    public static ShiftDto ToDto(this ShiftDbo dbo)
    {
        return new ShiftDto(
            dbo.Id,
            dbo.Name,
            dbo.StartTime
        );
    }
}