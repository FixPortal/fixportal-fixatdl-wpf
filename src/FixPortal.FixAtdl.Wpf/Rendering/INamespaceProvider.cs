// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.

namespace FixPortal.FixAtdl.Wpf.Rendering;

public interface INamespaceProvider
{
    Dictionary<string, string> CustomNamespaces { get; }
}
