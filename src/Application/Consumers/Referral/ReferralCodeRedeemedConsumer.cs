using BusinessEvents.Contracts.Issuing;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Consumers.Common;

namespace N1coLoyalty.Application.Consumers.Referral;

public class ReferralCodeRedeemedConsumer(
    ILogger<ReferralCodeRedeemedConsumer> logger,
    IApplicationDbContext dbContext,
    ILoyaltyEngine loyaltyEngine,
    IConfiguration configuration) : IConsumer<ReferralCodeRedeemed>
{
    public async Task Consume(ConsumeContext<ReferralCodeRedeemed> context)
    {
        try
        {
            logger.LogInformation("New message in {@Consumer} {@Message}", GetType().Name, context.Message);
            var message = context.Message;

            var userAdvocate = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalUserId == message.AdvocateUser.ExternalUserId);
            if (userAdvocate is null)
            {
                logger.LogWarning(
                    "Referral Code Redeemed Consumer: User not found, ExternalUserId: {@ExternalUserId}, ReferralId: {@Id}",
                    message.AdvocateUser.ExternalUserId, message.Id);
                return;
            }

            var profile = new LoyaltyProfileDto
            {
                IntegrationId = userAdvocate.ExternalUserId,
                FirstName = userAdvocate.Name,
                PhoneNumber = userAdvocate.Phone,
            };

            var origin = message.Origin;
            var attributes = new Dictionary<string, object>
            {
                { AttributesConstants.Origin, origin },
                { AttributesConstants.ReferralCode, message.ReferralCode },
                { AttributesConstants.ReferralServiceProviderId, message.ReferralServiceProviderId },
                { AttributesConstants.ReferralUserName, message.ReferralUser.Name},
                { AttributesConstants.ReferralUserPhoneNumber, message.ReferralUser.PhoneNumber}
            };

            var loyaltyProgramIntegrationId = configuration["LoyaltyCore:LoyaltyProgramIntegrationId"] ??
                                                  string.Empty;
            await loyaltyEngine.ProcessEventAsync(new ProcessEventInputDto
            {
                LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
                EventType = EventTypeConstant.ReferralRedeemed,
                LoyaltyProfile = profile,
                Attributes = attributes,
            });
        }
        catch (Exception e)
        {
            logger.LogError(exception: e,"Error in {@Class} {@Message}", GetType().Name, e.Message);
        }

    }
}
