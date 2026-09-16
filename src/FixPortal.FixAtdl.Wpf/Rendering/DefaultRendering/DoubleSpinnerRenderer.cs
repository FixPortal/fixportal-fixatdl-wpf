// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class DoubleSpinnerRenderer : IControlRenderer<DoubleSpinner_t>
{
    public Type ControlType => typeof(DoubleSpinner_t);

    public void Render(WpfXmlWriter writer, DoubleSpinner_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);
        WpfControlRenderer.RenderLabelledControl<DoubleSpinner_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.ControlsNamespaceUri, typeof(DoubleSpinner).Name))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.Width, "120");
                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,3,3,3");
                    if (!string.IsNullOrEmpty(c.Id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", writer.ControlIndex(control))
                    );
                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Value, "{Binding Path=Value, Mode=TwoWay}");
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.InnerIncrement,
                        "{Binding Path=UnderlyingControl.InnerIncrement, Mode=OneWay, TargetNullValue=1}"
                    );
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.OuterIncrement,
                        "{Binding Path=UnderlyingControl.OuterIncrement, Mode=OneWay, TargetNullValue=0.01}"
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
