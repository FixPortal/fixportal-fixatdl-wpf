// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FixPortal.FixAtdl.Model.Elements;

namespace FixPortal.FixAtdl.Wpf.Rendering;

public class WpfXmlWriter
{
    private struct AttributeInformation
    {
        public readonly string Name;
        public readonly string? Prefix;
        public readonly string? Namespace;

        public AttributeInformation(string name)
            : this(name, null, null) { }

        public AttributeInformation(string name, string? prefix, string? ns)
        {
            Name = name;
            Prefix = prefix;
            Namespace = ns;
        }
    }

    private struct TagInformation
    {
        public readonly string Name;
        public readonly string? Namespace;

        public TagInformation(string name)
            : this(name, null) { }

        public TagInformation(string name, string? ns)
        {
            Name = name;
            Namespace = ns;
        }
    }

    /// <summary>
    /// Helper class to simplify writing xHTML when dealing with tags that open and eventually close (e.g., &lt;table&gt;).
    /// </summary>
    public class WpfEnclosingTagHelper : IDisposable
    {
        private WpfXmlWriter? _writer;

        public WpfEnclosingTagHelper(WpfXmlWriter writer, WpfXmlWriterTag tag)
        {
            _writer = writer;

            _writer.WriteBeginTag(tag);
        }

        public WpfEnclosingTagHelper(WpfXmlWriter writer, string prefix, string localName, string ns)
        {
            _writer = writer;

            _writer.WriteBeginTag(prefix, localName, ns);
        }

