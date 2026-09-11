using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class HiddenFieldRenderer : IControlRenderer<HiddenField_t>
{
    public Type ControlType => typeof(HiddenField_t);

    public void Render(WpfXmlWriter writer, HiddenField_t control) { }
}
