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
