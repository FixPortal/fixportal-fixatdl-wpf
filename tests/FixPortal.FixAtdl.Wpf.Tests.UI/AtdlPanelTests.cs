using System.Globalization;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FixPortal.FixAtdl.Wpf.Tests.UI;

/// <summary>
/// WPF controls require an STA thread; the test runner uses MTA threads.
/// </summary>
public partial class AtdlPanelTests
{
    [Theory]
    [InlineData(null, -1d, -50d, "-49")]
    [InlineData(150d, null, 175d, "176")]
    public void NumericSlider_OneSidedBoundsLeaveAnEditableRange(
        double? minimum,
        double? maximum,
        double loaded,
        string expectedWire
    )
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var parameter = new FixPortal.FixAtdl.Model.Elements.Parameter_t<FixPortal.FixAtdl.Model.Types.Float_t>(
                "Limit"
            )
            {
                FixTag = 9001,
            };
            parameter.Value.MinValue = (decimal?)minimum;
            parameter.Value.MaxValue = (decimal?)maximum;
            strategy.Parameters.Add(parameter);
            var control = new FixPortal.FixAtdl.Model.Controls.Slider_t("Limit") { ParameterRef = "Limit" };
            control.SetValue((decimal)loaded);
            strategy.StrategyLayout.StrategyPanel.Controls.Add(control);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var native = Descendants(view).OfType<System.Windows.Controls.Slider>().Single();

            System.Windows.Controls.Slider.IncreaseSmall.Execute(null, native);