        public WpfEnclosingTagHelper(WpfXmlWriter writer, string namespaceUri, string localName)
        {
            _writer = writer;

            _writer.WriteBeginTag(namespaceUri, localName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _writer != null)
            {
                _writer.WriteEndTag();

                _writer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }

    private readonly XmlWriter _writer;
    private readonly string _radioGroupScope = Guid.NewGuid().ToString("N");
    private readonly IReadOnlyDictionary<Control_t, int> _controlIndexes;
    private readonly IReadOnlySet<string> _requiredParameterNames;
    private static AttributeInformation[] _attributeInformation = [];
    private static TagInformation[] _tagInformation = [];

    static WpfXmlWriter()
    {
        RegisterTags();
        RegisterAttributes();
    }

    public WpfXmlWriter(XmlWriter writer, IEnumerable<Control_t> controls)
        : this(writer, controls, new HashSet<string>(StringComparer.Ordinal)) { }

    /// <param name="requiredParameterNames">
    /// Names of the parameters declared <c>use="required"</c>. A control bound to one of these gets a
    /// required marker on its label, so the requirement is carried by something other than colour.
    /// </param>
    public WpfXmlWriter(XmlWriter writer, IEnumerable<Control_t> controls, IReadOnlySet<string> requiredParameterNames)
    {
        _requiredParameterNames = requiredParameterNames;
        _writer = writer;
        _controlIndexes = controls
            .Select((control, index) => (control, index))
            .ToDictionary(p => p.control, p => p.index);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var invalid = _controlIndexes.Keys.FirstOrDefault(control =>
            string.IsNullOrEmpty(control.Id) || !ids.Add(control.Id)
        );
        if (invalid is not null)
        {
            throw new ArgumentException($"Control IDs must be nonempty and unique: '{invalid.Id}'.", nameof(controls));
        }
    }

    public int ControlIndex(Control_t control) => _controlIndexes[control];

    /// <summary>
    /// Whether the supplied control is bound to a parameter declared <c>use="required"</c>.
    /// </summary>
    public bool IsRequired(Control_t control) =>
        control.ParameterRef is { } name && _requiredParameterNames.Contains(name);

    public string RadioGroupName(string group) => _radioGroupScope + group;

    public void WriteBeginTag(string namespaceUri, string localName)
    {
        _writer.WriteStartElement(localName, namespaceUri);
    }

    public void WriteBeginTag(string prefix, string localName, string ns)
    {
        _writer.WriteStartElement(prefix, localName, ns);
    }

    public void WriteBeginTag(WpfXmlWriterTag tag)
    {
        TagInformation tagInfo = _tagInformation[(int)tag];

        _writer.WriteStartElement(tagInfo.Name, tagInfo.Namespace);
    }

    public void WriteEndTag()
    {
        _writer.WriteEndElement();
    }

    public void WriteAttribute(WpfXmlWriterAttribute attribute, string value)
    {
        AttributeInformation attributeInfo = _attributeInformation[(int)attribute];

        if (attributeInfo.Namespace is null)
        {
            _writer.WriteAttributeString(attributeInfo.Name, value);
        }
        else
        {
            _writer.WriteAttributeString(attributeInfo.Prefix, attributeInfo.Name, attributeInfo.Namespace, value);
        }
    }

    public void WriteAttribute(string attribute, string value)
    {
        _writer.WriteAttributeString(attribute, value);
    }

    /// <summary>Writes venue text without allowing XAML markup-extension evaluation.</summary>
    public void WriteLiteralAttribute(WpfXmlWriterAttribute attribute, string value)
    {
        WriteAttribute(attribute, value.StartsWith('{') ? "{}" + value : value);
    }

    public void WriteNamespaceAttribute(string prefix, string uri)
    {
        _writer.WriteAttributeString("xmlns", prefix, null, uri);
    }

    public IDisposable New(WpfXmlWriterTag tag)
    {
        return new WpfEnclosingTagHelper(this, tag);
    }

    public IDisposable New(string namespaceUri, string localName)
    {
        return new WpfEnclosingTagHelper(this, namespaceUri, localName);
    }

    public IDisposable New(string prefix, string localName, string ns)
    {
        return new WpfEnclosingTagHelper(this, prefix, localName, ns);
    }

    protected static void RegisterTags()
    {
        _tagInformation = new TagInformation[Enum.GetValues(typeof(WpfXmlWriterTag)).Length];

        _tagInformation[(int)WpfXmlWriterTag.CheckBox] = new TagInformation("CheckBox");
        _tagInformation[(int)WpfXmlWriterTag.ColumnDefinition] = new TagInformation("ColumnDefinition");
        _tagInformation[(int)WpfXmlWriterTag.ComboBox] = new TagInformation("ComboBox");
        _tagInformation[(int)WpfXmlWriterTag.Grid] = new TagInformation("Grid");
        _tagInformation[(int)WpfXmlWriterTag.GridColumnDefinitions] = new TagInformation("Grid.ColumnDefinitions");
        _tagInformation[(int)WpfXmlWriterTag.GridRowDefinitions] = new TagInformation("Grid.RowDefinitions");
        _tagInformation[(int)WpfXmlWriterTag.Label] = new TagInformation("Label");
        _tagInformation[(int)WpfXmlWriterTag.ListBox] = new TagInformation("ListBox");
        _tagInformation[(int)WpfXmlWriterTag.RadioButton] = new TagInformation("RadioButton");
        _tagInformation[(int)WpfXmlWriterTag.Rectangle] = new TagInformation("Rectangle");
        _tagInformation[(int)WpfXmlWriterTag.RowDefinition] = new TagInformation("RowDefinition");
    }

    protected static void RegisterAttributes()
    {
        _attributeInformation = new AttributeInformation[Enum.GetValues(typeof(WpfXmlWriterAttribute)).Length];

        _attributeInformation[(int)WpfXmlWriterAttribute.AutomationProperties_AutomationId] = new AttributeInformation(
            "AutomationProperties.AutomationId"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.BorderThickness] = new AttributeInformation("BorderThickness");
        _attributeInformation[(int)WpfXmlWriterAttribute.BorderVisibility] = new AttributeInformation(
            "BorderVisibility"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.CollapseButtonVisibility] = new AttributeInformation(
            "CollapseButtonVisibility"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.Content] = new AttributeInformation("Content");
        _attributeInformation[(int)WpfXmlWriterAttribute.ContentStringFormat] = new AttributeInformation(
            "ContentStringFormat"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.DataContext] = new AttributeInformation("DataContext");
        _attributeInformation[(int)WpfXmlWriterAttribute.DisplayMemberPath] = new AttributeInformation(
            "DisplayMemberPath"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.ErrorCue_HasErrors] = new AttributeInformation(
            "ErrorCue.HasErrors",
            DefaultRendering.DefaultNamespaceProvider.ControlsNamespace,
            DefaultRendering.DefaultNamespaceProvider.ControlsNamespaceUri
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.GridColumn] = new AttributeInformation("Grid.Column");
        _attributeInformation[(int)WpfXmlWriterAttribute.GridRow] = new AttributeInformation("Grid.Row");
        _attributeInformation[(int)WpfXmlWriterAttribute.GroupName] = new AttributeInformation("GroupName");
        _attributeInformation[(int)WpfXmlWriterAttribute.Header] = new AttributeInformation("Header");
        _attributeInformation[(int)WpfXmlWriterAttribute.HeaderVisibility] = new AttributeInformation(
            "HeaderVisibility"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.Height] = new AttributeInformation("Height");
        _attributeInformation[(int)WpfXmlWriterAttribute.HorizontalAlignment] = new AttributeInformation(
            "HorizontalAlignment"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.VerticalAlignment] = new AttributeInformation(
            "VerticalAlignment"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.Increment] = new AttributeInformation("Increment");
        _attributeInformation[(int)WpfXmlWriterAttribute.InnerIncrement] = new AttributeInformation("InnerIncrement");
        _attributeInformation[(int)WpfXmlWriterAttribute.IsChecked] = new AttributeInformation("IsChecked");
        _attributeInformation[(int)WpfXmlWriterAttribute.IsContentValid] = new AttributeInformation("IsContentValid");
        _attributeInformation[(int)WpfXmlWriterAttribute.IsEditable] = new AttributeInformation("IsEditable");
        _attributeInformation[(int)WpfXmlWriterAttribute.IsEnabled] = new AttributeInformation("IsEnabled");
        _attributeInformation[(int)WpfXmlWriterAttribute.IsExpanded] = new AttributeInformation("IsExpanded");
        _attributeInformation[(int)WpfXmlWriterAttribute.ItemContainerStyle] = new AttributeInformation(
            "ItemContainerStyle"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.ItemsSource] = new AttributeInformation("ItemsSource");
        _attributeInformation[(int)WpfXmlWriterAttribute.Margin] = new AttributeInformation("Margin");
        _attributeInformation[(int)WpfXmlWriterAttribute.Name] = new AttributeInformation("Name");
        _attributeInformation[(int)WpfXmlWriterAttribute.Orientation] = new AttributeInformation("Orientation");
        _attributeInformation[(int)WpfXmlWriterAttribute.OuterIncrement] = new AttributeInformation("OuterIncrement");
        _attributeInformation[(int)WpfXmlWriterAttribute.Padding] = new AttributeInformation("Padding");
        _attributeInformation[(int)WpfXmlWriterAttribute.SelectedItem] = new AttributeInformation("SelectedItem");
        _attributeInformation[(int)WpfXmlWriterAttribute.SelectedValue] = new AttributeInformation("SelectedValue");
        _attributeInformation[(int)WpfXmlWriterAttribute.SelectedValuePath] = new AttributeInformation(
            "SelectedValuePath"
        );
        _attributeInformation[(int)WpfXmlWriterAttribute.SelectionMode] = new AttributeInformation("SelectionMode");
        _attributeInformation[(int)WpfXmlWriterAttribute.Target] = new AttributeInformation("Target");
        _attributeInformation[(int)WpfXmlWriterAttribute.Text] = new AttributeInformation("Text");
        _attributeInformation[(int)WpfXmlWriterAttribute.ToolTip] = new AttributeInformation("ToolTip");
        _attributeInformation[(int)WpfXmlWriterAttribute.Time] = new AttributeInformation("Time");
        _attributeInformation[(int)WpfXmlWriterAttribute.Value] = new AttributeInformation("Value");
        _attributeInformation[(int)WpfXmlWriterAttribute.VirtualizingStackPanel_IsVirtualizing] =
            new AttributeInformation("VirtualizingStackPanel.IsVirtualizing");
        _attributeInformation[(int)WpfXmlWriterAttribute.Visibility] = new AttributeInformation("Visibility");
        _attributeInformation[(int)WpfXmlWriterAttribute.Width] = new AttributeInformation("Width");
    }
}
