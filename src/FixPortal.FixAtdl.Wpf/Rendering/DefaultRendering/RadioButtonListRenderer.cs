// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class RadioButtonListRenderer : IControlRenderer<RadioButtonList_t>
{
    public Type ControlType => typeof(RadioButtonList_t);

    public void Render(WpfXmlWriter writer, RadioButtonList_t control)
    {
        WpfControlRenderer.RenderLabelledControl<RadioButtonList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.ControlsNamespaceUri, typeof(RadioButtonList).Name))
                {
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate);
                    writer.WriteAttribute(WpfXmlWriterAttribute.Orientation, "{Binding Path=Orientation, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                }
            }
        );
    }
}