            model.HasErrors.Should().BeFalse();
            model.ReadBackFixValues()[9001].Should().Be(expectedWire);
        });
    }

    [Fact]
    public void AmendmentPanel_DisablesImmutableControlAndPreservesItsWireValue()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.Controls["Qty"].SetValue(12m);
            strategy.Parameters["Qty"].MutableOnCxlRpl = false;
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services, isAmendment: true);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var spinner = Descendants(view).OfType<Controls.SingleSpinner>().Single();
            spinner.IsEnabled.Should().BeFalse();
            model.Controls[0].Value = 42m;
            model.ReadBackFixValues()[TestStrategies.QtyFixTag].Should().Be("12");
            spinner.Value.Should().Be(12m);
        });
    }

    [Theory]
    [InlineData(false, null, "11")]
    [InlineData(true, null, "0.11")]
    [InlineData(false, 0.25, "10.25")]
    [InlineData(true, 0.25, "0.1025")]
    public void NumericSlider_PreservesEmptyStateAndEditsWithinParameterBounds(
        bool percentage,
        double? increment,
        string expectedWire
    )
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Controls.Add(
                new FixPortal.FixAtdl.Model.Controls.Slider_t("Limit")
                {
                    ParameterRef = "Limit",
                    Increment = (decimal?)increment,
                }
            );
            FixPortal.FixAtdl.Model.Elements.Support.IParameter parameter;
            if (percentage)
            {
                var percent =
                    new FixPortal.FixAtdl.Model.Elements.Parameter_t<FixPortal.FixAtdl.Model.Types.Percentage_t>(
                        "Limit"
                    )
                    {
                        FixTag = 9001,
                    };
                percent.Value.MinValue = 0.1m;
                percent.Value.MaxValue = 0.2m;
                parameter = percent;
            }
            else
            {
                var number = new FixPortal.FixAtdl.Model.Elements.Parameter_t<FixPortal.FixAtdl.Model.Types.Float_t>(
                    "Limit"
                )
                {
                    FixTag = 9001,
                };
                number.Value.MinValue = 10m;
                number.Value.MaxValue = 20m;
                parameter = number;
            }
            strategy.Parameters.Add(parameter);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var slider = Descendants(view).OfType<Controls.NumericSlider>().Single();
            var native = Descendants(slider).OfType<System.Windows.Controls.Slider>().Single();
            slider.Minimum.Should().Be(10);
            slider.Maximum.Should().Be(20);
            model.Controls[1].Value.Should().BeNull();
            model.ReadBackFixValues().Should().NotContainKey(9001);
            System.Windows.Controls.Slider.IncreaseSmall.Execute(null, native);
            model.ReadBackFixValues()[9001].Should().Be(expectedWire);
            Descendants(slider)
                .OfType<System.Windows.Controls.Button>()
                .Single()
                .RaiseEvent(
                    new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)
                );
            model.Controls[1].Value.Should().BeNull();
            model.ReadBackFixValues().Should().NotContainKey(9001);
            model.Controls[1].Value = 25m;
            model.HasErrors.Should().BeTrue();
            model
                .Controls[1]
                .Value.Should()
                .Be(25m, "rendering a clamped thumb must not modify the loaded or supplied value");
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(FixPortal.FixAtdl.Model.Enumerations.IncrementPolicy_t.Tick)]
    [InlineData(FixPortal.FixAtdl.Model.Enumerations.IncrementPolicy_t.LotSize)]
    public void Spinner_UsesConfiguredIncrementAndRejectsValuesBeyondParameterBounds(
        FixPortal.FixAtdl.Model.Enumerations.IncrementPolicy_t? policy
    )
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var control = (FixPortal.FixAtdl.Model.Controls.SingleSpinner_t)strategy.Controls["Qty"];
            control.Increment = 0.25m;
            control.IncrementPolicy = policy;
            control.SetValue(1m);
            var parameter = (FixPortal.FixAtdl.Model.Elements.Parameter_t<FixPortal.FixAtdl.Model.Types.Float_t>)
                strategy.Parameters["Qty"];
            parameter.Value.MinValue = 1m;
            parameter.Value.MaxValue = 1.25m;
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var spinner = Descendants(view).OfType<Controls.SingleSpinner>().Single();
            var up = (System.Windows.Controls.Primitives.RepeatButton)spinner.FindName("upButton");
            var down = (System.Windows.Controls.Primitives.RepeatButton)spinner.FindName("downButton");
            up.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            model.ReadBackFixValues()[TestStrategies.QtyFixTag].Should().Be("1.25");
            up.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            model.HasErrors.Should().BeTrue();
            var read = () => model.ReadBackFixValues();
            read.Should().Throw<InvalidOperationException>();
            down.RaiseEvent(
                new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)
            );
            model.HasErrors.Should().BeFalse();
            spinner.Text = "0.75";
            model.HasErrors.Should().BeTrue();
        });
    }

    [Theory]
    [InlineData(2025, 12, 24, 15, "20251224-15:30:45")]
    [InlineData(2026, 11, 1, 6, "20261101-06:30:45")]
    public void Clock_RenderPreservesLoadedTimestampAndEditsUseInjectedMarketDate(
        int year,
        int month,
        int day,
        int hour,
        string loadedWire
    )
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var control = new FixPortal.FixAtdl.Model.Controls.Clock_t("Clock")
            {
                ParameterRef = "Clock",
                LocalMktTz = "America/New_York",
                Clock = new FixedClock(),
            };
            control.SetValue(new DateTime(year, month, day, hour, 30, 45, DateTimeKind.Utc));
            strategy.Parameters.Add(
                new FixPortal.FixAtdl.Model.Elements.Parameter_t<FixPortal.FixAtdl.Model.Types.UTCTimestamp_t>("Clock")
                {
                    FixTag = 9003,
                }
            );
            strategy.StrategyLayout.StrategyPanel.Controls.Add(control);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var picker = Descendants(view).OfType<Controls.TimePicker>().Single();
            picker.Hours.Should().Be(month == 11 ? "01" : "10");
            model.Controls[1].GetErrors().Should().BeEmpty("the loaded clock must remain valid");
            model.ReadBackFixValues()[9003].Should().Be(loadedWire);
            picker.Minutes = "31";
            model.Controls[1].GetErrors().Should().BeEmpty("the edited time must be anchored by the core");
            model.ReadBackFixValues()[9003].Should().Be(month == 11 ? "20260101-06:31:00" : "20260101-15:31:00");
        });
    }

    private sealed class FixedClock : NodaTime.IClock
    {
        public NodaTime.Instant GetCurrentInstant() => NodaTime.Instant.FromUtc(2026, 1, 2, 1, 0);
    }

    [Theory]
    [InlineData("RadioButton", "")]
    [InlineData("CheckBoxList", "")]
    [InlineData("RadioButtonList", "")]
    [InlineData("RadioButton", "Qty")]
    [InlineData("CheckBoxList", "Qty")]
    [InlineData("RadioButtonList", "Qty")]
    public void DirectRenderer_RejectsInvalidControlIdentities(string kind, string id)
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            FixPortal.FixAtdl.Model.Elements.Control_t control = kind switch
            {
                "RadioButton" => new FixPortal.FixAtdl.Model.Controls.RadioButton_t("Other"),
                "CheckBoxList" => new FixPortal.FixAtdl.Model.Controls.CheckBoxList_t("Other"),
                _ => new FixPortal.FixAtdl.Model.Controls.RadioButtonList_t("Other"),
            };
            strategy.StrategyLayout.StrategyPanel.Controls.Add(control);
            control.Id = id;
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var render = () =>
                services.GetRequiredService<Rendering.StrategyPanelRenderer>().Render(strategy, services);
            render.Should().Throw<ArgumentException>().WithMessage("*nonempty and unique*");
        });
    }

    [Fact]
    public void Clock_RemainsInvalidUntilBothFieldsAreValid()
    {
        RunOnSta(() =>
        {
            var picker = new Controls.TimePicker { Hours = "10", Minutes = "30" };
            picker.Hours = "bad";
            picker.Minutes = "31";
            picker.IsContentValid.Should().BeFalse();
            picker.Hours = "11";
            picker.IsContentValid.Should().BeTrue();
        });
    }

    [Fact]
    public void Clock_ClearOrExternalValueRemovesStaleValidation()
    {
        RunOnSta(() =>
        {
            var picker = new Controls.TimePicker { Hours = "10", Minutes = "30" };
            picker.Hours = "bad";
            picker.Minutes = "";
            picker.Hours.Should().Be("bad");
            picker.IsContentValid.Should().BeFalse();
            picker.Hours = "";
            picker.Hours.Should().BeEmpty();
            picker.IsContentValid.Should().BeTrue();
            picker.Hours = "bad";
            picker.Time = new DateTime(2026, 9, 11, 12, 30, 0, DateTimeKind.Utc);
            picker.Hours.Should().Be("12");
            picker.Minutes.Should().Be("30");
            picker.IsContentValid.Should().BeTrue();
        });
    }

    [Fact]
    public void RadioGroups_AreMutuallyExclusiveAndScopedToThePanel()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Controls.Add(
                new FixPortal.FixAtdl.Model.Controls.RadioButton_t("A") { RadioGroup = "Group" }
            );
            strategy.StrategyLayout.StrategyPanel.Controls.Add(
                new FixPortal.FixAtdl.Model.Controls.RadioButton_t("B") { RadioGroup = "Group" }
            );
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var buttons = Descendants(view).OfType<System.Windows.Controls.RadioButton>().ToArray();
            buttons[0].GroupName.Should().NotBeEmpty().And.Be(buttons[1].GroupName);
            buttons[0].SetCurrentValue(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, true);
            buttons[1].SetCurrentValue(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, true);
            buttons[0].IsChecked.Should().BeFalse();
            model.Controls[1].Value.Should().Be(false);
            model.Controls[2].Value.Should().Be(true);
            var (otherView, _) = AtdlPanel.Create(strategy, services);
            Descendants(otherView)
                .OfType<System.Windows.Controls.RadioButton>()
                .First()
                .GroupName.Should()
                .NotBe(buttons[0].GroupName);
        });
    }

    [Fact]
    public void DuplicateIds_AreRejectedBeforeXamlParsing()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var duplicate = new FixPortal.FixAtdl.Model.Controls.CheckBox_t("Other");
            strategy.StrategyLayout.StrategyPanel.Controls.Add(duplicate);
            duplicate.Id = "Qty";
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var create = () => AtdlPanel.Create(strategy, services);
            create.Should().Throw<ArgumentException>().WithMessage("*unique*Qty*");
        });
    }

    [Fact]
    public void ClockEdits_KeepBindingAndUseTimeOnlyBoundary()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Controls.Add(new FixPortal.FixAtdl.Model.Controls.Clock_t("Clock"));
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var picker = Descendants(view).OfType<Controls.TimePicker>().Single();
            picker.Hours = "10";
            picker.Minutes = "30";
            picker.Time.Should().Be(new DateTime(1, 1, 1, 10, 30, 0, DateTimeKind.Unspecified));
            model.Controls[1].Value.Should().Be(picker.Time);
            System
                .Windows.Data.BindingOperations.IsDataBound(picker, Controls.TimePicker.TimeProperty)
                .Should()
                .BeTrue();
            picker.Minutes = "31";
            model.Controls[1].Value.Should().Be(picker.Time);
        });
    }

    [Fact]
    public void SpinnerTextEdits_PreserveBindingAndReportInvalidInput()
    {
        RunOnSta(() =>
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(TestStrategies.MinimalOneControlStrategy(), services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var spinner = Descendants(view).OfType<Controls.SingleSpinner>().Single();
            var input = (System.Windows.Controls.TextBox)spinner.FindName("value");
            input.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, "12.5");
            model.Controls[0].Value.Should().Be(12.5m);
            input.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, "bad");
            model.HasErrors.Should().BeTrue();
            input.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, "43");
            model.HasErrors.Should().BeFalse();
            model.Controls[0].Value.Should().Be(43m);
            System
                .Windows.Data.BindingOperations.IsDataBound(spinner, Controls.NumericSpinnerControlBase.ValueProperty)
                .Should()
                .BeTrue();
        });
    }

    [Fact]
    public void CorrectingSpinnerText_RefreshesStrategyValidation()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyEdits.Add(
                new FixPortal.FixAtdl.Model.Elements.StrategyEdit_t
                {
                    ErrorMessage = "Must exceed 20",
                    Edit =
                        new FixPortal.FixAtdl.Model.Elements.Edit_t<FixPortal.FixAtdl.Model.Elements.Support.IParameter>
                        {
                            Field = "Qty",
                            Operator = FixPortal.FixAtdl.Model.Enumerations.Operator_t.GreaterThan,
                            Value = "20",
                        },
                }
            );
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var spinner = Descendants(view).OfType<Controls.SingleSpinner>().Single();
            spinner.Text = "bad";
            spinner.Text = "21";
            model.HasErrors.Should().BeFalse();
        });
    }

    [Theory]
    [InlineData("CheckBoxList")]
    [InlineData("RadioButtonList")]
    [InlineData("DropDownList")]
    [InlineData("EditableDropDownList")]
    [InlineData("SingleSelectList")]
    [InlineData("MultiSelectList")]
    [InlineData("Slider")]
    public void ListControls_RenderAndBind(string kind)
    {
        RunOnSta(() =>
        {
            FixPortal.FixAtdl.Model.Controls.Support.ListControlBase control = kind switch
            {
                "CheckBoxList" => new FixPortal.FixAtdl.Model.Controls.CheckBoxList_t("List"),
                "RadioButtonList" => new FixPortal.FixAtdl.Model.Controls.RadioButtonList_t("List"),
                "DropDownList" => new FixPortal.FixAtdl.Model.Controls.DropDownList_t("List"),
                "EditableDropDownList" => new FixPortal.FixAtdl.Model.Controls.EditableDropDownList_t("List"),
                "SingleSelectList" => new FixPortal.FixAtdl.Model.Controls.SingleSelectList_t("List"),
                "MultiSelectList" => new FixPortal.FixAtdl.Model.Controls.MultiSelectList_t("List"),
                _ => new FixPortal.FixAtdl.Model.Controls.Slider_t("List"),
            };
            control.ListItems.Add(new FixPortal.FixAtdl.Model.Elements.ListItem_t { EnumId = "A", UiRep = "Alpha" });
            control.ListItems.Add(new FixPortal.FixAtdl.Model.Elements.ListItem_t { EnumId = "B", UiRep = "Beta" });
            control.InitValue = "B";
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Controls.Add(control);
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            var list = model.Controls[1].Should().BeOfType<Core.ViewModels.ListControlViewModel>().Subject;
            var element = Descendants(view)
                .OfType<System.Windows.FrameworkElement>()
                .First(e => e.Name == Rendering.WpfControlRenderer.CleanName("List"));
            element.DataContext.Should().BeSameAs(list);
            if (element is Controls.Slider slider)
            {
                slider.ItemsSource.Should().HaveCount(2);
                slider.SelectedValue.Should().Be("B");
                slider.SelectedIndex = 0;
            }
            else if (
                element is System.Windows.Controls.Primitives.Selector selector
                && element is not Controls.MultiButtonControlBase
            )
            {
                selector.Items.Count.Should().Be(2);
                if (kind != "MultiSelectList")
                {
                    selector.SelectedValue = "A";
                }
                else
                {
                    (
                        (System.Windows.Controls.ListBoxItem)selector.ItemContainerGenerator.ContainerFromIndex(0)
                    ).IsSelected = true;
                }
            }
            else
            {
                var items = (System.Windows.Controls.ItemsControl)element;
                items.Items.Count.Should().Be(2);
                System.Windows.Media.VisualTreeHelper.GetChildrenCount(items).Should().BeGreaterThan(0);
                list.Items[0].IsSelected = true;
            }
            list.Items[0].IsSelected.Should().BeTrue();
            if (kind == "EditableDropDownList")
            {
                list.Text = "custom";
                list.Text.Should().Be("custom");
                ((System.Windows.Controls.ComboBox)element).Text.Should().Be("custom");
            }
        });
    }

    [Fact]
    public void Create_RendersContentAndBindsEdits()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (view, model) = AtdlPanel.Create(strategy, services);
            view.Measure(new System.Windows.Size(800, 600));
            view.Arrange(new System.Windows.Rect(0, 0, 800, 600));
            view.UpdateLayout();
            view.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.DataBind);

            System.Windows.Media.VisualTreeHelper.GetChildrenCount(view).Should().BeGreaterThan(0);
            var spinner = Descendants(view).OfType<Controls.SingleSpinner>().Single();
            spinner.DataContext.Should().BeSameAs(model.Controls[0]);
            spinner.SetCurrentValue(Controls.NumericSpinnerControlBase.ValueProperty, 42m);
            spinner.GetBindingExpression(Controls.NumericSpinnerControlBase.ValueProperty)!.UpdateSource();
            model.ReadBackFixValues().Should().ContainKey(TestStrategies.QtyFixTag).WhoseValue.Should().Be("42");
        });
    }

    private static IEnumerable<System.Windows.DependencyObject> Descendants(System.Windows.DependencyObject root)
    {
        foreach (
            var child in System.Windows.LogicalTreeHelper.GetChildren(root).OfType<System.Windows.DependencyObject>()
        )
        {
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    [Theory]
    [InlineData("{x:Static sys:Environment.MachineName}")]
    [InlineData("{}literal & <text>")]
    public void Create_TreatsVenueTextAsLiteral(string text)
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var panel = strategy.StrategyLayout.StrategyPanel;
            panel.Title = text;
            panel.Controls.Add(new FixPortal.FixAtdl.Model.Controls.CheckBox_t("Flag") { Label = text });
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (view, _) = AtdlPanel.Create(strategy, services);

            var frame = view.Should().BeOfType<Controls.StrategyPanelFrame>().Subject;
            frame.Header.Should().Be(text);
            var grid = frame.Content.Should().BeOfType<System.Windows.Controls.Grid>().Subject;
            grid.Children.OfType<System.Windows.Controls.CheckBox>().Single().Content.Should().Be(text);
        });
    }

    [Theory]
    [InlineData("Order Qty")]
    [InlineData("Order.Qty-1")]
    [InlineData("X, Source={x:Static sys:Environment.MachineName}")]
    public void Create_AcceptsIdsWithoutInterpretingThemAsXaml(string id)
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            strategy.StrategyLayout.StrategyPanel.Controls.Add(new FixPortal.FixAtdl.Model.Controls.CheckBox_t(id));
            using var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var create = () => AtdlPanel.Create(strategy, services);

            create.Should().NotThrow();
        });
    }

    [Fact]
    public void Create_ReturnsViewAndViewModelForStrategy()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();

            var (view, viewModel) = AtdlPanel.Create(strategy, services);

            view.Should().NotBeNull();
            viewModel.Controls.Should().HaveCount(1);
        });
    }

    [Fact]
    public void ReadBackFixValues_ReturnsEditedValueKeyedByTag()
    {
        RunOnSta(() =>
        {
            var strategy = TestStrategies.MinimalOneControlStrategy();
            var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (_, viewModel) = AtdlPanel.Create(strategy, services);

            viewModel.Controls[0].Value = 12.5m;
            var values = viewModel.ReadBackFixValues();

            values.Should().ContainKey(TestStrategies.QtyFixTag).WhoseValue.Should().Be("12.5");
        });
    }

    [Fact]
    public void ReadBackFixValues_UsesInvariantCulture_RegardlessOfCurrentCulture()
    {
        // Reading back via Value.ToString() with no explicit
        // culture would render 12.5m as "12,5" under a comma-decimal culture (de-DE), which is not a
        // valid FIX value and would corrupt the outbound message. Runs on a dedicated STA thread, so
        // setting its CurrentCulture does not leak to other tests.
        RunOnSta(() =>
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var strategy = TestStrategies.MinimalOneControlStrategy();
            var services = new ServiceCollection().AddFixAtdlWpf().BuildServiceProvider();
            var (_, viewModel) = AtdlPanel.Create(strategy, services);

            viewModel.Controls[0].Value = 12.5m;
            var values = viewModel.ReadBackFixValues();

            values.Should().ContainKey(TestStrategies.QtyFixTag).WhoseValue.Should().Be("12.5");
        });
    }

    private static void RunOnSta(Action action)
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
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
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
