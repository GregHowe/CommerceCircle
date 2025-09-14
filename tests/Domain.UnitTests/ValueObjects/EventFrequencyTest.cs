using FluentAssertions;
using N1coLoyalty.Domain.Enums;
using N1coLoyalty.Domain.ValueObjects;
using NUnit.Framework;

namespace N1coLoyalty.Domain.UnitTests.ValueObjects
{

    public class EventFrequencyTest
    {
        [Test]
        public void ShouldHaveCorrectEventFrequencyForDAILY()
        {
            var frequenceDaily = EventFrequency.For("Daily");
            frequenceDaily.Frequency.Should().Be(FrequencyValue.Daily);
        }

        [Test]
        public void ShouldHaveCorrectEventFrequencyForMONTHLY()
        {
            var frequenceDaily = EventFrequency.For("Monthly");
            frequenceDaily.Frequency.Should().Be(FrequencyValue.Monthly);
        }

        [Test]
        public void ShouldBeEquivalent()
        {
            EventFrequency.For("Daily").Should().Be(EventFrequency.For("Daily"));
            EventFrequency.For("Monthly").Should().Be(EventFrequency.For("Monthly"));
        }

        [Test]
        public void ShouldCheckInequality()
        {
            (EventFrequency.For("Daily") != EventFrequency.For("Monthly")).Should().BeTrue();
        }

        [Test]
        public void ShouldThrowExceptionWhenInvalidStatus()
        {
            FluentActions.Invoking(() => EventFrequency.For("INVALID"))
           .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ShouldCheckImplicitConversionToString()
        {
            string subtype = EventFrequency.For("Monthly");
            subtype.Should().Be("Monthly");
        }

        [Test]
        public void ShouldCheckValue()
        {
            var eventFrequency = EventFrequency.For("Daily");
            eventFrequency.Value.Should().Be("Daily");
        }


    }
}
