// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using FixPortal.FixAtdl.Diagnostics.Exceptions;
using FixPortal.FixAtdl.Model.Collections;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Wpf.Rendering.DefaultRendering;
using ThrowHelper = FixPortal.FixAtdl.Diagnostics.ThrowHelper;

namespace FixPortal.FixAtdl.Wpf.Rendering;

public sealed class StrategyPanelRenderer
{
    private const int MaxPanelDepth = 64;
    public const string ExceptionContext = "StrategyPanelRenderer";

    public static readonly string CollapsedVisibility = nameof(Visibility.Collapsed);
    public static readonly string VisibleVisibility = nameof(Visibility.Visible);

    private readonly IReadOnlyDictionary<Type, IControlRenderer> _renderersByControlType;
    private readonly INamespaceProvider _namespaceProvider = new DefaultNamespaceProvider();

    public StrategyPanelRenderer(IEnumerable<IControlRenderer> renderers)
    {
        _renderersByControlType = renderers
            .GroupBy(r => r.ControlType)
            .ToDictionary(group => group.Key, group => group.Last());
    }

    public FrameworkElement? Render(Strategy_t strategy, IServiceProvider services)
    {
        _ = services;

        if (strategy.StrategyLayout == null)
        {
            throw ThrowHelper.New<RenderingException>(ExceptionContext, "No strategy layout was supplied.");
        }

        StrategyPanel_t rootPanel =
            strategy.StrategyLayout.StrategyPanel
            ?? throw ThrowHelper.New<RenderingException>(
                ExceptionContext,
                "No strategy panels were found in this strategy."
            );

        var xamlText = new StringBuilder();

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment,
        };

        using (XmlWriter xmlWriter = XmlWriter.Create(xamlText, settings))
        {
            var requiredParameterNames = strategy
                .Parameters.Where(parameter => parameter.Use == Use_t.Required)
                .Select(parameter => parameter.Name)
                .ToHashSet(StringComparer.Ordinal);

            WpfXmlWriter wpfWriter = new WpfXmlWriter(xmlWriter, strategy.Controls, requiredParameterNames);

            WpfControlRenderer controlRenderer = new WpfControlRenderer(
                wpfWriter,
                _renderersByControlType.Values,
                _namespaceProvider
            );

            ProcessPanel(rootPanel, wpfWriter, controlRenderer, -1, true, 0);
        }

