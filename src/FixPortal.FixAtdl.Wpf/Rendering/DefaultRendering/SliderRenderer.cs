// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class SliderRenderer : IControlRenderer<Slider_t>
{
    public Type ControlType => typeof(Slider_t);

    public void Render(WpfXmlWriter writer, Slider_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);
        WpfControlRenderer.RenderLabelledControl<Slider_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (
                    writer.New(
                        DefaultNamespaceProvider.ControlsNamespaceUri,
                        control.ListItems.Count == 0 ? nameof(NumericSlider) : nameof(Slider)
                    )
                )
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());
                    if (!string.IsNullOrEmpty(c.Id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }
                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "2,5,2,5");
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", writer.ControlIndex(control))
                    );
                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    if (control.ListItems.Count == 0)
                    {
                        writer.WriteAttribute("Value", "{Binding Path=Value, Mode=TwoWay}");
                        writer.WriteAttribute("Minimum", "{Binding Path=NumericMinimum}");
                        writer.WriteAttribute("Maximum", "{Binding Path=NumericMaximum}");
                        writer.WriteAttribute(
                            "Increment",
                            "{Binding Path=UnderlyingControl.Increment, TargetNullValue=1}"
                        );
                    }
                    else
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                        writer.WriteAttribute(
                            WpfXmlWriterAttribute.SelectedValue,
                            "{Binding Path=SelectedValue, Mode=TwoWay}"
                        );
                    }
                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, "{Binding Path=Enabled, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, "{Binding Path=Visibility, Mode=OneWay}");
                }
            }
        );
    }
}
