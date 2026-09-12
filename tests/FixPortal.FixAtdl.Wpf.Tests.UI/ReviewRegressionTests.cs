using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AwesomeAssertions;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Model.Types;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

public partial class AtdlPanelTests
{
    [Theory]
    [InlineData("single", false)]
    [InlineData("double", false)]
    [InlineData("hours", false)]
    [InlineData("minutes", false)]
    [InlineData("single", true)]
    public void Spinner_ArrowKeyIsConsumedWithoutResurrectingHandledInput(string kind, bool alreadyHandled)
    {
        RunOnSta(() =>
        {
            FrameworkElement spinner = kind switch
            {
                "single" => new Controls.SingleSpinner(),
                "double" => new Controls.DoubleSpinner(),
                _ => new Controls.TimePicker(),
            };
            var field = kind is "hours" or "minutes" ? kind : "value";
            var input = (TextBox)spinner.FindName(field);
            using var source = new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "test", IntPtr.Zero);
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.Up)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
                Handled = alreadyHandled,
            };
            input.RaiseEvent(args);
            args.Handled.Should().BeTrue();
            if (alreadyHandled)
            {
                ((Controls.SingleSpinner)spinner).Value.Should().BeNull();
            }
            else if (spinner is Controls.NumericSpinnerControlBase numeric)
            {
                numeric.Value.Should().Be(1);
            }
            else
            {
                ((Controls.TimePicker)spinner).Time.Should().NotBeNull();
            }
        });
    }

    [Fact]
    public void Spinner_InvariantInputRejectsCommaDecimalUnderGermanCulture()
    {
        RunOnSta(() =>
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            // Reproduce the user's locale; the renderer deliberately keeps invariant input syntax.
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(TestStrategies.MinimalOneControlStrategy(), services);
            Layout(view);
            var spinner = Descendants(view).OfType<Controls.SingleSpinner>().Single();
            var input = (TextBox)spinner.FindName("value");
            input.SetCurrentValue(TextBox.TextProperty, "1,5");

            model.HasErrors.Should().BeTrue();
            spinner.Value.Should().BeNull();
            input.Text.Should().Be("1,5");
        });
    }

    [Theory]
    [InlineData("upButton", "downButton", true, true)]
    [InlineData("downButton", "upButton", true, false)]
    [InlineData("innerUpButton", "innerDownButton", false, true)]
    [InlineData("innerDownButton", "innerUpButton", false, false)]
    [InlineData("outerUpButton", "outerDownButton", false, true)]
    [InlineData("outerDownButton", "outerUpButton", false, false)]
    public void Spinner_OverflowKeepsPreviousValueAndCanStepBack(
        string buttonName,
        string reverseName,
        bool single,
        bool up
    )
    {
        RunOnSta(() =>
        {
            Controls.NumericSpinnerControlBase spinner = single
                ? new Controls.SingleSpinner()
                : new Controls.DoubleSpinner { InnerIncrement = 1m, OuterIncrement = 1m };
            var limit = up ? decimal.MaxValue : decimal.MinValue;
            spinner.Value = limit;
            var button = (RepeatButton)spinner.FindName(buttonName);
            var click = () => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            click.Should().NotThrow();
            spinner.Value.Should().Be(limit);
            ((RepeatButton)spinner.FindName(reverseName)).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            spinner.Value.Should().Be(up ? limit - 1 : limit + 1);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Clock_StartingOtherFieldReplacesInvalidEmptySibling(bool hoursFirst)
    {
        RunOnSta(() =>
        {
            var picker = new Controls.TimePicker();
            if (hoursFirst)
            {
                picker.Hours = "bad";
                picker.Minutes = "15";
            }
            else
            {
                picker.Minutes = "bad";
                picker.Hours = "15";
            }
            picker.IsContentValid.Should().BeTrue();
            picker.Time.Should().NotBeNull();
            (hoursFirst ? picker.Hours : picker.Minutes).Should().Be("00");
        });
    }

    [Theory]
    [InlineData("hours", "13", 13)]
    [InlineData("minutes", "45", 45)]
    public void Clock_TextInputAcceptsTwoDigits(string field, string text, int expected)
    {
        RunOnSta(() =>
        {
            var picker = new Controls.TimePicker();
            var window = new Window
            {
                Content = picker,
                Width = 250,
                Height = 100,
            };
            try
            {
                window.Show();
                var input = (TextBox)picker.FindName(field);
                input.Focus();
                input.SelectAll();
                foreach (var digit in text)
                {
                    input.RaiseEvent(
                        new TextCompositionEventArgs(
                            Keyboard.PrimaryDevice,
                            new TextComposition(InputManager.Current, input, digit.ToString())
                        )
                        {
                            RoutedEvent = TextCompositionManager.TextInputEvent,
                        }
                    );
                }
                input.Text.Should().Be(text);
                picker.IsContentValid.Should().BeTrue();
                (field == "hours" ? picker.Time!.Value.Hour : picker.Time!.Value.Minute).Should().Be(expected);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NumericSlider_EmptyMinimumCanBeCommitted(bool mouse)
    {
        RunOnSta(() =>
        {
            var slider = new Controls.NumericSlider { Minimum = 10, Maximum = 20 };
            var native = Descendants(slider).OfType<System.Windows.Controls.Slider>().Single();
            slider.Value.Should().BeNull();
            if (mouse)
            {
                native.RaiseEvent(
                    new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                    }
                );
            }
            else
            {
                System.Windows.Controls.Slider.MinimizeValue.Execute(null, native);
                using var source = new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "test", IntPtr.Zero);
                native.RaiseEvent(
                    new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.Home) { RoutedEvent = Keyboard.KeyUpEvent }
                );
            }
            slider.Value.Should().Be(10);
        });
    }

    [Fact]
    public void EnumeratedSlider_EmptyAndRuleClearHaveDistinctPosition()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var control = new Slider_t("List") { ParameterRef = "List" };
            control.ListItems.Add(new ListItem_t { EnumId = "A", UiRep = "Alpha" });
            control.ListItems.Add(new ListItem_t { EnumId = "B", UiRep = "Beta" });
            var parameter = new Parameter_t<String_t>("List") { FixTag = 9001 };
            parameter.EnumPairs.Add(new EnumPair_t { EnumId = "A", WireValue = "alpha" });
            parameter.EnumPairs.Add(new EnumPair_t { EnumId = "B", WireValue = "beta" });
            strategy.Parameters.Add(parameter);
            strategy.StrategyLayout.StrategyPanel.Controls.Add(control);
            control.StateRules.Add(
                new StateRule_t
                {
                    Value = "{NULL}",
                    Edit = new Edit_t<Control_t>
                    {
                        Field = "Qty",
                        Operator = Operator_t.GreaterThan,
                        Value = "20",
                    },
                }
            );
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            Layout(view);
            var slider = Descendants(view).OfType<Controls.Slider>().Single();
            var native = (System.Windows.Controls.Slider)slider.FindName("sliderControl");
            native.Value.Should().Be(-1);
            model.ReadBackFixValues().Should().NotContainKey(9001);
            System.Windows.Controls.Slider.IncreaseSmall.Execute(null, native);
            model.ReadBackFixValues()[9001].Should().Be("alpha");
            native.Value = 1;
            model.Controls[0].Value = 21m;
            slider.SelectedValue.Should().BeNull();
            native.Value.Should().Be(-1);
            model.ReadBackFixValues().Should().NotContainKey(9001);
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Parameters")]
    public void CollapsiblePanel_HasVisibleNamedToggle(string? title)
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Title = title!;
            strategy.StrategyLayout.StrategyPanel.Collapsible = true;
            strategy.StrategyLayout.StrategyPanel.Collapsed = true;
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(strategy, services);
            var window = new Window { Content = view };
            try
            {
                window.Show();
                Layout(view);
                var frame = (Controls.StrategyPanelFrame)view;
                var toggle = (ToggleButton)frame.Template.FindName("HeaderToggle", frame);
                toggle.IsVisible.Should().BeTrue();
                new ToggleButtonAutomationPeer(toggle).GetName().Should().NotBeNullOrWhiteSpace();
                frame.Header = string.Empty;
                new ToggleButtonAutomationPeer(toggle).GetName().Should().NotBeNullOrWhiteSpace();
                toggle.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
                frame.IsExpanded.Should().BeTrue();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void VerticalPanel_ColumnsAndHeaderlessPaddingAreStable(int count)
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var panel = strategy.StrategyLayout.StrategyPanel;
            panel.Title = null!;
            panel.Orientation = Orientation_t.Vertical;
            for (var i = 1; i < count; i++)
            {
                panel.Controls.Add(new TextField_t("Text" + i));
            }
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(strategy, services);
            Layout(view);
            var frame = (Controls.StrategyPanelFrame)view;
            var grid = (Grid)frame.Content;
            grid.ColumnDefinitions[0].Width.IsAuto.Should().BeTrue();
            grid.ColumnDefinitions[1].Width.IsStar.Should().BeTrue();
            frame.Padding.Top.Should().Be(8);
        });
    }

    [Theory]
    [InlineData(Orientation_t.Vertical)]
    [InlineData(Orientation_t.Horizontal)]
    public void NestedPanels_RenderWithoutExplicitParentLinks(Orientation_t orientation)
    {
        RunOnSta(() =>
        {
            var strategy = new Strategy_t();
            var root = new StrategyPanel_t(strategy) { Orientation = orientation };
            strategy.StrategyLayout = new StrategyLayout_t { StrategyPanel = root };
            root.StrategyPanels.Add(new StrategyPanel_t(strategy));
            root.StrategyPanels.Add(new StrategyPanel_t(strategy));
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(strategy, services);
            var grid = (Grid)((Controls.StrategyPanelFrame)view).Content;
            var children = grid.Children.OfType<Controls.StrategyPanelFrame>().ToArray();
            children.Should().HaveCount(2);
            (orientation == Orientation_t.Vertical ? Grid.GetRow(children[1]) : Grid.GetColumn(children[1]))
                .Should()
                .Be(1);
        });
    }

    [Fact]
    public void RepeatedRegistration_RendersSuccessfully()
    {
        RunOnSta(() =>
        {
            using var services = new ServiceCollection().AddFixAtdlWpf().AddFixAtdlWpf().BuildServiceProvider();
            var render = () => AtdlPanel.Create(TestStrategies.MinimalOneControlStrategy(), services);
            render.Should().NotThrow();
        });
    }

    [Fact]
    public void CustomRenderer_RegisteredLastOverridesDefault()
    {
        RunOnSta(() =>
        {
            using var services = new ServiceCollection()
                .AddFixAtdlWpf()
                .AddTransient<Rendering.IControlRenderer, CustomSpinnerRenderer>()
                .BuildServiceProvider();
            var (view, _) = AtdlPanel.Create(TestStrategies.MinimalOneControlStrategy(), services);
            Descendants(view).OfType<TextBlock>().Should().ContainSingle().Which.Text.Should().Be("Custom spinner");
        });
    }

    private sealed class CustomSpinnerRenderer : Rendering.IControlRenderer<SingleSpinner_t>
    {
        public Type ControlType => typeof(SingleSpinner_t);

        public void Render(Rendering.WpfXmlWriter writer, SingleSpinner_t control)
        {
            using (writer.New("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "TextBlock"))
            {
                writer.WriteAttribute(Rendering.WpfXmlWriterAttribute.Text, "Custom spinner");
            }
        }
    }

    [Fact]
    public void EditableDropdown_ReadBackIncludesTextBeforeFocusMoves()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Controls.Add(
                new EditableDropDownList_t("Text") { ParameterRef = "Text" }
            );
            strategy.Parameters.Add(new Parameter_t<String_t>("Text") { FixTag = 9001 });
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            var window = new Window { Content = view };
            try
            {
                window.Show();
                Layout(view);
                var combo = Descendants(view).OfType<ComboBox>().Single();
                var input = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);
                input.Focus();
                input.RaiseEvent(
                    new TextCompositionEventArgs(
                        Keyboard.PrimaryDevice,
                        new TextComposition(InputManager.Current, input, "custom")
                    )
                    {
                        RoutedEvent = TextCompositionManager.TextInputEvent,
                    }
                );
                input.IsKeyboardFocused.Should().BeTrue();
                model.ReadBackFixValues()[9001].Should().Be("custom");
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void Layout(FrameworkElement view)
    {
        view.Measure(new Size(800, 600));
        view.Arrange(new Rect(0, 0, 800, 600));
        view.UpdateLayout();
    }
}
