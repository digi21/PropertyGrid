using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>Renders the header of a group of properties, and opens or closes the group.</summary>
public partial class PropertyGridCategoryHeader : Control
{
    /// <summary>Identifies the <see cref="Row"/> dependency property.</summary>
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row),
        typeof(PropertyGridCategoryRow),
        typeof(PropertyGridCategoryHeader),
        new PropertyMetadata(null, (d, _) => ((PropertyGridCategoryHeader)d).OnRowChanged()));

    private PropertyGridCategoryRow? subscribed;
    private bool isPointerOver;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridCategoryHeader"/> class.</summary>
    public PropertyGridCategoryHeader()
    {
        DefaultStyleKey = typeof(PropertyGridCategoryHeader);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();
    }

    /// <summary>Gets or sets the category the header stands for.</summary>
    public PropertyGridCategoryRow? Row
    {
        get => (PropertyGridCategoryRow?)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateStates(false);
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);
        isPointerOver = true;
        UpdateStates(true);
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        isPointerOver = false;
        UpdateStates(true);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);

        // The whole header is the hit target, not just the chevron: a category name is a much
        // easier thing to aim at than a twelve-pixel glyph.
        Toggle();
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case VirtualKey.Space:
            case VirtualKey.Enter:
                Toggle();
                e.Handled = true;
                break;

            case VirtualKey.Left when Row is { IsExpanded: true }:
                Row.IsExpanded = false;
                e.Handled = true;
                break;

            case VirtualKey.Right when Row is { IsExpanded: false }:
                Row.IsExpanded = true;
                e.Handled = true;
                break;
        }
    }

    internal void PrepareForRecycling()
    {
        isPointerOver = false;
        Row = null;
    }

    private void OnRowChanged()
    {
        if (subscribed is not null)
        {
            subscribed.PropertyChanged -= OnRowPropertyChanged;
        }

        subscribed = Row;

        if (subscribed is not null)
        {
            subscribed.PropertyChanged += OnRowPropertyChanged;
        }

        UpdateStates(false);
    }

    private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PropertyGridCategoryRow.IsExpanded))
        {
            UpdateStates(true);
        }
    }

    private void Toggle()
    {
        if (Row is { } row)
        {
            row.IsExpanded = !row.IsExpanded;
        }
    }

    private void UpdateStates(bool useTransitions)
    {
        VisualStateManager.GoToState(this, isPointerOver ? "PointerOver" : "Normal", useTransitions);
        VisualStateManager.GoToState(this, Row?.IsExpanded == false ? "Collapsed" : "Expanded", useTransitions);
    }
}
