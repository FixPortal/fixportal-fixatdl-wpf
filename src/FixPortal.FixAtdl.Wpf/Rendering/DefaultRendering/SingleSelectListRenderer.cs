// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class SingleSelectListRenderer : IControlRenderer<SingleSelectList_t>
{
    public Type ControlType => typeof(SingleSelectList_t);

    public void Render(WpfXmlWriter writer, SingleSelectList_t control)
    {
        WpfControlRenderer.RenderLabelledControl<SingleSelectList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(WpfXmlWriterTag.ListBox))
                {
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate, includeErrorCue: true);
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
