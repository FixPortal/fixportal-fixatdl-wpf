// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Wpf.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

public class CheckBoxListRenderer : IControlRenderer<CheckBoxList_t>
{
    public Type ControlType => typeof(CheckBoxList_t);

    public void Render(WpfXmlWriter writer, CheckBoxList_t control)
    {
        WpfControlRenderer.RenderLabelledControl<CheckBoxList_t>(
            writer,
            control,
            (c, gridCoordinate) =>
            {
                using (writer.New(DefaultNamespaceProvider.ControlsNamespaceUri, typeof(CheckBoxList).Name))
                {
                    WpfControlRenderer.WriteStandardControlAttributes(writer, c, gridCoordinate);
                    writer.WriteAttribute(WpfXmlWriterAttribute.Orientation, "{Binding Path=Orientation, Mode=OneWay}");
                    writer.WriteAttribute(WpfXmlWriterAttribute.ItemsSource, "{Binding Path=Items}");
                }
            }
        );
    }
}
