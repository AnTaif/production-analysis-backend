using ProductionAnalysis.Application.Domain.Common.ValueTypes;
using ProductionAnalysis.Client.Models.Dictionaries;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Converters;

public static class DictionariesConverter
{
    public static DepartmentDto ToDto(this DepartmentDbo dbo)
    {
        return new DepartmentDto
        {
            Id = dbo.Id,
            Name = dbo.Name,
            EnterpriseId = dbo.EnterpriseId
        };
    }

    public static DowntimeReasonGroupDto ToDto(this DowntimeReasonGroupDbo dbo)
    {
        return new DowntimeReasonGroupDto
        {
            Id = dbo.Id,
            Name = dbo.Name,
            Description = dbo.Description
        };
    }

    public static EmployeeDto ToDto(this EmployeeDbo dbo)
    {
        var fullName = new FullName(dbo.LastName, dbo.FirstName, dbo.MiddleName);

        return new EmployeeDto
        {
            Id = dbo.Id,
            FullName = fullName.ToString(),
            Position = dbo.Position,
            Email = dbo.Email,
            DepartmentId = dbo.DepartmentId,
            UserId = dbo.UserId,
        };
    }

    public static EnterpriseDto ToDto(this EnterpriseDbo dbo)
    {
        return new EnterpriseDto
        {
            Id = dbo.Id,
            Name = dbo.Name
        };
    }

    public static AuxiliaryOperationDto ToDto(this AuxiliaryOperationDbo dbo)
    {
        return new AuxiliaryOperationDto
        {
            Id = dbo.Id,
            Name = dbo.Name,
            Duration = TimeSpan.FromSeconds(dbo.DurationInSeconds)
        };
    }

    public static OperationDto ToDto(this OperationDbo dbo)
    {
        return new OperationDto
        {
            Id = dbo.Id,
            Name = dbo.Name,
            Duration = dbo.DurationInSeconds == null ? null : TimeSpan.FromSeconds(dbo.DurationInSeconds.Value),
            BasedOnType = (OperationBasedOnType)dbo.BasedOnType,
            BasedOperationId = dbo.BasedOperationId,
            BasedProductId = dbo.BasedProductId
        };
    }

    public static ProductDto ToDto(this ProductDbo dbo)
    {
        return new ProductDto
        {
            Id = dbo.Id,
            Name = dbo.Name,
            TactTime = TimeSpan.FromSeconds(dbo.TactTimeInSeconds),
            EnterpriseId = dbo.EnterpriseId
        };
    }

    public static ShiftDto ToDto(this ShiftDbo dbo)
    {
        return new ShiftDto
        {
            Id = dbo.Id,
            Name = dbo.Name,
            StartTime = dbo.StartTime
        };
    }

    public static ShiftScheduleDto ToDto(this ShiftScheduleDbo dbo)
    {
        return new ShiftScheduleDto
        {
            Id = dbo.Id,
            ShiftId = dbo.ShiftId,
            AuxiliaryOperationId = dbo.AuxiliaryOperationId,
            StartTime = dbo.StartTime
        };
    }
}