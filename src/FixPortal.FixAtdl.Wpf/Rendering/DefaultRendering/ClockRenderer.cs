// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class ClockRenderer : IControlRenderer<Clock_t>
{
    public Type ControlType => typeof(Clock_t);

    public void Render(WpfXmlWriter writer, Clock_t control)
    {
        WpfControlRenderer.RenderLabelledControl<Clock_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.ControlsNamespaceUri, typeof(TimePicker).Name))
                {
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate);
                    // Fixed width; revisit if this needs to self-size to content.
                    writer.WriteAttribute(WpfXmlWriterAttribute.Width, "75");
                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Time, "{Binding Path=Value, Mode=TwoWay}");
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.IsContentValid,
                        "{Binding Path=IsContentValid, Mode=OneWayToSource}"
                    );
                }
            }
        );
    }
}
