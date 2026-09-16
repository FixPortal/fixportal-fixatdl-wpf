// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Controls;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

internal class HiddenFieldRenderer : IControlRenderer<HiddenField_t>
{
    public Type ControlType => typeof(HiddenField_t);

    public void Render(WpfXmlWriter writer, HiddenField_t control) { }
}
