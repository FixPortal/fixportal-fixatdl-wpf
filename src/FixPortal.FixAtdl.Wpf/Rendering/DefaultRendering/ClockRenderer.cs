using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class ClockRenderer : IControlRenderer<Clock_t>
{
    public Type ControlType => typeof(Clock_t);

    public void Render(WpfXmlWriter writer, Clock_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);
        WpfControlRenderer.RenderLabelledControl<Clock_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.Atdl4netNamespaceUri, typeof(TimePicker).Name))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());
                    // Fixed width; revisit if this needs to self-size to content.
                    writer.WriteAttribute(WpfXmlWriterAttribute.Width, "75");
                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,3,1,3");
                    if (!string.IsNullOrEmpty(c.Id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", id)
                    );
                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Time, "{Binding Path=UiValue, Mode=TwoWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, "{Binding Path=Enabled, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, "{Binding Path=Visibility, Mode=OneWay}");
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.IsContentValid,
                        "{Binding Path=IsContentValid, Mode=OneWayToSource}"
                    );
                }
            }
        );
    }
}
