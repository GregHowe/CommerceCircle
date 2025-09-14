namespace N1coLoyalty.AdminApi.GraphQL.Mutations;

public class TransactionStatusChangedPayload
{
    public Guid Id { get; init; }

    public string TransactionType { get; init; }

    public string OperationType { get; init; }

    public string TransactionStatus { get; init; }

    public string ApprovalStatus { get; init; }

    public Decimal Amount { get; init; }
    
    public TransactionStatusChangedPosMetadataPayload? PosMetadata { get; init; }

    public TransactionStatusChangedUserPayload User { get; init; }
}

public class TransactionStatusChangedUserPayload
{
    public string ExternalUserId { get; init; }

    public string PhoneNumber { get; init; }

    public string Name { get; init; }
}

public class TransactionStatusChangedPosMetadataPayload
{
    public string? MerchantId { get; init; }

    public string? TerminalId { get; init; }

    public string? Mcc { get; init; }
}
