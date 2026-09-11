using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class EditableDropDownListRenderer : IControlRenderer<EditableDropDownList_t>
{
    public Type ControlType => typeof(EditableDropDownList_t);

    public void Render(WpfXmlWriter writer, EditableDropDownList_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);

        WpfControlRenderer.RenderLabelledControl<EditableDropDownList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(WpfXmlWriterTag.ComboBox))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());

                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,3,1,3");

                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEditable, "True");

                    if (!string.IsNullOrEmpty(id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }

                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");

                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", writer.ControlIndex(control))
                    );

                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.SelectedValuePath, "EnumId");
                    writer.WriteAttribute(WpfXmlWriterAttribute.DisplayMemberPath, "UiRep");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Text, "{Binding Path=Text, Mode=TwoWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, "{Binding Path=Enabled, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, "{Binding Path=Visibility, Mode=OneWay}");
                }
            }
        );
    }
}
