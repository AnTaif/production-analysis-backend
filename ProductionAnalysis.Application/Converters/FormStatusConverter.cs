using static ProductionAnalysis.Application.Domain.Forms.FormStatus;
using FormStatus = ProductionAnalysis.Application.Domain.Forms.FormStatus;

namespace ProductionAnalysis.Application.Converters;

internal static class FormStatusConverter
{
    public static FormStatus ConvertToDomainFormStatus(Client.Models.Forms.FormStatus clientStatus)
    {
        return clientStatus switch
        {
            Client.Models.Forms.FormStatus.InProgress => InProgress,
            Client.Models.Forms.FormStatus.Completed => Finished,
            _ => throw new ArgumentOutOfRangeException(nameof(clientStatus), clientStatus, null)
        };
    }

    public static Client.Models.Forms.FormStatus ConvertToClientFormStatus(FormStatus domainStatus)
    {
        return domainStatus switch
        {
            InProgress => Client.Models.Forms.FormStatus.InProgress,
            Finished => Client.Models.Forms.FormStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(domainStatus), domainStatus, null)
        };
    }
}