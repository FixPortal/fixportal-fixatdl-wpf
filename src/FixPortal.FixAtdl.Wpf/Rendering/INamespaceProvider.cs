using System.Collections.Generic;

namespace FixPortal.FixAtdl.Wpf.Rendering;

public interface INamespaceProvider
{
    Dictionary<string, string> CustomNamespaces { get; }
}
