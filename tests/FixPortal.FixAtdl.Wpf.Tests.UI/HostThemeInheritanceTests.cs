using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Wpf.Controls;
using FixPortal.FixAtdl.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// The panel's resource dictionary is merged into the innermost resource scope in its tree, so an
/// implicit style there for a type the host owns shadows the host's own implicit style entirely -
/// every property, not only the one a trigger touches. Measured under WPF's Fluent dark theme, a
/// shadowed <see cref="ComboBox"/> rendered the legacy template (light chrome, black text) while its
/// items kept Fluent's near-white foreground: an unreadable dropdown. These tests are the standing
/// guard against that style reappearing.
/// </summary>
public class HostThemeInheritanceTests
{
    private static ResourceDictionary LoadPanelDictionary() =>
        new() { Source = new Uri("/FixPortal.FixAtdl.Wpf;component/FixAtdlWpfResources.xaml", UriKind.Relative) };

    [Fact]
    public void The_panel_dictionary_declares_no_implicit_style_for_a_type_the_host_owns()
    {
        StaTestHarness.Run(() =>
        {
            var owned = typeof(ErrorCue).Assembly;

            var foreign = LoadPanelDictionary()
                .Keys.OfType<Type>()
                .Where(key => key.Assembly != owned)
                .Select(key => key.FullName)
                .ToList();

            foreign
                .Should()
                .BeEmpty(
                    "an implicit style keyed on a host type shadows the host's own style for that type; "
                        + "state-dependent chrome belongs on ErrorCue, which sets one property and clears it"
                );
        });
    }

