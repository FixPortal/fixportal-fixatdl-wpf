// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
using System;
using System.Collections.Generic;
using System.Linq;
using FixPortal.FixAtdl.Model.Controls;
using FixPortal.FixAtdl.Model.Controls.Support;
using FixPortal.FixAtdl.Model.Elements;
using FixPortal.FixAtdl.Model.Enumerations;
using FixPortal.FixAtdl.Utility;

namespace FixPortal.FixAtdl.Wpf.Rendering;

/// <summary>
/// Provides XAML rendering of FIXatdl controls.
/// </summary>
public class WpfControlRenderer : IControlVisitor
{
    private struct StandardGridCoordinates
    {
        public static readonly GridCoordinate Label = new GridCoordinate(0, 0);
        public static readonly GridCoordinate Control = new GridCoordinate(0, 1);
    }

    private readonly WpfXmlWriter _writer;

    public INamespaceProvider NamespaceProvider { get; }

    private readonly IReadOnlyDictionary<Type, IControlRenderer> _renderersByControlType;

    /// <summary>
    /// Initializes a new WpfControlRenderer.
    /// </summary>
    /// <param name="writer">WpfXmlWriter to use for writing the output XAML.</param>
    /// <param name="renderers">The set of per-control-type renderers to dispatch to.</param>
    /// <param name="namespaceProvider">Provides the custom XAML namespaces needed for custom controls.</param>
    public WpfControlRenderer(
        WpfXmlWriter writer,
        IEnumerable<IControlRenderer> renderers,
        INamespaceProvider namespaceProvider
    )
    {
        _writer = writer;
        NamespaceProvider = namespaceProvider;
        _renderersByControlType = renderers.ToDictionary(r => r.ControlType);
    }

    /// <summary>
    /// Processes each control, i.e., renders the XAML for the supplied control.
    /// </summary>
    /// <param name="control">Control to generate XAML for.</param>
    public void ProcessControl(Control_t control)
    {
        ModelUtils.VisitHelper(typeof(IControlVisitor), this, control);
    }

    private T GetRenderer<T>(Type controlType)
        where T : class, IControlRenderer
    {
        return (T)_renderersByControlType[controlType];
    }

