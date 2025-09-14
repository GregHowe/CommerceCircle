using N1coLoyalty.Application.Common.Models;

namespace N1coLoyalty.Api.GraphQL.ObjectTypes;

public class GameSettingsDtoType : ObjectType<GameSettingsDto>
{
    protected override void Configure(IObjectTypeDescriptor<GameSettingsDto> descriptor)
    {
        descriptor.Field(f => f.UserEventLimit)
            .Deprecated("Use the `AttemptsLimit` field instead")
            .Resolve(context => context.Parent<GameSettingsDto>().AttemptsLimit);
        descriptor.Field(f => f.EventCost)
            .Deprecated("Use the `AttemptCost` field instead")
            .Resolve(context => context.Parent<GameSettingsDto>().AttemptCost);
        descriptor.Field(f => f.UnredeemedFreeEvents)
            .Deprecated("Use the `UnredeemedFreeAttempts` field instead")
            .Resolve(context => context.Parent<GameSettingsDto>().UnredeemedFreeAttempts);
        descriptor.Field(f => f.UserEventRemaining)
            .Deprecated("Use the `RemainingAttempts` field instead")
            .Resolve(context => context.Parent<GameSettingsDto>().RemainingAttempts);
    }
}
