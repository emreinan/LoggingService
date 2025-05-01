using LoggingBroker.Application.Services.Loki;
using LoggingBroker.Application.Services.Validations;
using LoggingBroker.Domain.Models;
using MediatR;

namespace LoggingBroker.Application.Operations.SendLog;

public record SendLogCommand(LogRequest LogRequest) : IRequest<Unit>;

public class SendLogCommandHandler(ILokiService lokiService,
                             IValidationDispatcher validation) : IRequestHandler<SendLogCommand, Unit>
{
    public async Task<Unit> Handle(SendLogCommand request, CancellationToken cancellationToken)
    {
        // TODO: Validasyon otomatik ise validation dispatcher'dan kaldır.
        await validation.ValidateAsync(request.LogRequest, cancellationToken);

        // 2) BusinessRules 

        // 3) Call Loki service
        await lokiService.SendLogAsync(request.LogRequest, cancellationToken);

        return Unit.Value;
    }
}