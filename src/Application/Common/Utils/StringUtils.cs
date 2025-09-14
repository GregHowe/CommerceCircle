using System.Globalization;

namespace N1coLoyalty.Application.Common.Utils;

public static class StringUtils
{
    public const string FormatCash = "#,0.00";
    public static string TryGetCurrencySymbol(string isoCurrencySymbol)
    {
        return CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Where(c => !c.IsNeutralCulture)
            .Select(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name);
                }
                catch
                {
                    return null;
                }
            })
            .Where(ri => ri != null && ri.ISOCurrencySymbol == isoCurrencySymbol)
            .Select(ri => ri?.CurrencySymbol)
            .FirstOrDefault() ?? "N/A";
    }

    public static decimal RoundToDown(decimal value)
    {
        return Math.Floor(value);
    }
}
