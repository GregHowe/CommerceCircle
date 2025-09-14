using FluentAssertions;
using N1coLoyalty.Application.Common.Utils;
using NUnit.Framework;

namespace N1coLoyalty.Application.UnitTests.Common.Utils;

public class StringUtilsTest
{

    [Test]
    public static void Should_Round_To_Down()
    {
        var amount = 10.75m;
        var result = StringUtils.RoundToDown(amount);
        result.Should().Be(10.00m);
    }


    [Test]
    public static void Should_Get_Currency_Symbol()
    {
        var result2 = StringUtils.TryGetCurrencySymbol("USD");
        result2.Should().Be("$");
    }
}

