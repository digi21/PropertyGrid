using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>Shows the name and the explanation of the selected property under the grid.</summary>
public partial class PropertyGridDescriptionPane : Control
{
    /// <summary>Identifies the <see cref="Row"/> dependency property.</summary>
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row),
        typeof(PropertyGridPropertyRow),
        typeof(PropertyGridDescriptionPane),
        new PropertyMetadata(null, (d, _) => ((PropertyGridDescriptionPane)d).OnRowChanged()));

    /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(PropertyGridDescriptionPane),
        new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(PropertyGridDescriptionPane),
        new PropertyMetadata(string.Empty));

    private PropertyGridPropertyRow? subscribed;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridDescriptionPane"/> class.</summary>
    public PropertyGridDescriptionPane()
    {
        DefaultStyleKey = typeof(PropertyGridDescriptionPane);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();
    }

    /// <summary>Gets or sets the property being described.</summary>
    public PropertyGridPropertyRow? Row
    {
        get => (PropertyGridPropertyRow?)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    /// <summary>Gets the name shown at the top of the pane.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        private set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets the sentence shown under the name, or the reason the value was rejected.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        private set => SetValue(TextProperty, value);
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

        Update();
    }

    private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PropertyGridPropertyRow.HasErrors) or nameof(PropertyGridPropertyRow.ErrorMessage))
        {
            Update();
        }
    }

    private void Update()
    {
        Title = Row?.DisplayName ?? string.Empty;

        // While a value is rejected, why it was rejected is more useful than what the property is
        // for - and it is the one place with room to show the whole sentence.
        Text = Row is null
            ? string.Empty
            : Row.HasErrors ? Row.ErrorMessage ?? string.Empty : Row.HelpText ?? string.Empty;

        VisualStateManager.GoToState(this, Row?.HasErrors == true ? "Invalid" : "Valid", true);
    }
}
