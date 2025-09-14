namespace N1coLoyalty.Application.Common.Constants;

public static class AttributesConstants
{
    // Expense transaction attributes
    public static string TransactionId => "transactionId";
    public static string TransactionAmount => "transactionAmount";
    public static string TerminalId => "terminalId";
    public static string MerchantId => "merchantId";
    public static string Mcc => "mcc";
    // Bill payment attributes
    public static string BillId => "billId";
    public static string BillAmount => "billAmount";
    public static string ServiceName => "serviceName";
    public static string CategoryName => "categoryName";
    public static string Datetime => "datetime";
    // Referral attributes
    public static string Origin => "origin";
    public static string ReferralCode => "referralCode";
    public static string ReferralServiceProviderId => "referralServiceProviderId";
    public static string ReferralUserName => "referralUserName";
    public static string ReferralUserPhoneNumber => "referralUserPhoneNumber";
}