    /// <summary>
    /// Renders the supplied CheckBox_t as XAML.
    /// </summary>
    /// <param name="control">CheckBox_t to render.</param>
    public void Visit(CheckBox_t control)
    {
        GetRenderer<IControlRenderer<CheckBox_t>>(typeof(CheckBox_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied CheckBoxList_t as XAML.
    /// </summary>
    /// <param name="control">CheckBoxList_t to render.</param>
    public void Visit(CheckBoxList_t control)
    {
        GetRenderer<IControlRenderer<CheckBoxList_t>>(typeof(CheckBoxList_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied Clock_t as XAML.
    /// </summary>
    /// <param name="control">Clock_t to render.</param>
    public void Visit(Clock_t control)
    {
        GetRenderer<IControlRenderer<Clock_t>>(typeof(Clock_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied DoubleSpinner_t as XAML.
    /// </summary>
    /// <param name="control">DoubleSpinner_t to render.</param>
    public void Visit(DoubleSpinner_t control)
    {
        GetRenderer<IControlRenderer<DoubleSpinner_t>>(typeof(DoubleSpinner_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied DropDownList_t as XAML.
    /// </summary>
    /// <param name="control">DropDownList_t to render.</param>
    public void Visit(DropDownList_t control)
    {
        GetRenderer<IControlRenderer<DropDownList_t>>(typeof(DropDownList_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied EditableDropDownList_t as XAML.
    /// </summary>
    /// <param name="control">EditableDropDownList_t to render.</param>
    public void Visit(EditableDropDownList_t control)
    {
        GetRenderer<IControlRenderer<EditableDropDownList_t>>(typeof(EditableDropDownList_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied HiddenField_t as XAML.
    /// </summary>
    /// <param name="control">HiddenField_t to render.</param>
    public void Visit(HiddenField_t control)
    {
        GetRenderer<IControlRenderer<HiddenField_t>>(typeof(HiddenField_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied Label_t as XAML.
    /// </summary>
    /// <param name="control">Label_t to render.</param>
    public void Visit(Label_t control)
    {
        GetRenderer<IControlRenderer<Label_t>>(typeof(Label_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied MultiSelectList_t as XAML.
    /// </summary>
    /// <param name="control">MultiSelectList_t to render.</param>
    public void Visit(MultiSelectList_t control)
    {
        GetRenderer<IControlRenderer<MultiSelectList_t>>(typeof(MultiSelectList_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied RadioButton_t as XAML.
    /// </summary>
    /// <param name="control">RadioButton_t to render.</param>
    public void Visit(RadioButton_t control)
    {
        GetRenderer<IControlRenderer<RadioButton_t>>(typeof(RadioButton_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied RadioButtonList_t as XAML.
    /// </summary>
    /// <param name="control">RadioButtonList_t to render.</param>
    public void Visit(RadioButtonList_t control)
    {
        GetRenderer<IControlRenderer<RadioButtonList_t>>(typeof(RadioButtonList_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied SingleSelectList_t as XAML.
    /// </summary>
    /// <param name="control">SingleSelectList_t to render.</param>
    public void Visit(SingleSelectList_t control)
    {
        GetRenderer<IControlRenderer<SingleSelectList_t>>(typeof(SingleSelectList_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied SingleSpinner_t as XAML.
    /// </summary>
    /// <param name="control">SingleSpinner_t to render.</param>
    public void Visit(SingleSpinner_t control)
    {
        GetRenderer<IControlRenderer<SingleSpinner_t>>(typeof(SingleSpinner_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied Slider_t as XAML.
    /// </summary>
    /// <param name="control">Slider_t to render.</param>
    public void Visit(Slider_t control)
    {
        GetRenderer<IControlRenderer<Slider_t>>(typeof(Slider_t)).Render(_writer, control);
    }

    /// <summary>
    /// Renders the supplied TextField_t as XAML.
    /// </summary>
    /// <param name="control">TextField_t to render.</param>
    public void Visit(TextField_t control)
    {
        GetRenderer<IControlRenderer<TextField_t>>(typeof(TextField_t)).Render(_writer, control);
    }

    /// <summary>
    /// Visitor fallback for a control type with no registered renderer.
    /// </summary>
    /// <param name="control">Control to render.</param>
    /// <exception cref="NotSupportedException">Always. This is where an unrecognised control lands.</exception>
    public void Visit(Control_t control)
    {
        ArgumentNullException.ThrowIfNull(control);

        // This is the arm an unrecognised broker control reaches, so it is a real diagnostic surface
        // rather than dead code: name the type and the control id, and say what is unsupported. A bare
        // NotImplementedException told the consumer neither which control failed nor why.
        throw new NotSupportedException(
            $"No renderer is registered for control type '{control.GetType().Name}' (control id '{control.Id}')."
        );
    }

    /// <summary>
    /// Delegate that is used to call each control renderer.
    /// </summary>
    /// <typeparam name="T">Type of control.</typeparam>
    /// <param name="control">Control to be rendered.</param>
    /// <param name="gridCoordinate">Place in the grid for this control.</param>
    public delegate void ControlRenderer<in T>(T control, GridCoordinate gridCoordinate);

    /// <summary>
    /// Renders the supplied "labelled control", i.e., control which has a label.
    /// </summary>
    /// <typeparam name="T">Type of control.</typeparam>
    /// <param name="writer">WpfXmlWriter to use to write the XAML.</param>
    /// <param name="control">Control to render alongside label.</param>
    /// <param name="controlRenderer">Control renderer to use to render the control.</param>
    public static void RenderLabelledControl<T>(WpfXmlWriter writer, T control, ControlRenderer<T> controlRenderer)
        where T : Control_t
    {
        bool isVertical = ((IParentable<StrategyPanel_t>)control).Parent.Orientation == Orientation_t.Vertical;

        // If this is a vertical StrategyPanel, we don't bother with a containing Grid - this provides nice alignment of labels and controls
        if (isVertical)
        {
            RenderControlLabel(writer, control, new GridCoordinate(control.Index, 0));

            controlRenderer(control, new GridCoordinate(control.Index, 1));
        }
        else
        {
            using (writer.New(WpfXmlWriterTag.Grid))
            {
                writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, control.Index.ToString());

                using (writer.New(WpfXmlWriterTag.GridColumnDefinitions))
                {
                    for (int n = 0; n < 2; n++)
                    {
                        using (writer.New(WpfXmlWriterTag.ColumnDefinition))
                        {
                            writer.WriteAttribute(WpfXmlWriterAttribute.Width, "Auto");
                        }
                    }
                }

                using (writer.New(WpfXmlWriterTag.GridRowDefinitions))
                {
                    using (writer.New(WpfXmlWriterTag.RowDefinition))
                    {
                        writer.WriteAttribute(WpfXmlWriterAttribute.Height, "Auto");
                    }
                }

                RenderControlLabel(writer, control, StandardGridCoordinates.Label);

                controlRenderer(control, StandardGridCoordinates.Control);
            }
        }
    }

    /// <summary>
    /// Writes the Grid.Row or Grid.Column attribute as appropriate for this control based on orientation of
    /// the panel this control is a member of.
    /// </summary>
    /// <param name="writer">WpfXmlWriter to use to write the XAML to.</param>
    /// <param name="control">Control.</param>
    public static void WriteGridAttribute(WpfXmlWriter writer, Control_t control)
    {
        bool isVertical = ((IParentable<StrategyPanel_t>)control).Parent.Orientation == Orientation_t.Vertical;

        writer.WriteAttribute(
            isVertical ? WpfXmlWriterAttribute.GridRow : WpfXmlWriterAttribute.GridColumn,
            control.Index.ToString()
        );
    }

    /// <summary>
    /// Gets a "cleaned up" version of the control ID so it can be used in XAML.
    /// </summary>
    /// <param name="controlId">ID of the Control_t.</param>
    /// <returns>Cleaned up control ID.</returns>
    /// <remarks>Encoding every UTF-16 code unit preserves uniqueness while excluding XAML syntax.</remarks>
    public static string CleanName(string controlId)
    {
        return "Control_"
            + string.Concat(
                controlId.Select(c => ((int)c).ToString("X4", System.Globalization.CultureInfo.InvariantCulture))
            );
    }

    private static void RenderControlLabel(WpfXmlWriter writer, Control_t control, GridCoordinate gridCoordinate)
    {
        string label = control.Label;
        string forControl = control.Id;

        // Assumes that a control label will always be in the first cell of a 2 x 1 grid.
        if (!string.IsNullOrEmpty(label))
        {
            using (writer.New(WpfXmlWriterTag.Label))
            {
                writer.WriteAttribute(WpfXmlWriterAttribute.GridColumn, gridCoordinate.Column.ToString());
                writer.WriteAttribute(WpfXmlWriterAttribute.GridRow, gridCoordinate.Row.ToString());

                // A Label defaults to top alignment while the control beside it centres itself, so
                // the two sat on different baselines. The right margin is the gap between label and
                // control, which had none at all.
                writer.WriteAttribute(WpfXmlWriterAttribute.VerticalAlignment, "Center");
                writer.WriteAttribute(WpfXmlWriterAttribute.Margin, "0,0,8,0");

                if (!string.IsNullOrEmpty(forControl))
                {
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.Target,
                        string.Format("{{Binding ElementName={0}}}", CleanName(forControl))
                    );
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.IsEnabled,
                        string.Format("{{Binding Path=Controls[{0}].Enabled}}", writer.ControlIndex(control))
                    );
                    writer.WriteAttribute(
                        WpfXmlWriterAttribute.Visibility,
                        string.Format("{{Binding Path=Controls[{0}].Visibility}}", writer.ControlIndex(control))
                    );
                }

                // A required parameter is marked on its label, not by colour alone. The venue's own
                // label text stays untouched: ContentStringFormat applies the marker at render time, so
                // Content still reads back as exactly what the strategy document declared, and a
                // screen reader announces the marker as part of the label.
                // A venue that already marks the requirement in its own label text keeps its marker
                // and gets none of ours - the FIXatdl conformance corpus carries label="Urgency:*" on
                // a use="required" parameter, which would otherwise render as "Urgency:* *".
                if (writer.IsRequired(control) && !label.TrimEnd().EndsWith('*'))
                {
                    writer.WriteAttribute(WpfXmlWriterAttribute.ContentStringFormat, "{}{0} *");
                    writer.WriteLiteralAttribute(
                        WpfXmlWriterAttribute.AutomationProperties_Name,
                        label.TrimEnd() + " *"
                    );
                }

                writer.WriteLiteralAttribute(WpfXmlWriterAttribute.Content, label);
            }
        }
    }
}
