using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using FixPortal.FixAtdl.Fix;
using FixPortal.FixAtdl.Wpf.Core.ViewModels;
using FixPortal.FixAtdl.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Sample;

/// <summary>
/// The whole sample: load a strategy document, render it, read the FIX values back. Three calls into
/// the library and nothing else, so what appears on screen is the library's own output rather than
/// anything this host styled.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ServiceProvider _services;
    private readonly EditViewModel _model;

    public MainWindow()
    {
        InitializeComponent();

        // The declared height fits the sample strategy without scrolling; on a shorter screen, fit
        // the work area instead of hanging off the bottom of it.
        Height = Math.Min(Height, SystemParameters.WorkArea.Height - 40);

        _services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

        string path = Path.Combine(AppContext.BaseDirectory, "Strategies", "sample-strategy.xml");
        using FileStream stream = File.OpenRead(path);
        var strategy = new StrategiesReader().Load(stream).Strategies[0];

        // A freshly loaded strategy's controls hold no values yet. This is the step that applies each
        // control's initValue (and, where a control asks for one, a value from an inbound FIX field -
        // there is none here, so the provider is empty). AtdlPanel.Create deliberately does not do it
        // for you: it preserves whatever values the controls already carry, which is what makes an
        // amendment editor possible.
        strategy.LoadInitialControlValues(FixFieldValueProvider.Empty);

        (FrameworkElement view, EditViewModel model) = AtdlPanel.Create(strategy, _services);

        PanelHost.Content = view;
        _model = model;

        StatusText.Text = "Fields marked * are required.";
    }

    private void OnReadBack(object sender, RoutedEventArgs e)
    {
        if (_model.HasErrors)
        {
            StatusText.Text = "The strategy has validation errors - correct them before reading FIX values.";
            OutputText.Text = string.Join(Environment.NewLine, _model.StrategyErrors);
            return;
        }

        var builder = new StringBuilder();
        foreach ((int tag, string value) in _model.ReadBackFixValues().OrderBy(pair => pair.Key))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"{tag}={value}");
        }

        StatusText.Text = "Read back from the panel's current state.";
        OutputText.Text = builder.Length == 0 ? "(no parameters set)" : builder.ToString();
    }

    /// <summary>
    /// Switches the host's theme. The rendered panel is not touched and knows nothing about this:
    /// its chrome resolves through SystemColors, so it follows whatever the host is running. That is
    /// the whole point of the sample - watch the strategy form change with everything else.
    /// </summary>
    private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeSelector.SelectedItem is not ComboBoxItem { Content: string choice })
        {
            return;
        }

        Application.Current.ThemeMode = choice switch
        {
            "Light" => ThemeMode.Light,
            "Dark" => ThemeMode.Dark,
            _ => ThemeMode.System,
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        _services.Dispose();
        base.OnClosed(e);
    }
}
