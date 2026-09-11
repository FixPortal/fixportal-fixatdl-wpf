using System;
using FixPortal.FixAtdl.Model.Elements;

namespace FixPortal.FixAtdl.Wpf.Rendering;

// FP Enhancement: non-generic base added (with the original MEF metadata-attribute's job — telling
// the composer which Control_t this renderer handles — now a plain property) so a mixed bag of
// per-control renderers can be constructor-injected as IEnumerable&lt;IControlRenderer&gt; and looked
// up by Type, replacing the MEF [ImportMany]/[Export] pair.
public interface IControlRenderer
{
    Type ControlType { get; }
}

// FP Enhancement: renamed from IWpfControlRenderer<T> for clarity. Still XAML-text-writer-shaped
// (Render(WpfXmlWriter, T)) per the port decision to keep the original rendering mechanism verbatim
// — only StrategyPanelRenderer's outermost Render() returns a FrameworkElement, via
// XamlReader.Parse over the composed XAML string.
public interface IControlRenderer<in T> : IControlRenderer
    where T : Control_t
{
    void Render(WpfXmlWriter writer, T control);
}
