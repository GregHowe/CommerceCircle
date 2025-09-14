using FluentAssertions;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;
using NUnit.Framework;

namespace N1coLoyalty.Domain.UnitTests.ValueObjects;

public class TransactionSubTypeTests
{
    [Test]
    public void ShouldHaveCorrectSubTypeForCash()
    {
        var subtype = TransactionSubType.For("CASH");

        subtype.SubType.Should().Be(EffectSubTypeValue.Cash);
    }
    
    [Test]
    public void ShouldHaveCorrectSubTypeForCompensation()
    {
        var subtype = TransactionSubType.For("COMPENSATION");

        subtype.SubType.Should().Be(EffectSubTypeValue.Compensation);
    }
    
    [Test]
    public void ShouldHaveCorrectSubTypeForRetry()
    {
        var subtype = TransactionSubType.For("RETRY");

        subtype.SubType.Should().Be(EffectSubTypeValue.Retry);
    }
    
    [Test]
    public void ShouldHaveCorrectSubTypeForPoint()
    {
        var subtype = TransactionSubType.For("POINT");

        subtype.SubType.Should().Be(EffectSubTypeValue.Point);
    }
    
    [Test]
    public void ShouldBeEquivalent()
    {
        TransactionSubType.For("CASH").Should().Be(TransactionSubType.For("CASH"));
        TransactionSubType.For("COMPENSATION").Should().Be(TransactionSubType.For("COMPENSATION"));
        TransactionSubType.For("RETRY").Should().Be(TransactionSubType.For("RETRY"));
        TransactionSubType.For("POINT").Should().Be(TransactionSubType.For("POINT"));
    }
    
    [Test]
    public void ShouldCheckInequality()
    {
        (TransactionSubType.For("CASH") != TransactionSubType.For("COMPENSATION")).Should().BeTrue();
    }
    
    [Test]
    public void ShouldThrowExceptionWhenInvalidStatus()
    {
        FluentActions.Invoking(() => TransactionSubType.For("INVALID"))
            .Should().Throw<InvalidOperationException>();
    }
    
    [Test]
    public void ShouldCheckImplicitConversionToString()
    {
        string subtype = TransactionSubType.For("COMPENSATION");
        subtype.Should().Be("COMPENSATION");
    }
    
    [Test]
    public void ShouldCheckValue()
    {
        var subtype = TransactionSubType.For("COMPENSATION");
        subtype.Value.Should().Be("COMPENSATION");
    }
}
