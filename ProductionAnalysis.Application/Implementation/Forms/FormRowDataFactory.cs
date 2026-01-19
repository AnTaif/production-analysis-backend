using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowDataFactory
{
    FormRowData CreateWorkRow(
        short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ProductContext? productContext);

    FormRowData CreateBreakRow(
        short order,
        Indicator? workTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        string operationName,
        int auxiliaryOperationId,
        ProductContext? productContext = null,
        Indicator? operationNameIndicator = null,
        Indicator? startTimePlanIndicator = null,
        Indicator? endTimePlanIndicator = null,
        int shiftStartMinutes = 0);

    ICollection<FormRowData> CreateOperationCycleRows(
        ref short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        Indicator? operationNameIndicator,
        Indicator? operationTimeIndicator,
        Indicator? startTimePlanIndicator,
        Indicator? endTimePlanIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ICollection<OperationDto> operations,
        int shiftStartMinutes);

    FormRowData CreateOperationTimeRow(
        short order,
        Indicator? operationNameIndicator,
        Indicator? planMinutesIndicator,
        Indicator? startTimePlanIndicator,
        Indicator? endTimePlanIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        OperationDto operation,
        int shiftStartMinutes);
}

[RegisterScoped]
public class FormRowDataFactory(IPlanCalculator planCalculator) : IFormRowDataFactory
{
    public FormRowData CreateWorkRow(
        short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ProductContext? productContext)
    {
        var values = new List<FormRowValueData>
        {
            CreateFormRowValueData(workTimeIndicator, FormatTimeRange(startTime, endTime))
        };

        if (planIndicator is not null)
        {
            if (productContext is null)
                throw new ArgumentNullException(nameof(productContext), "ProductContext cannot be null");

            var planValue = planCalculator.Calculate(startTime, endTime, productContext);
            values.Add(CreateFormRowValueData(planIndicator, planValue.ToString()));
        }

        return new FormRowData
        {
            Order = order,
            IsAuxiliaryOperation = false,
            ProductId = productContext?.ProductId,
            Values = values
        };
    }

    public FormRowData CreateBreakRow(
        short order,
        Indicator? workTimeIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        string operationName,
        int auxiliaryOperationId,
        ProductContext? productContext = null,
        Indicator? operationNameIndicator = null,
        Indicator? startTimePlanIndicator = null,
        Indicator? endTimePlanIndicator = null,
        int shiftStartMinutes = 0)
    {
        var values = new List<FormRowValueData>();

        if (workTimeIndicator is not null)
        {
            values.Add(CreateFormRowValueData(workTimeIndicator,
                FormatTimeRange(startTime, endTime) + " " + operationName));
        }
        else if (operationNameIndicator is not null)
        {
            // Для типов ПА без workTime индикатора используем operationName
            values.Add(CreateFormRowValueData(operationNameIndicator, operationName));
        }

        if (startTimePlanIndicator is not null)
        {
            if (startTimePlanIndicator.ValueType == FieldValueTypes.Time)
            {
                values.Add(CreateFormRowValueData(startTimePlanIndicator, startTime));
            }
            else
            {
                var startMinutes = startTime.Hour * 60 + startTime.Minute - shiftStartMinutes;
                values.Add(CreateFormRowValueData(startTimePlanIndicator, startMinutes.ToString()));
            }
        }

        if (endTimePlanIndicator is not null)
        {
            if (endTimePlanIndicator.ValueType == FieldValueTypes.Time)
            {
                values.Add(CreateFormRowValueData(endTimePlanIndicator, endTime));
            }
            else
            {
                var endMinutes = endTime.Hour * 60 + endTime.Minute - shiftStartMinutes;
                values.Add(CreateFormRowValueData(endTimePlanIndicator, endMinutes.ToString()));
            }
        }

        return new FormRowData
        {
            Order = order,
            IsAuxiliaryOperation = true,
            AuxiliaryOperationId = auxiliaryOperationId,
            ProductId = productContext?.ProductId,
            Values = values
        };
    }

