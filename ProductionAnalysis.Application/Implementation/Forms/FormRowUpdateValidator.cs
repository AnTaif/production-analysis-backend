using Core.Results;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IFormRowUpdateValidator
{
    Result<(Form Form, FormRow Row)> Validate(UpdateFormRowRequest request, Form? form);
}

[RegisterScoped]
public class FormRowUpdateValidator : IFormRowUpdateValidator
{
    public Result<(Form Form, FormRow Row)> Validate(UpdateFormRowRequest request, Form? form)
    {
        if (form == null)
        {
            return ServiceError.NotFound($"Form with id {request.FormId} not found");
        }

        var row = form.Rows.SingleOrDefault(r => r.Order == request.RowOrder);
        if (row == null)
        {
            return ServiceError.NotFound($"Form row with Order={request.RowOrder} not found in form {request.FormId}");
        }

        return (form, row);
    }
}