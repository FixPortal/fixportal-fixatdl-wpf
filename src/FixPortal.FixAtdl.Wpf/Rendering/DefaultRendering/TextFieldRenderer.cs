// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class TextFieldRenderer : IControlRenderer<TextField_t>
{
    public Type ControlType => typeof(TextField_t);

    public void Render(WpfXmlWriter writer, TextField_t control)
    {
        WpfControlRenderer.RenderLabelledControl<TextField_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.ControlsNamespaceUri, typeof(ClickSelectTextBox).Name))
                {
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate, includeErrorCue: true);
                    writer.WriteAttribute(WpfXmlWriterAttribute.Width, "120");
                    writer.WriteAttribute(WpfXmlWriterAttribute.HorizontalAlignment, "Left");
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.Text,
                        "{Binding Path=Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                    );
                }
            }
        );
    }
}
