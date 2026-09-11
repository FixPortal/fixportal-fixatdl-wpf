using System.Windows;
using AwesomeAssertions;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// WPF control/resource smoke tests. No headless test harness exists for WPF (see
/// scaffold-desktop's Testability guidance), so tests that touch WPF types run on an
/// explicit STA thread rather than the default MTA test-runner thread.
/// </summary>
public class ResourceDictionaryLoadTests
{
    [Fact]
    public void ResourceDictionary_ConstructsAndAcceptsEntries_OnStaThread()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var dictionary = new ResourceDictionary { { "smoke", "value" } };
                dictionary["smoke"].Should().Be("value");
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        failure.Should().BeNull();
    }
}
