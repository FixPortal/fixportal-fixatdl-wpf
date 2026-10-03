// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class EditableDropDownListRenderer : IControlRenderer<EditableDropDownList_t>
{
    public Type ControlType => typeof(EditableDropDownList_t);

    public void Render(WpfXmlWriter writer, EditableDropDownList_t control)
    {
        WpfControlRenderer.RenderLabelledControl<EditableDropDownList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(WpfXmlWriterTag.ComboBox))
                {
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate, includeErrorCue: true);

                    writer.WriteAttribute(WpfXmlWriterAttribute.IsEditable, "True");

                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");
                    // The invalid-state cue is a local value on the control, not an implicit style:
                    // ComboBox and ListBox belong to the host, and a style for them in this panel's
                    // dictionary would shadow the host's own. See ErrorCue.
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.SelectedValuePath, "EnumId");
                    writer.WriteAttribute(WpfXmlWriterAttribute.DisplayMemberPath, "UiRep");
                    writer.WriteAttribute(WpfXmlWriterAttribute.Text, "{Binding Path=Text, Mode=TwoWay}");
                }
            }
        );
    }
}
