using FluentAssertions;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;
using NUnit.Framework;

namespace N1coLoyalty.Domain.UnitTests.ValueObjects
{

    public class BenefitTypeTests
    {
        [Test]
        public void ShouldHaveCorrectBenefitTypeForShopAccess()
        {
            var benefitType = BenefitType.For("ShopAccess");
            benefitType.Type.Should().Be(BenefitTypeValue.ShopAccess);
        }

        [Test]
        public void ShouldHaveCorrectBenefitTypeForConcertDiscounts()
        {
            var benefitType = BenefitType.For("ConcertDiscounts");
            benefitType.Type.Should().Be(BenefitTypeValue.ConcertDiscounts);
        }
        
        [Test]
        public void ShouldHaveCorrectBenefitTypeForRewards()
        {
            var benefitType = BenefitType.For("Rewards");
            benefitType.Type.Should().Be(BenefitTypeValue.Rewards);
        }

        [Test]
        public void ShouldBeEquivalent()
        {
            BenefitType.For("ShopAccess").Should().Be(BenefitType.For("ShopAccess"));
            BenefitType.For("ConcertDiscounts").Should().Be(BenefitType.For("ConcertDiscounts"));
            BenefitType.For("Rewards").Should().Be(BenefitType.For("Rewards"));
        }

        [Test]
        public void ShouldCheckInequality()
        {
            (BenefitType.For("ShopAccess") != BenefitType.For("ConcertDiscounts")).Should().BeTrue();
        }

        [Test]
        public void ShouldThrowExceptionWhenInvalid()
        {
            FluentActions.Invoking(() => BenefitType.For("FreeDelivery"))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ShouldCheckImplicitConversionToString()
        {
            string type = BenefitType.For("ConcertDiscounts");
            type.Should().Be("ConcertDiscounts");
        }

        [Test]
        public void ShouldCheckValue()
        {
            var benefitType = BenefitType.For("ShopAccess");
            benefitType.Value.Should().Be("ShopAccess");
        }

    }
}

