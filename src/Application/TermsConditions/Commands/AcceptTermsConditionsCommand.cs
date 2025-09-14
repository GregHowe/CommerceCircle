using FluentValidation.Results;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Domain.Entities;
using ValidationException = N1coLoyalty.Application.Common.Exceptions.ValidationException;

namespace N1coLoyalty.Application.TermsConditions.Commands;

public class AcceptTermsConditionsCommand : IRequest<CommonServiceResponse<TermsConditionsAcceptanceInfoDto>>
{
    public bool IsAccepted { get; set; }

    public class AcceptTermsConditionsCommandHandler(
        IUser currentUser,
        IUserRepository userRepository,
        ITermsConditionsRepository termsConditionsRepository,
        ITermsConditionsAcceptanceRepository termsConditionsAcceptanceRepository)
        : IRequestHandler<AcceptTermsConditionsCommand, CommonServiceResponse<TermsConditionsAcceptanceInfoDto>>
    {
        public async Task<CommonServiceResponse<TermsConditionsAcceptanceInfoDto>> Handle(
            AcceptTermsConditionsCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetOrCreateUserAsync(currentUser.ExternalId, currentUser.Phone);
            var termsConditionsInfo = await termsConditionsRepository.GetCurrentTermsConditionsAsync();

            if (termsConditionsInfo is null)
                throw new ValidationException(new List<ValidationFailure>()
                {
                    new("TermsConditions", "No se encontraron los términos y condiciones.")
                });

            var termsConditionsAcceptance =
                await termsConditionsAcceptanceRepository.GetTermsConditionsAcceptedAsync(user, termsConditionsInfo);

            return termsConditionsAcceptance is not null
                ? AlreadyAcceptedResponse(termsConditionsInfo.Id)
                : await AcceptTermsConditions(request, user, termsConditionsInfo, cancellationToken);
        }

        private static CommonServiceResponse<TermsConditionsAcceptanceInfoDto> AlreadyAcceptedResponse(
            Guid termsConditionsInfoId)
        {
            return new CommonServiceResponse<TermsConditionsAcceptanceInfoDto>()
            {
                Success = false,
                Message = "Los términos y condiciones ya han sido aceptados.",
                Data = new TermsConditionsAcceptanceInfoDto() { Id = termsConditionsInfoId, IsAccepted = true }
            };
        }

        private async Task<CommonServiceResponse<TermsConditionsAcceptanceInfoDto>> AcceptTermsConditions(
            AcceptTermsConditionsCommand request, User user, TermsConditionsInfo termsConditionsInfo,
            CancellationToken cancellationToken)
        {
            var acceptance = await termsConditionsAcceptanceRepository
                .SaveTermsConditionsAcceptance(user, termsConditionsInfo, request.IsAccepted, cancellationToken);
            
            if (acceptance is null)
                throw new ValidationException(new List<ValidationFailure>()
                {
                    new("TermsConditionsAcceptance", "Hubo un error al intentar aceptar los términos y condiciones.")
                });

            return new CommonServiceResponse<TermsConditionsAcceptanceInfoDto>()
            {
                Message = "Aceptación de los términos y condiciones actualizada con éxito.",
                Data = new TermsConditionsAcceptanceInfoDto()
                {
                    Id = acceptance.Id, IsAccepted = acceptance.IsAccepted
                }
            };
        }
    }
}
