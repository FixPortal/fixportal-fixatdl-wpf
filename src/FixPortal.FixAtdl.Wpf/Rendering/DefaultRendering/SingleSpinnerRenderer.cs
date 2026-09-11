using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class SingleSpinnerRenderer : IControlRenderer<SingleSpinner_t>
{
    public Type ControlType => typeof(SingleSpinner_t);

    public void Render(WpfXmlWriter writer, SingleSpinner_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);
        WpfControlRenderer.RenderLabelledControl<SingleSpinner_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.Atdl4netNamespaceUri, typeof(SingleSpinner).Name))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,3,3,3");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Width, "120");
                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");
                    if (!string.IsNullOrEmpty(c.Id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", id)
                    );
                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.Value,
                        "{Binding Path=UiValue, Mode=TwoWay, TargetNullValue={x:Static sys:String.Empty}}"
                    );
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.Increment,
                        "{Binding Path=UnderlyingControl.Increment, Mode=OneWay, TargetNullValue=1}"
                    );
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
