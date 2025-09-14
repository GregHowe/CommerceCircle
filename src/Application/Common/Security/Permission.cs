using System.Reflection;

namespace N1coLoyalty.Application.Common.Security;

public static class Permission
{
    /// <summary>
    /// Users
    /// </summary>
    public const string ReadUsers = "read:lyt-users";
    public const string WriteUserWalletBalance = "write:lyt-user-wallet-balance";
    
    /// <summary>
    /// Transactions
    /// </summary>
    public const string ReadTransactions = "read:lyt-transactions";
    
    
    /// <summary>
    /// Get all permissions
    /// </summary>
    /// <returns>list of permissions</returns>
    public static string[] GetAllPermissions()
    {
        var type = typeof(Permission);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        var permissions = fields.Where(field => field.FieldType == typeof(string))
            .Select(field => (string)field.GetValue(null))
            .ToArray();
        
        var permissionsToIgnore = new List<string>{ };
        
        permissions = permissions.Where(permission =>
                !permissionsToIgnore.Contains(permission)
            )
            .ToArray();

        return permissions;
    }
}