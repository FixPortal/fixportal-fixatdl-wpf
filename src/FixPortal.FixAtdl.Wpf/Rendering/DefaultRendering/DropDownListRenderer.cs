using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class DropDownListRenderer : IControlRenderer<DropDownList_t>
{
    public Type ControlType => typeof(DropDownList_t);

    public void Render(WpfXmlWriter writer, DropDownList_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);

        WpfControlRenderer.RenderLabelledControl<DropDownList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(WpfXmlWriterTag.ComboBox))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());

                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,3,1,3");

                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");

                    if (!string.IsNullOrEmpty(id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }

                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", writer.ControlIndex(control))
                    );

                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.SelectedValue, "{Binding Path=SelectedValue}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.SelectedValuePath, "EnumId");
                    writer.WriteAttribute(WpfXmlWriterAttribute.DisplayMemberPath, "UiRep");
                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, "{Binding Path=Enabled, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, "{Binding Path=Visibility, Mode=OneWay}");
                }
            }
        );
    }
}
