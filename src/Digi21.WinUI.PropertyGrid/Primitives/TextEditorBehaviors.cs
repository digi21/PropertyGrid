using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>Makes a text box in an editor template commit on Enter and give up on Escape.</summary>
/// <remarks>
/// A <see cref="DataTemplate"/> declared in a resource dictionary cannot carry event handlers, so
/// behaviour an editor needs has to arrive as an attached property. Both built-in text editors set
/// these, and a replacement editor is welcome to.
/// </remarks>
public static class TextEditorBehaviors
{
    /// <summary>Identifies the CommitOnEnter attached property.</summary>
    public static readonly DependencyProperty CommitOnEnterProperty = DependencyProperty.RegisterAttached(
        "CommitOnEnter",
        typeof(bool),
        typeof(TextEditorBehaviors),
        new PropertyMetadata(false, OnCommitOnEnterChanged));

    /// <summary>Identifies the SelectAllOnFocus attached property.</summary>
    public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached(
        "SelectAllOnFocus",
        typeof(bool),
        typeof(TextEditorBehaviors),
        new PropertyMetadata(false, OnSelectAllOnFocusChanged));

    /// <summary>Gets whether the text box writes its value back when Enter is pressed.</summary>
    /// <param name="element">The text box.</param>
    /// <returns>Whether the behaviour is on.</returns>
    public static bool GetCommitOnEnter(TextBox element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return (bool)element.GetValue(CommitOnEnterProperty);
    }

    /// <summary>Sets whether the text box writes its value back when Enter is pressed.</summary>
    /// <param name="element">The text box.</param>
    /// <param name="value">Whether to turn the behaviour on.</param>
    public static void SetCommitOnEnter(TextBox element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.SetValue(CommitOnEnterProperty, value);
    }

    /// <summary>Gets whether the text box selects everything in it when it takes focus.</summary>
    /// <param name="element">The text box.</param>
    /// <returns>Whether the behaviour is on.</returns>
    public static bool GetSelectAllOnFocus(TextBox element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return (bool)element.GetValue(SelectAllOnFocusProperty);
    }

    /// <summary>Sets whether the text box selects everything in it when it takes focus.</summary>
    /// <param name="element">The text box.</param>
    /// <param name="value">Whether to turn the behaviour on.</param>
    public static void SetSelectAllOnFocus(TextBox element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.SetValue(SelectAllOnFocusProperty, value);
    }

    private static void OnCommitOnEnterChanged(DependencyObject element, DependencyPropertyChangedEventArgs arguments)
    {
        if (element is not TextBox box)
        {
            return;
        }

        box.KeyDown -= OnKeyDown;

        if (arguments.NewValue is true)
        {
            box.KeyDown += OnKeyDown;
        }
    }

    private static void OnSelectAllOnFocusChanged(DependencyObject element, DependencyPropertyChangedEventArgs arguments)
    {
        if (element is not TextBox box)
        {
            return;
        }

        box.GotFocus -= OnGotFocus;

        if (arguments.NewValue is true)
        {
            box.GotFocus += OnGotFocus;
        }
    }

    private static void OnKeyDown(object sender, KeyRoutedEventArgs arguments)
    {
        if (sender is not TextBox box)
        {
            return;
        }

        switch (arguments.Key)
        {
            case VirtualKey.Enter:
                // The binding commits on lost focus by default, and waiting for that after Enter
                // feels like the grid ignored the keystroke.
                box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                arguments.Handled = true;
                break;

            case VirtualKey.Escape:
                // There is no UpdateTarget on a WinUI binding expression, so putting back what the
                // row holds means reading it and assigning it.
                if (box.DataContext is PropertyGridPropertyRow row)
                {
                    box.Text = row.Text;
                    box.SelectionStart = box.Text.Length;
                }

                arguments.Handled = true;
                break;
        }
    }

    private static void OnGotFocus(object sender, RoutedEventArgs arguments)
    {
        if (sender is TextBox box)
        {
            box.SelectAll();
        }
    }
}
