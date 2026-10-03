// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class LabelRenderer : IControlRenderer<Label_t>
{
    public Type ControlType => typeof(Label_t);

    public void Render(WpfXmlWriter writer, Label_t control)
    {
        string id = WpfControlRenderer.CleanName(control.Id);
        using (writer.New(WpfXmlWriterTag.Label))
        {
            writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "2,5,2,5");
            WpfControlRenderer.WriteGridAttribute(writer, control);
            // nosemgrep: fixatdl-wpf-venue-text-requires-literal-write -- internal markup constants: the format produces a {Binding ...} path, venue text arrives as the bound VALUE, never as markup.
            writer.WriteAttribute(
                WpfXmlWriterAttribute.ToolTip,
                string.Format("{{Binding Path=Controls[{0}].ToolTip}}", writer.ControlIndex(control))
            );
            // nosemgrep: fixatdl-wpf-venue-text-requires-literal-write -- internal markup constant: {Binding ...} path; the label's venue text is the bound Value, evaluated as content, not markup.
            writer.WriteAttribute(
                WpfXmlWriterAttribute.Content,
                string.Format("{{Binding Path=Controls[{0}].Value}}", writer.ControlIndex(control))
            );
            writer.WriteAttribute(
                WpfXmlWriterAttribute.IsEnabled,
                string.Format("{{Binding Path=Controls[{0}].Enabled}}", writer.ControlIndex(control))
            );
            writer.WriteAttribute(
                WpfXmlWriterAttribute.Visibility,
                string.Format("{{Binding Path=Controls[{0}].Visibility}}", writer.ControlIndex(control))
            );
            writer.WriteAttribute(WpfXmlWriterAttribute.AutomationProperties_AutomationId, id);
        }
    }
}
