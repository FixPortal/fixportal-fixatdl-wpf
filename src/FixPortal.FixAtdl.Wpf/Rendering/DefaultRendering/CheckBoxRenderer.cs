// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class CheckBoxRenderer : IControlRenderer<CheckBox_t>
{
    public Type ControlType => typeof(CheckBox_t);

    public void Render(WpfXmlWriter writer, CheckBox_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);
        using (writer.New(WpfXmlWriterTag.CheckBox))
        {
            WpfControlRenderer.WriteGridAttribute(writer, control);
            writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "1,8,2,3");
            if (!string.IsNullOrEmpty(control.Label))
            {
                writer.WriteLiteralAttribute(WpfXmlWriterAttribute.Content, control.Label);
            }
            if (!string.IsNullOrEmpty(control.Id))
            {
                writer.WriteAttribute(WpfXmlWriterAttribute.Name, id);
            }
            writer.WriteAttribute(
                WpfXmlWriterAttribute.DataContext,
                string.Format("{{Binding Path=Controls[{0}]}}", writer.ControlIndex(control))
            );
            writer.WriteAttribute(WpfXmlWriterAttribute.ToolTip, "{Binding Path=ToolTip, Mode=OneWay}");
            writer.WriteAttribute(WpfXmlWriterAttribute.IsChecked, "{Binding Path=Value, Mode=TwoWay}");
            writer.WriteAttribute(WpfXmlWriterAttribute.IsEnabled, "{Binding Path=Enabled, Mode=OneWay}");
            writer.WriteAttribute(WpfXmlWriterAttribute.Visibility, "{Binding Path=Visibility, Mode=OneWay}");
        }
    }
}
