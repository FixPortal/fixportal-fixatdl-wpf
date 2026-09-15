using System.Windows;
using System.Windows.Controls;
using AwesomeAssertions;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// Audit H4: the three public controls that override DefaultStyleKey (CheckBoxList, RadioButtonList,
/// StrategyPanelFrame) receive their templates only from the source assembly's theme chain
/// (ThemeInfo attribute in AssemblyInfo, Themes/Generic.xaml, and the merged
/// FixAtdlWpfResources.xaml). No in-panel test can detect a broken theme chain: the renderer's
/// per-view resource merge shadows the theme inside an AtdlPanel tree, so those controls resolve
/// their styles from view resources, never from the theme dictionary. These tests host each control
/// in a bare window - no panel tree, no view resources - and apply the template, forcing resolution
/// from the theme alone. (A truly detached ApplyTemplate does not consult the theme dictionaries and
/// returns a null template even for a correct theme; the bare presentation root is what makes the
/// implicit-style lookup reach them.)
/// </summary>
public class ThemeStandaloneTests
{
    [Theory]
    [InlineData(typeof(CheckBoxList))]
    [InlineData(typeof(RadioButtonList))]
    [InlineData(typeof(StrategyPanelFrame))]
    public void StandaloneControl_ResolvesThemeTemplate(Type controlType)
    {
        StaTestHarness.Run(() =>
        {
            var control = (Control)Activator.CreateInstance(controlType)!;
            var window = new Window
            {
                Content = control,
                Width = 200,
                Height = 100,
            };
            try
            {
                window.Show();

                control.ApplyTemplate();

                control.Template.Should().NotBeNull();
            }
            finally
            {
                window.Close();
            }
        });
    }
}
