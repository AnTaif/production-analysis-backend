using Core.Results;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormsService
{
    Task<PaginatedResult<FormShortDto>> SearchFormsAsync(SearchFormsFilterDto searchFilter);
    Task<Result<FormShortDto>> CreateAsync(CreateFormRequest request, Guid creatorId);
    Task<Result<FormDto>> GetByIdAsync(int formId);
    Task<Result<ICollection<FormRowDto>>> GetFormRowsAsync(int formId);
}

[RegisterScoped]
public class FormsService(
    IPaUnitOfWork unitOfWork
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
        createForm.TemplateSnapshot = TemplateSerializer.SerializeTemplateSnapshot(template);

        var form = unitOfWork.Forms.Create(createForm);
        await unitOfWork.SaveChangesAsync();

        // Заполняем строки времени работы и обедов/перерывов на основе смены
        if (createForm.Context.TryGetValue("shift", out var shiftValue))
        {
            var shiftId = Convert.ToInt32(shiftValue);
            var shifts = await unitOfWork.Dictionaries.SelectShiftsAsync();
            var shift = shifts.FirstOrDefault(s => s.Id == shiftId);

            if (shift != null)
            {
                var schedules = await unitOfWork.Dictionaries.SelectShiftSchedulesByShiftIdAsync(shiftId);
                var rows = await FormRowGenerator.GenerateRowsForShiftAsync(
                    shift.StartTime,
                    schedules,
                    template,
                    unitOfWork);

                await unitOfWork.Forms.CreateFormRowsAsync(form.Id, rows);
                await unitOfWork.SaveChangesAsync();
            }
        }

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

        var rows = form.Rows
            .OrderBy(r => r.Order)
            .Select(r => new FormRowDto(
                r.Order,
                r.IsAdditionalOperation,
                r.Values
            ))
            .ToList();

        return rows;
    }
}