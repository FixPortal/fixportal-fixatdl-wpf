using System.Windows;
using System.Windows.Controls;

namespace FixPortal.FixAtdl.Wpf.Controls;

/// <summary>
/// Marks a control as invalid by painting its border and text with <c>ValidationErrorBrush</c> while
/// its view model reports errors, and handing both properties straight back to the host's theme when
/// it does not.
/// </summary>
/// <remarks>
/// <para>
/// This exists because the obvious implementation is wrong for a library. The natural way to express
/// "red border when invalid" is an implicit <c>Style</c> for <see cref="ComboBox"/> or
/// <see cref="ListBox"/>, which is what this library used to carry. Those are the host's types, not
/// ours, and the panel's resource dictionary is merged into the innermost resource scope in the tree
/// - so an implicit style there shadows the host's own implicit style completely, for every property,
/// not just the one the trigger touches.
/// </para>
/// <para>
/// Measured under WPF's Fluent dark theme: the shadowed <see cref="ComboBox"/> fell back to the
/// legacy template (a light <c>LinearGradientBrush</c> chrome with <c>#FF000000</c> text) while its
/// items, which our dictionary said nothing about, kept Fluent's <c>#FFFFFFFF</c> foreground. White
/// text on a near-white popup: an unreadable dropdown.
/// </para>
/// <para>
/// Setting a local value and later clearing it touches exactly one property and leaves the rest of
/// the host's style intact. <see cref="FrameworkElement.SetResourceReference"/> rather than a direct
/// brush so a host can still redefine <c>ValidationErrorBrush</c>, and
/// <see cref="DependencyObject.ClearValue(DependencyProperty)"/> rather than restoring a captured
/// value so the property resolves afresh through whatever the host's theme now says. A converter
/// returning <see cref="DependencyProperty.UnsetValue"/> does not work here: measured, it leaves the
/// property null rather than falling through to the style.
/// </para>
/// </remarks>
public static class ErrorCue
{
    /// <summary>
    /// Attached property bound to a control view model's <c>HasErrors</c>. True paints the cue; false
    /// removes it.
    /// </summary>
    public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.RegisterAttached(
        "HasErrors",
        typeof(bool),
        typeof(ErrorCue),
        new PropertyMetadata(false, OnHasErrorsChanged)
    );

    /// <summary>Sets whether <paramref name="element"/> is painted as invalid.</summary>
    public static void SetHasErrors(DependencyObject element, bool value) => element.SetValue(HasErrorsProperty, value);

    /// <summary>Gets whether <paramref name="element"/> is painted as invalid.</summary>
    public static bool GetHasErrors(DependencyObject element) => (bool)element.GetValue(HasErrorsProperty);

    private static void OnHasErrorsChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        if (element is not Control control)
        {
            return;
        }

        if (e.NewValue is true)
        {
            control.SetResourceReference(Control.BorderBrushProperty, "ValidationErrorBrush");
            control.SetResourceReference(Control.ForegroundProperty, "ValidationErrorBrush");
        }
        else
        {
            control.ClearValue(Control.BorderBrushProperty);
            control.ClearValue(Control.ForegroundProperty);
        }
    }
}