        var view = (FrameworkElement)XamlReader.Parse(xamlText.ToString());
        view.Resources.MergedDictionaries.Add(
            new ResourceDictionary
            {
                Source = new Uri("/FixPortal.FixAtdl.Wpf;component/FixAtdlWpfResources.xaml", UriKind.Relative),
            }
        );
        return view;
    }

    private static void ProcessPanel(
        StrategyPanel_t panel,
        WpfXmlWriter writer,
        WpfControlRenderer controlRenderer,
        int rowOrColumn,
        bool parentIsVertical,
        int depth
    )
    {
        if (depth >= MaxPanelDepth)
        {
            throw ThrowHelper.New<RenderingException>(
                ExceptionContext,
                $"Strategy panel nesting exceeds the maximum depth of {MaxPanelDepth}."
            );
        }

        bool isVertical = panel.Orientation == Orientation_t.Vertical;

        using (
            writer.New(
                DefaultNamespaceProvider.ControlsNamespace,
                "StrategyPanelFrame",
                DefaultNamespaceProvider.ControlsNamespaceUri
            )
        )
        {
            // Adjacent panels sat 2px apart at the upstream value of 1, which read as one block.
            writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "4");

            WritePanelAttributes(writer, panel);
            WritePanelPositionOrNamespaces(writer, controlRenderer, parentIsVertical, rowOrColumn);

            bool containsControls = panel.Controls.Count > 0;

            // For grids containing a horizontal arrangement of controls, we add an empty column so we can set its width to "*"
            int horizontalPad = isVertical ? 0 : 1;
            int childCount = containsControls ? panel.Controls.Count + horizontalPad : panel.StrategyPanels.Count;

            using (writer.New(WpfXmlWriterTag.Grid))
            {
                WriteGridDefinitions(writer, isVertical, containsControls, childCount);
                ProcessPanelChildrenOrControls(panel, writer, controlRenderer, isVertical, depth + 1);
            }
        }
    }

    private static void WritePanelPositionOrNamespaces(
        WpfXmlWriter writer,
        WpfControlRenderer controlRenderer,
        bool parentIsVertical,
        int rowOrColumn
    )
    {
        if (rowOrColumn == -1)
        {
            foreach (KeyValuePair<string, string> ns in controlRenderer.NamespaceProvider.CustomNamespaces)
            {
                writer.WriteNamespaceAttribute(ns.Key, ns.Value);
            }

            return;
        }

        WpfXmlWriterAttribute positionAttribute = parentIsVertical
            ? WpfXmlWriterAttribute.GridRow
            : WpfXmlWriterAttribute.GridColumn;

        writer.WriteAttribute(positionAttribute, rowOrColumn.ToString());
    }

    private static void WriteGridDefinitions(
        WpfXmlWriter writer,
        bool isVertical,
        bool containsControls,
        int childCount
    )
    {
        using (writer.New(WpfXmlWriterTag.GridRowDefinitions))
        {
            int rowCount = isVertical ? childCount : 1;

            for (int n = 0; n < rowCount; n++)
            {
                using (writer.New(WpfXmlWriterTag.RowDefinition))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.Height, "Auto");
                }
            }
        }

        using (writer.New(WpfXmlWriterTag.GridColumnDefinitions))
        {
            // Special treatment for vertical panels that contain controls - put in two columns, one for the label and
            // one for the control itself.
            int verticalColumnCount = containsControls ? 2 : 1;
            int columnCount = isVertical ? verticalColumnCount : childCount;

            for (int n = 0; n < columnCount; n++)
            {
                using (writer.New(WpfXmlWriterTag.ColumnDefinition))
                {
                    if (containsControls)
                    {
                        string width = n < columnCount - 1 ? "Auto" : "*";

                        writer.WriteAttribute(WpfXmlWriterAttribute.Width, width);
                    }
                }
            }
        }
    }

    private static void ProcessPanelChildrenOrControls(
        StrategyPanel_t panel,
        WpfXmlWriter writer,
        WpfControlRenderer controlRenderer,
        bool isVertical,
        int depth
    )
    {
        // Note that a StrategyPanel_t can either contain other strategy panels, or controls but NOT BOTH.
        if (panel.StrategyPanels != null && panel.StrategyPanels.Count > 0)
        {
            int thisRowOrColumn = 0;

            foreach (StrategyPanel_t childPanel in panel.StrategyPanels)
            {
                ProcessPanel(childPanel, writer, controlRenderer, thisRowOrColumn, isVertical, depth);

                thisRowOrColumn++;
            }

            return;
        }

        ProcessControls(panel, controlRenderer);

        // For horizontal strategy panels, put a dummy rectangle in the last column to trick
        // WPF to sizing the other columns to their control size.
        if (!isVertical)
        {
            using (writer.New(WpfXmlWriterTag.Rectangle))
            {
                writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, panel.Controls.Count.ToString());
            }
        }
    }

    private static void ProcessControls(StrategyPanel_t panel, WpfControlRenderer renderer)
    {
        ControlCollection controls = panel.Controls;

        if (controls == null || controls.Count == 0)
        {
            return;
        }

        foreach (var control in controls)
        {
            renderer.ProcessControl(control);
        }
    }

    private static void WritePanelAttributes(WpfXmlWriter writer, StrategyPanel_t panel)
    {
        writer.WriteAttribute(
            WpfXmlWriterAttribute.BorderVisibility,
            panel.Border == Border_t.Line ? VisibleVisibility : CollapsedVisibility
        );
        writer.WriteAttribute(
            WpfXmlWriterAttribute.HeaderVisibility,
            string.IsNullOrEmpty(panel.Title) && panel.Collapsible != true ? CollapsedVisibility : VisibleVisibility
        );
        writer.WriteAttribute(
            WpfXmlWriterAttribute.CollapseButtonVisibility,
            panel.Collapsible == true ? VisibleVisibility : CollapsedVisibility
        );
        writer.WriteAttribute(
            WpfXmlWriterAttribute.IsExpanded,
            panel.Collapsible == true && panel.Collapsed == true ? "False" : "True"
        );

        if (!string.IsNullOrEmpty(panel.Title))
        {
            writer.WriteLiteralAttribute(WpfXmlWriterAttribute.Header, panel.Title);
        }
    }
}
