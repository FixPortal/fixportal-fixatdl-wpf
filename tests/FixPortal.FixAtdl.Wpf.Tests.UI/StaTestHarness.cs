using System.Windows.Threading;
using AwesomeAssertions;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// WPF controls require an STA thread; the test runner uses MTA threads. Runs an action on a
/// dedicated STA thread and fails the test if it does not complete within 30 seconds. The
/// thread's dispatcher is shut down in a finally block so every run releases its thread.
/// </summary>
internal static class StaTestHarness
{
    public static void Run(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join(TimeSpan.FromSeconds(30)).Should().BeTrue("WPF checks must finish within 30 seconds");

        if (failure != null)
        {
            throw failure;
        }
    }
}
