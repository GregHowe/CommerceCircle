using FluentAssertions;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;
using NUnit.Framework;

namespace N1coLoyalty.Domain.UnitTests.ValueObjects;

public class EffectTypeTests
{
    [Test]
    public void ShouldHaveCorrectTypeForReward()
    {
        var type = EffectType.For("REWARD");

        type.Type.Should().Be(EffectTypeValue.Reward);
    }
    
    [Test]
    public void ShouldBeEquivalent()
    {
        EffectType.For("REWARD").Should().Be(EffectType.For("REWARD"));
    }
    
    [Test]
    public void ShouldThrowExceptionWhenInvalidStatus()
    {
        FluentActions.Invoking(() => EffectType.For("INVALID"))
            .Should().Throw<InvalidOperationException>();
    }
    
    [Test]
    public void ShouldCheckImplicitConversionToString()
    {
        string transactionType = EffectType.For("REWARD");
        transactionType.Should().Be("REWARD");
    }
    
    [Test]
    public void ShouldCheckValue()
    {
        var transactionType = EffectType.For("REWARD");
        transactionType.Value.Should().Be("REWARD");
    }
}
