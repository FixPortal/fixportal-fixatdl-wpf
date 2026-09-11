using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class MultiSelectListRenderer : IControlRenderer<MultiSelectList_t>
{
    public Type ControlType => typeof(MultiSelectList_t);

    // FP Enhancement: the original Atdl4netConfiguration.Settings.Wpf.View.AutoSizeDropDowns switch
    // has no equivalent in the current core, so the combo-box-sizer registration call is dropped.
    public void Render(WpfXmlWriter writer, MultiSelectList_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);

        WpfControlRenderer.RenderLabelledControl<MultiSelectList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(WpfXmlWriterTag.ListBox))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                    writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());

                    writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,3,1,3");

                    if (!string.IsNullOrEmpty(id))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
                    }

                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.DataContext,
                        string.Format("{{Binding Path=Controls[{0}]}}", id)
                    );

                    writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=ListItems}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.SelectionMode, "Multiple");
                    writer.WriteAttribute(WpfXmlWriterAttribute.VirtualizingStackPanel_IsVirtualizing, "False");
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.ItemContainerStyle,
                        "{DynamicResource MultiSelectListItemStyle}"
                    );
                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, "{Binding Path=Enabled, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, "{Binding Path=Visibility, Mode=OneWay}");
                }
            }
        );
    }
}
