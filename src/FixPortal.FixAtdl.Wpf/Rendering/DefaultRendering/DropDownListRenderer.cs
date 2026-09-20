// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class DropDownListRenderer : IControlRenderer<DropDownList_t>
{
    public Type ControlType => typeof(DropDownList_t);

    public void Render(WpfXmlWriter writer, DropDownList_t control)
    {
        WpfControlRenderer.RenderLabelledControl<DropDownList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(WpfXmlWriterTag.ComboBox))
                {
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate, includeErrorCue: true);

                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");
                    // The invalid-state cue is a local value on the control, not an implicit style:
                    // ComboBox and ListBox belong to the host, and a style for them in this panel's
                    // dictionary would shadow the host's own. See ErrorCue.
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.SelectedValue, "{Binding Path=SelectedValue}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.SelectedValuePath, "EnumId");
                    writer.WriteAttribute(WpfXmlWriterAttribute.DisplayMemberPath, "UiRep");
                }
            }
        );
    }
}