    public ICollection<FormRowData> CreateOperationCycleRows(
        ref short order,
        Indicator workTimeIndicator,
        Indicator? planIndicator,
        Indicator? operationNameIndicator,
        Indicator? operationTimeIndicator,
        Indicator? startTimePlanIndicator,
        Indicator? endTimePlanIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        ICollection<OperationDto> operations,
        int shiftStartMinutes)
    {
        var rows = new List<FormRowData>();
        var workTimeValue = FormatTimeRange(startTime, endTime);
        const string planValue = "1"; // В плане всегда 1 шт. в рамках полного цикла под-операций

        var groupKey = order;
        var operationList = operations.ToList();

        // Вычисляем общую длительность цикла
        var totalCycleDuration = TimeHelper.CalculateDurationAcrossMidnight(startTime, endTime);

        // Вычисляем длительности операций
        var operationsWithDuration = operationList
            .Where(op => op.Duration.HasValue)
            .ToList();
        var operationsWithoutDuration = operationList
            .Where(op => !op.Duration.HasValue)
            .ToList();
        var totalOperationsDuration = operationsWithDuration
            .Aggregate(TimeSpan.Zero, (sum, op) => sum + op.Duration!.Value);

        // Вычисляем длительность для операций без указанной длительности
        TimeSpan durationPerOperationWithoutDuration = TimeSpan.Zero;
        if (operationsWithoutDuration.Count > 0)
        {
            var remainingDuration = totalCycleDuration - totalOperationsDuration;
            if (remainingDuration > TimeSpan.Zero)
            {
                durationPerOperationWithoutDuration =
                    TimeSpan.FromTicks(remainingDuration.Ticks / operationsWithoutDuration.Count);
            }
        }

        // Если ни у одной операции нет длительности, распределяем время равномерно
        if (operationsWithDuration.Count == 0 && operationList.Count > 0)
        {
            durationPerOperationWithoutDuration = TimeSpan.FromTicks(totalCycleDuration.Ticks / operationList.Count);
        }

        // Текущее время для расчета времени начала/окончания каждой операции
        var currentOperationStartTime = startTime;

        for (var i = 0; i < operationList.Count; i++)
        {
            var operation = operationList[i];
            var values = new List<FormRowValueData>
            {
                CreateFormRowValueData(workTimeIndicator, workTimeValue)
            };

            if (planIndicator is not null) values.Add(CreateFormRowValueData(planIndicator, planValue));

            if (operationNameIndicator is not null)
            {
                var operationName = $"{i + 1}. {operation.Name}";
                values.Add(CreateFormRowValueData(operationNameIndicator, operationName));
            }

            if (operationTimeIndicator is not null)
            {
                var operationTime = operation.Duration.HasValue
                    ? operation.Duration.Value.TotalMinutes.ToString("0")
                    : "-";
                values.Add(CreateFormRowValueData(operationTimeIndicator, operationTime));
            }

            // Рассчитываем время начала и окончания для текущей операции
            TimeOnly operationStartTime = currentOperationStartTime;
            TimeOnly operationEndTime;

            if (operation.Duration.HasValue)
            {
                // Если у операции есть длительность, используем её
                operationEndTime = currentOperationStartTime.Add(operation.Duration.Value);
            }
            else
            {
                // Если у операции нет длительности, используем распределенное время
                operationEndTime = currentOperationStartTime.Add(durationPerOperationWithoutDuration);
            }

            // Для последней операции убеждаемся, что время окончания совпадает с концом цикла
            if (i == operationList.Count - 1)
            {
                operationEndTime = endTime;
            }

            // Обновляем время начала для следующей операции
            currentOperationStartTime = operationEndTime;

            if (startTimePlanIndicator is not null)
            {
                if (startTimePlanIndicator.ValueType == FieldValueTypes.Time)
                {
                    values.Add(CreateFormRowValueData(startTimePlanIndicator, operationStartTime));
                }
                else
                {
                    var startMinutes = operationStartTime.Hour * 60 + operationStartTime.Minute - shiftStartMinutes;
                    values.Add(CreateFormRowValueData(startTimePlanIndicator, startMinutes.ToString()));
                }
            }

            if (endTimePlanIndicator is not null)
            {
                if (endTimePlanIndicator.ValueType == FieldValueTypes.Time)
                {
                    values.Add(CreateFormRowValueData(endTimePlanIndicator, operationEndTime));
                }
                else
                {
                    var endMinutes = operationEndTime.Hour * 60 + operationEndTime.Minute - shiftStartMinutes;
                    values.Add(CreateFormRowValueData(endTimePlanIndicator, endMinutes.ToString()));
                }
            }

            rows.Add(new FormRowData
            {
                Order = order++,
                IsAuxiliaryOperation = false,
                GroupKey = groupKey,
                Values = values
            });
        }

        return rows;
    }

    public FormRowData CreateOperationTimeRow(
        short order,
        Indicator? operationNameIndicator,
        Indicator? planMinutesIndicator,
        Indicator? startTimePlanIndicator,
        Indicator? endTimePlanIndicator,
        TimeOnly startTime,
        TimeOnly endTime,
        OperationDto operation,
        int shiftStartMinutes)
    {
        var values = new List<FormRowValueData>();

        if (operationNameIndicator is not null)
        {
            values.Add(CreateFormRowValueData(operationNameIndicator, operation.Name));
        }

        if (startTimePlanIndicator is not null)
        {
            if (startTimePlanIndicator.ValueType == FieldValueTypes.Time)
            {
                values.Add(CreateFormRowValueData(startTimePlanIndicator, startTime));
            }
            else
            {
                var startMinutes = startTime.Hour * 60 + startTime.Minute - shiftStartMinutes;
                values.Add(CreateFormRowValueData(startTimePlanIndicator, startMinutes.ToString()));
            }
        }

        if (endTimePlanIndicator is not null)
        {
            if (endTimePlanIndicator.ValueType == FieldValueTypes.Time)
            {
                values.Add(CreateFormRowValueData(endTimePlanIndicator, endTime));
            }
            else
            {
                var endMinutes = endTime.Hour * 60 + endTime.Minute - shiftStartMinutes;
                values.Add(CreateFormRowValueData(endTimePlanIndicator, endMinutes.ToString()));
            }
        }

        if (planMinutesIndicator is not null)
        {
            var planMinutes = operation.Duration.HasValue
                ? operation.Duration.Value.TotalMinutes.ToString("0")
                : "0";
            values.Add(CreateFormRowValueData(planMinutesIndicator, planMinutes));
        }

        return new FormRowData
        {
            Order = order,
            IsAuxiliaryOperation = false,
            Values = values
        };
    }

    private static string FormatTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        return $"{startTime:HH:mm}-{endTime:HH:mm}";
    }

    private static FormRowValueData CreateFormRowValueData(Indicator indicator, object value)
    {
        return new FormRowValueData
        {
            IndicatorId = indicator.Id,
            Value = value
        };
    }
}