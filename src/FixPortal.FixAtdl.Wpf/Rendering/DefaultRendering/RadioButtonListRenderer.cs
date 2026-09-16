// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class RadioButtonListRenderer : IControlRenderer<RadioButtonList_t>
{
    public Type ControlType => typeof(RadioButtonList_t);

    public void Render(WpfXmlWriter writer, RadioButtonList_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);
        WpfControlRenderer.RenderLabelledControl<RadioButtonList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.ControlsNamespaceUri, typeof(RadioButtonList).Name))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,3,1,3");
                    if (!string.IsNullOrEmpty(c.Id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", writer.ControlIndex(control))
                    );
                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Orientation, "{Binding Path=Orientation, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, "{Binding Path=Enabled, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, "{Binding Path=Visibility, Mode=OneWay}");
                }
            }
        );
    }
}
