using Core.Auth;
using FluentAssertions;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Tests.Infrastructure;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Tests.Forms;

public abstract class FormsTestBase : BaseIntegrationTest
{
    protected async Task<int> CreateAssigneeAsync(int departmentId = 1)
    {
        var assigneeUser = await DataBuilder.CreateUserAsync($"assignee{Guid.NewGuid()}@test.com");
        var assignee = await DataBuilder.CreateEmployeeAsync(assigneeUser.Id, departmentId);
        return assignee.Id;
    }

    protected async Task UpdateRowAsync(int formId, short rowOrder, int indicatorId, int value, Guid userId)
    {
        var updateRequest = new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object> { { indicatorId, value } }
        };

        var result = await FormsService.UpdateFormRowAsync(formId, rowOrder, updateRequest, userId);
        result.IsSuccess.Should().BeTrue();
    }

    protected async Task UpdateRowAsync(int formId, short rowOrder, Dictionary<int, object> values, Guid userId)
    {
        var updateRequest = new UpdateFormRowRequest { Values = values };
        var result = await FormsService.UpdateFormRowAsync(formId, rowOrder, updateRequest, userId);
        result.IsSuccess.Should().BeTrue();
    }

    protected static int GetValue(FormRow row, int indicatorId)
    {
        var key = indicatorId.ToString();
        if (!row.Values.TryGetValue(key, out var rowValue))
        {
            return 0;
        }

        return Convert.ToInt32(rowValue.Value);
    }

    protected static int GetCumulativeValue(FormRow row, int cumulativeIndicatorId)
    {
        var key = cumulativeIndicatorId.ToString();
        return !row.Values.TryGetValue(key, out var rowValue)
            ? 0
            : Convert.ToInt32(rowValue.Value);
    }

    protected static DateTime? GetDateTimeValue(FormRow row, int indicatorId)
    {
        var key = indicatorId.ToString();
        if (!row.Values.TryGetValue(key, out var rowValue))
        {
            return null;
        }

        return rowValue.Value switch
        {
            DateTime dt => dt,
            TimeOnly time => DateTime.Today.Add(time.ToTimeSpan()),
            _ => null
        };
    }

    protected static ContextUser CreateContextUser(Guid userId, params string[] roles)
    {
        return new ContextUser
        {
            Id = userId,
            Roles = roles.ToHashSet()
        };
    }

    protected static CreateFormRequest CreateSingleProductFormRequest(int shiftId, int assigneeId, int productId = 1,
        int dailyRate = 400, int cycleTime = 72)
    {
        return new CreateFormRequest
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = shiftId,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            Product = new ProductContextRequest
            {
                ProductId = productId,
                DailyRate = dailyRate,
                CycleTime = cycleTime
            }
        };
    }

    protected static CreateFormRequest CreateLessThanOnePerHourFormRequest(int shiftId, int assigneeId, int productId)
    {
        return new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerHour,
            ShiftId = shiftId,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            OperationOrProduct = new OperationOrProductContextRequest
            {
                ProductId = productId
            }
        };
    }

    protected static CreateFormRequest CreateLessThanOnePerShiftFormRequest(int shiftId, int assigneeId,
        int operationId)
    {
        return new CreateFormRequest
        {
            PaType = PaTypeDto.LessThanOnePerShift,
            ShiftId = shiftId,
            AssigneeId = assigneeId,
            FormDate = DateTime.UtcNow.Date,
            OperationOrProduct = new OperationOrProductContextRequest
            {
                OperationId = operationId
            }
        };
    }
}