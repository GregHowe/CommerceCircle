using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Services.Void;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.Transactions.Commands.VoidTransaction;

public class VoidTransactionCommand : IRequest<CommonServiceResponse<object>>
{
    public required Guid TransactionId { get; set; }
    public required string Reason { get; set; }
    
    public class VoidTransactionCommandHandler(
        VoidService voidService
        ) : IRequestHandler<VoidTransactionCommand, CommonServiceResponse<object>>
    {
        public async Task<CommonServiceResponse<object>> Handle(VoidTransactionCommand request, CancellationToken cancellationToken)
        {
            var processVoidResponse = await voidService.ProcessVoid(request.TransactionId, request.Reason, TransactionOriginValue.Admin, false ,cancellationToken);
            
            return new CommonServiceResponse<object>
            {
                Success = processVoidResponse.Success,
                Message = processVoidResponse.Message,
                Code = processVoidResponse.Code,
            };
        }
    }
}