    [Theory]
    [InlineData(typeof(ComboBox))]
    [InlineData(typeof(ListBox))]
    public void A_rendered_control_gets_the_same_template_as_the_hosts_own(Type controlType)
    {
        StaTestHarness.Run(() =>
        {
            var (view, _) = RenderSample();
            var reference = (Control)Activator.CreateInstance(controlType)!;
            var root = new StackPanel();
            root.Children.Add(reference);
            root.Children.Add(view);

#pragma warning disable WPF0001 // ThemeMode is evaluation-only in .NET 10; the sample suppresses it too.
            var window = new Window
            {
                Content = root,
                Width = 700,
                Height = 900,
                ThemeMode = ThemeMode.Dark,
            };
#pragma warning restore WPF0001
            try
            {
                window.Show();
                window.UpdateLayout();

                var rendered = (Control)Descendants(view).First(child => child.GetType() == controlType);

                // Both templates are read in the same layout pass, so the comparison cannot straddle
                // a theme-dictionary change made by another test on another STA thread.
                reference.ApplyTemplate();
                rendered.ApplyTemplate();

                rendered
                    .Template.Should()
                    .BeSameAs(
                        reference.Template,
                        "the panel must render the host's own {0}, not a shadowed fallback",
                        controlType.Name
                    );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void A_rendered_text_field_gets_the_hosts_TextBox_template()
    {
        StaTestHarness.Run(() =>
        {
            var (view, _) = RenderSample();
            var reference = new TextBox();
            var root = new StackPanel();
            root.Children.Add(reference);
            root.Children.Add(view);

#pragma warning disable WPF0001
            var window = new Window
            {
                Content = root,
                Width = 700,
                Height = 1200,
                ThemeMode = ThemeMode.Dark,
            };
#pragma warning restore WPF0001
            try
            {
                window.Show();
                window.UpdateLayout();

                // ClickSelectTextBox derives from TextBox, and WPF matches an implicit style on the
                // element's EXACT type - so without help it never picks up a host's TextBox style
                // and renders light chrome inside a dark host.
                var rendered = Descendants(view).OfType<ClickSelectTextBox>().ToList();
                rendered.Should().NotBeEmpty("the sample has text fields, spinners and a clock");

                reference.ApplyTemplate();
                foreach (var box in rendered)
                {
                    box.ApplyTemplate();
                    box.Template.Should().BeSameAs(reference.Template);
                    box.Background.Should().BeSameAs(reference.Background);
                    box.Foreground.Should().BeSameAs(reference.Foreground);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void The_invalid_state_cue_hands_the_border_back_to_the_host_when_it_clears()
    {
        StaTestHarness.Run(() =>
        {
            var (view, _) = RenderSample();
#pragma warning disable WPF0001
            var window = new Window
            {
                Content = view,
                Width = 700,
                Height = 900,
                ThemeMode = ThemeMode.Dark,
            };
#pragma warning restore WPF0001
            try
            {
                window.Show();
                window.UpdateLayout();

                var combo = Descendants(view).OfType<ComboBox>().First();
                Brush themed = combo.BorderBrush;

                ErrorCue.SetHasErrors(combo, true);
                combo.BorderBrush.Should().BeOfType<SolidColorBrush>().Which.Color.Should().Be(Colors.Red);

                ErrorCue.SetHasErrors(combo, false);
                combo.BorderBrush.Should().BeSameAs(themed, "clearing the cue must not leave a value behind");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData("c_maxpct", "c_limit")]
    [InlineData("c_start", "c_end")]
    public void The_invalid_state_cue_reaches_a_controls_nested_text_boxes(string invalidId, string validId)
    {
        StaTestHarness.Run(() =>
        {
            var (view, model) = RenderSample();
            var window = new Window
            {
                Content = view,
                Width = 700,
                Height = 1200,
            };
            try
            {
                window.Show();
                window.UpdateLayout();

                var invalid = model.Controls.First(control => control.Id == invalidId);
                invalid.Value = "99999";
                invalid.HasErrors.Should().BeTrue("the theory's first id must name a control this value breaks");
                window.UpdateLayout();

                // A spinner's and a clock's text boxes live inside the composite's own XAML, so no
                // renderer can reach them - they carry ErrorCue themselves, off the DataContext the
                // renderer set on the composite. Before that, deleting the implicit
                // ClickSelectTextBox style silently took the cue away from every one of them.
                BoxesOf(view, model, invalidId)
                    .Should()
                    .NotBeEmpty()
                    .And.OnlyContain(box => ((SolidColorBrush)box.Foreground).Color == Colors.Red);

                BoxesOf(view, model, validId)
                    .Should()
                    .NotBeEmpty()
                    .And.OnlyContain(box => ((SolidColorBrush)box.Foreground).Color != Colors.Red);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static List<ClickSelectTextBox> BoxesOf(
        DependencyObject view,
        FixPortal.FixAtdl.Wpf.Core.ViewModels.EditViewModel model,
        string controlId
    )
    {
        var viewModel = model.Controls.First(control => control.Id == controlId);
        var composite = Descendants(view)
            .OfType<FrameworkElement>()
            .First(element =>
                element is SingleSpinner or DoubleSpinner or TimePicker
                && ReferenceEquals(element.DataContext, viewModel)
            );
        return Descendants(composite).OfType<ClickSelectTextBox>().ToList();
    }

    [Fact]
    public void The_shipped_sample_opens_with_no_control_in_error()
    {
        StaTestHarness.Run(() =>
        {
            var (_, model) = RenderSample();

            model
                .Controls.Where(control => control.HasErrors)
                .Select(control => control.Id)
                .Should()
                .BeEmpty("a sample that opens showing validation errors is a bad first impression");
        });
    }

    private static (FrameworkElement View, FixPortal.FixAtdl.Wpf.Core.ViewModels.EditViewModel Model) RenderSample()
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Strategies", "sample-strategy.xml"));
        Strategy_t strategy = new StrategiesReader().Load(stream).Strategies[0];
        strategy.LoadInitialControlValues(FixPortal.FixAtdl.Fix.FixFieldValueProvider.Empty);
        // Rendering is synchronous and the returned view holds no reference to the provider, so it
        // is disposed here rather than leaked to five call sites.
        using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
        return AtdlPanel.Create(strategy, services);
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}
