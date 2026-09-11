using System.Collections.Generic;

namespace FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;

// These constants are XAML/XML namespace identifiers, not filesystem paths or network locations, so
// Sonar's "no hardcoded absolute paths or URIs" advice (S1075) does not apply to them.
#pragma warning disable S1075
public class DefaultNamespaceProvider : INamespaceProvider
{
    public const string XamlNamespaceUri = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    public const string XamlXNamespace = "x";
    public const string XamlXNamespaceUri = "http://schemas.microsoft.com/winfx/2006/xaml";
    public const string ControlsNamespace = "atdl";

    public const string ControlsNamespaceUri =
        "clr-namespace:FixPortal.FixAtdl.Wpf.Controls;assembly=FixPortal.FixAtdl.Wpf";

    public const string SystemNamespace = "sys";
    public const string SystemNamespaceUri = "clr-namespace:System;assembly=mscorlib";

    private readonly Dictionary<string, string> _namespaces = new()
    {
        [string.Empty] = XamlNamespaceUri,
        [XamlXNamespace] = XamlXNamespaceUri,
        [ControlsNamespace] = ControlsNamespaceUri,
        [SystemNamespace] = SystemNamespaceUri,
    };

    public Dictionary<string, string> CustomNamespaces => _namespaces;
}
#pragma warning restore S1075
