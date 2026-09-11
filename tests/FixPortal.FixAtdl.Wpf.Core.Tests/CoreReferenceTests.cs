using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Elements;
using Xunit;

namespace FixPortal.FixAtdl.Wpf.Core.Tests;

public class CoreReferenceTests
{
    [Fact]
    public void CanReferenceStrategyType()
    {
        var strategyType = typeof(Strategy_t);

        strategyType.Should().NotBeNull();
    }
}
