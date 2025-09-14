using BusinessEvents.Contracts.BillPayments;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Consumers.Common;

namespace N1coLoyalty.Application.Consumers.BillPayment;

public class BillPaidConsumer(
    ILogger<BillPaidConsumer> logger,
    IApplicationDbContext dbContext,
    ILoyaltyEngine loyaltyEngine,
    IConfiguration configuration) : IConsumer<BillPaid>
{
    public async Task Consume(ConsumeContext<BillPaid> context)
    {
        try
        {
            logger.LogInformation("New message in {@Consumer} {@Message}", GetType().Name, context.Message);
            var message = context.Message;

            if (!message.IsManagedPaymentMethod)
            {
                logger.LogInformation("Bill Paid Consumer: Not a managed payment method, BillId: {@Id}", message.Id);
                return;
            }
            
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalUserId == message.User.ExternalUserId);
            if (user is null)
            {
                logger.LogWarning(
                    "Bill Paid Consumer: User not found, ExternalUserId: {@ExternalUserId}, BillId: {@Id}",
                    message.User.ExternalUserId, message.Id);
                return;
            }
            
            var profile = new LoyaltyProfileDto
            {
                IntegrationId = user.ExternalUserId,
                FirstName = user.Name,
                PhoneNumber = user.Phone,
            };
            
            var attributes = new Dictionary<string, object>
            {
                { AttributesConstants.BillId, message.Id },
                { AttributesConstants.BillAmount, message.Amount },
                { AttributesConstants.ServiceName, message.ServiceName },
                { AttributesConstants.CategoryName, message.CategoryName },
                { AttributesConstants.Datetime, message.DateTime },
            };
            
            var loyaltyProgramIntegrationId = configuration["LoyaltyCore:LoyaltyProgramIntegrationId"] ??
                                              string.Empty;
            await loyaltyEngine.ProcessEventAsync(new ProcessEventInputDto
            {
                LoyaltyProgramIntegrationId = loyaltyProgramIntegrationId,
                EventType = EventTypeConstant.BillPaid,
                LoyaltyProfile = profile,
                Attributes = attributes,
            });
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Bill Paid Consumer: Error processing message. Error: {@Error} Message: {@Message}",
                e.Message, context.Message);
        }
    }
}
