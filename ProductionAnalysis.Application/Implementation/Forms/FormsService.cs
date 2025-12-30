using System.Globalization;
using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormsService
{
    Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter);
    Task<Result<FormShortDto>> CreateAsync(CreateFormRequest request, Guid creatorId);
    Task<Result<FormDto>> GetByIdAsync(int formId);
    Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId);
    Task<Result<FormRowDto>> UpdateFormRowAsync(UpdateFormRowRequest request, Guid userId);
}

[RegisterScoped]
public class FormsService(
    IPaUnitOfWork unitOfWork,
    IFormRowGenerator formRowGenerator,
    IFormulaCalculator formulaCalculator
)
    : IFormsService
{
    public async Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter)
    {
        var domainFilter = searchFilter.ToDomain();
        var (forms, totalCount) = await unitOfWork.Forms.SearchFormsAsync(domainFilter);

        var dtos = forms.Select(f => f.ToShortDto()).ToList();

        var response = new PaginatedResponse<FormShortDto>(
            dtos,
            totalCount,
            domainFilter.PageNumber,
            domainFilter.PageSize
        );

        return response;
    }

    public async Task<Result<FormShortDto>> CreateAsync(CreateFormRequest request, Guid creatorId)
    {
        var createForm = request.ToDomain(creatorId);

        var template = await unitOfWork.Templates.GetLatestByPaTypeIdAsync(createForm.PaTypeId);
        if (template == null)
        {
            return ServiceError.NotFound($"Template for PaType {createForm.PaTypeId} not found");
        }

        createForm.TemplateSnapshot = TemplateSerializer.SerializeTemplateSnapshot(template);

        var form = await unitOfWork.Forms.CreateAsync(createForm);

        await CreateFormRowsIfNeededAsync(form.Id, createForm.ShiftId, template);
        await unitOfWork.SaveChangesAsync();

        return form.ToShortDto();
    }

    public async Task<Result<FormDto>> GetByIdAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        return form.ToDto();
    }

    public async Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId)
    {
        var form = await unitOfWork.Forms.FindAsync(formId);

        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {formId} not found");
        }

        return form.Rows.ToRowDtos();
    }

    private async Task CreateFormRowsIfNeededAsync(int formId, int shiftId, Template template)
    {
        var shift = await unitOfWork.Dictionaries.SelectShiftByIdAsync(shiftId);
        if (shift == null)
        {
            return;
        }

        var schedules = await unitOfWork.Dictionaries.SelectShiftSchedulesByShiftIdAsync(shiftId);
        var rows = await formRowGenerator.GenerateRowsForShiftAsync(
            shift.StartTime,
            schedules,
            template);

        await unitOfWork.Forms.CreateFormRowsAsync(formId, rows);
    }

    public async Task<Result<FormRowDto>> UpdateFormRowAsync(UpdateFormRowRequest request, Guid userId)
    {
        var form = await unitOfWork.Forms.FindAsync(request.FormId);
        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {request.FormId} not found");
        }

        var row = form.Rows.SingleOrDefault(r => r.Order == request.RowOrder);
        if (row == null)
        {
            return ServiceError.NotFound($"Form row with Order={request.RowOrder} not found in form {request.FormId}");
        }

        var templateSnapshot = FormTemplateParser.ParseTemplateSnapshot(form.TemplateSnapshot);
        var filteredValues = FilterUpdatableValues(request.Values, templateSnapshot);

        if (!filteredValues.Any())
        {
            return row.ToRowDto();
        }

        // Загружаем шаблон для пересчета формул
        var template = await unitOfWork.Templates.GetLatestByPaTypeIdAsync(form.PaTypeId);
        if (template == null)
        {
            return ServiceError.NotFound($"Template for PaType {form.PaTypeId} not found");
        }

        // Обновляем значения вручную введенных индикаторов
        await unitOfWork.Forms.UpdateFormRowValuesAsync(
            request.FormId,
            request.RowOrder,
            filteredValues,
            userId);

        // Пересчитываем и обновляем формулы
        var updatedIndicatorIds = filteredValues.Select(v => v.IndicatorId).ToList();
        var formulaValuesToUpdate = await CalculateFormulaValuesAsync(
            form,
            row,
            template,
            updatedIndicatorIds);

        // Обновляем значения формул, если они изменились
        if (formulaValuesToUpdate.Any())
        {
            await unitOfWork.Forms.UpdateFormRowValuesAsync(
                request.FormId,
                request.RowOrder,
                formulaValuesToUpdate,
                userId);
        }

        // Сохраняем все изменения одной транзакцией
        await unitOfWork.SaveChangesAsync();

        // Перезагружаем форму для получения актуальных данных
        var updatedForm = await unitOfWork.Forms.FindAsync(request.FormId);
        var updatedRow = updatedForm?.Rows.SingleOrDefault(r => r.Order == request.RowOrder);

        if (updatedRow == null)
        {
            return ServiceError.NotFound($"Form row with Order={request.RowOrder} not found after update");
        }

        return updatedRow.ToRowDto();
    }

    private static List<FormRowValueData> FilterUpdatableValues(
        Dictionary<int, object> requestValues,
        FormTemplateDto templateSnapshot)
    {
        var indicatorsDict = templateSnapshot.TableColumns
            .Where(c => c.Id > 0 && !string.IsNullOrEmpty(c.InputType))
            .ToDictionary(c => c.Id, c => c.InputType);

        var filteredValues = new List<FormRowValueData>();
        foreach (var (indicatorId, value) in requestValues)
        {
            if (!indicatorsDict.TryGetValue(indicatorId, out var inputType))
            {
                continue;
            }

            if (inputType is FieldInputTypes.Manual or FieldInputTypes.Dictionary)
            {
                filteredValues.Add(new FormRowValueData
                {
                    IndicatorId = indicatorId,
                    Value = value
                });
            }
        }

        return filteredValues;
    }

    private async Task<List<FormRowValueData>> CalculateFormulaValuesAsync(
        Form form,
        FormRow row,
        Template template,
        ICollection<int> updatedIndicatorIds)
    {
        // Собираем текущие значения строки в словарь (ключ - IndicatorId)
        var currentValues = ParseRowValuesToDictionary(row.Values);

        // Вычисляем формулы
        var calculatedValues = formulaCalculator.CalculateFormulas(
            currentValues,
            template.Indicators,
            updatedIndicatorIds);

        // Находим значения формул, которые нужно обновить
        var formulaValuesToUpdate = new List<FormRowValueData>();
        foreach (var (indicatorId, calculatedValue) in calculatedValues)
        {
            // Обновляем только если значение изменилось или его не было
            if (!currentValues.TryGetValue(indicatorId, out var oldValue) ||
                !AreValuesEqual(oldValue, calculatedValue))
            {
                var formulaIndicator = template.Indicators.FirstOrDefault(i => i.Id == indicatorId);
                if (formulaIndicator != null && formulaIndicator.InputType == FieldInputTypes.Formula)
                {
                    formulaValuesToUpdate.Add(new FormRowValueData
                    {
                        IndicatorId = indicatorId,
                        Value = calculatedValue
                    });
                }
            }
        }

        return formulaValuesToUpdate;
    }

    private static Dictionary<int, object> ParseRowValuesToDictionary(Dictionary<string, object> rowValues)
    {
        var result = new Dictionary<int, object>();
        foreach (var (key, value) in rowValues)
        {
            if (int.TryParse(key, out var indicatorId))
            {
                result[indicatorId] = value;
            }
        }

        return result;
    }

    private static bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 == null && value2 == null)
        {
            return true;
        }

        if (value1 == null || value2 == null)
        {
            return false;
        }

        // Преобразуем в числа для сравнения
        if (TryConvertToDouble(value1, out var num1) && TryConvertToDouble(value2, out var num2))
        {
            return Math.Abs(num1 - num2) < 0.0001;
        }

        return value1.Equals(value2);
    }

    private static bool TryConvertToDouble(object value, out double result)
    {
        result = 0;
        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case double d:
                result = d;
                return true;
            case decimal dec:
                result = (double)dec;
                return true;
            case float f:
                result = f;
                return true;
            case string s:
                return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            default:
                return false;
        }
    }
}