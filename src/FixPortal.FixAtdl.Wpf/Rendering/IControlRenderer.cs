// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using FixPortal.FixAtdl.Model.Elements;

namespace FixPortal.FixAtdl.Wpf.Rendering;

public interface IControlRenderer
{
    Type ControlType { get; }
}

public interface IControlRenderer<in T> : IControlRenderer
    where T : Control_t
{
    void Render(WpfXmlWriter writer, T control);
}
