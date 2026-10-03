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
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate);
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
                }
            }
        );
    }
}
