using System.Globalization;
using Digi21.WinUI.PropertyGrid;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PropertyGridGallery;

public sealed partial class MainWindow : Window
{
    private int changes;

    public MainWindow()
    {
        InitializeComponent();
        Title = "PropertyGrid Gallery";

        SortBox.ItemsSource = Enum.GetValues<PropertySort>();
        SortBox.SelectedItem = Grid.PropertySort;

        // The Opacity property is an ordinary int; the event is what makes this one use the slider
        // registered in the markup, without the model having to know a control exists.
        Grid.EditorSelecting += (_, arguments) =>
        {
            if (arguments.Row.Name == nameof(SampleModel.Opacity))
            {
                arguments.Template = Grid.EditorTemplates?.Resolve(typeof(int), null, "Percent");
            }
        };

        Grid.PropertyValueChanged += (_, arguments) =>
        {
            changes++;
            Trace.Text = string.Format(
                CultureInfo.CurrentCulture,
                "{0} changes. Last: {1} = {2}",
                changes,
                arguments.Row.DisplayName,
                arguments.NewValue ?? "(none)");
        };
    }

    public SampleModel Model { get; } = new();

    internal void OpenNestedRowForPicture()
    {
        if (Grid.FindRow(nameof(SampleModel.Server)) is { IsExpandable: true } server)
        {
            server.IsExpanded = true;
        }

        Grid.SelectProperty(nameof(SampleModel.Opacity));
    }

    private void OnSortChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SortBox.SelectedItem is PropertySort sort)
        {
            Grid.PropertySort = sort;
        }
    }

    private void OnChangeFromOutside(object sender, RoutedEventArgs e)
    {
        // Nothing tells the grid about this. It hears about it because the model raises
        // PropertyChanged and the grid is listening - weakly.
        Model.Opacity = Random.Shared.Next(0, 101);
        Model.Name = "Parcels " + Random.Shared.Next(100, 999);
        Model.Server.Port = Random.Shared.Next(1, 65536);
    }

    private void OnExpandAll(object sender, RoutedEventArgs e) => Grid.ExpandAllCategories();

    private void OnCollapseAll(object sender, RoutedEventArgs e) => Grid.CollapseAllCategories();

    private void OnToggleReadOnly(object sender, RoutedEventArgs e) =>
        Grid.IsReadOnly = ReadOnlyToggle.IsChecked == true;

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        if (Content is FrameworkElement root)
        {
            root.RequestedTheme = root.RequestedTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        }
    }
}

