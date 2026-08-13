using System.Globalization;
using Digi21.WinUI.PropertyGrid;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

        // The grid asks; the application picks. It knows which dialog is right, where it should
        // start, and - the part a library cannot do - the window handle a WinUI picker needs.
        Grid.BrowseRequested += async (_, arguments) =>
        {
            nint handle = WindowNative.GetWindowHandle(this);

            if (arguments.Kind == FilePathKind.Folder)
            {
                FolderPicker folders = new();
                InitializeWithWindow.Initialize(folders, handle);
                folders.FileTypeFilter.Add("*");

                if (await folders.PickSingleFolderAsync() is { } folder)
                {
                    arguments.Row.Value = folder.Path;
                }

                return;
            }

            if (arguments.Kind == FilePathKind.SaveFile)
            {
                FileSavePicker saving = new();
                InitializeWithWindow.Initialize(saving, handle);
                saving.FileTypeChoices.Add("Data", [.. Extensions(arguments)]);

                if (await saving.PickSaveFileAsync() is { } target)
                {
                    arguments.Row.Value = target.Path;
                }

                return;
            }

            FileOpenPicker opening = new();
            InitializeWithWindow.Initialize(opening, handle);
            foreach (string extension in Extensions(arguments))
            {
                opening.FileTypeFilter.Add(extension);
            }

            // The write can happen long after the handler returned; the row is observable, so the
            // grid picks it up without anything else being told.
            if (await opening.PickSingleFileAsync() is { } chosen)
            {
                arguments.Row.Value = chosen.Path;
            }
        };

        // The same bargain as browsing: the grid offers the button, the application decides what
        // opens. Here a list of numbers, but nothing stops it being a whole editor for a type only
        // this application knows about.
        Grid.EditRequested += async (_, arguments) =>
        {
            if (arguments.Row.Value is not IList<int> scales)
            {
                return;
            }

            TextBox box = new()
            {
                AcceptsReturn = true,
                Text = string.Join(Environment.NewLine, scales),
            };

            ContentDialog dialog = new()
            {
                Title = arguments.Row.DisplayName,
                Content = box,
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,

                // A ContentDialog with no XamlRoot throws in WinUI 3, and it is the single easiest
                // thing to forget.
                XamlRoot = Grid.XamlRoot,
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            scales.Clear();
            foreach (string line in box.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(line.Trim(), CultureInfo.CurrentCulture, out int scale))
                {
                    scales.Add(scale);
                }
            }

            // The list was edited in place rather than replaced, so the row is told to read it again.
            arguments.Row.Refresh();
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

    internal void ToggleThemeForPicture() => OnToggleTheme(this, new RoutedEventArgs());

    // Reached from a Click in the editor template. The data context of an editor template is the
    // row, so a handler knows which property it was pressed for without being told.
    private void OnResetOpacity(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PropertyGridPropertyRow row })
        {
            row.Value = 80;
            Trace.Text = $"Reset {row.DisplayName} from a button inside its editor.";
        }
    }

    // Which radio is filled in. There is no converter to bind IsChecked through, and a sample that
    // writes the value but never shows it is worse than no sample.
    private void OnQualityLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { DataContext: PropertyGridPropertyRow row, Tag: string option } radio)
        {
            radio.IsChecked = string.Equals(option, row.Text, StringComparison.Ordinal);
        }
    }

    private void OnQualityChosen(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { DataContext: PropertyGridPropertyRow row, Tag: string chosen })
        {
            row.Text = chosen;
        }
    }

    private async void OnOpenDocumentation(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PropertyGridPropertyRow row }
            && Uri.TryCreate(row.Text, UriKind.Absolute, out Uri? target))
        {
            await Windows.System.Launcher.LaunchUriAsync(target);
        }
    }

    private static IReadOnlyList<string> Extensions(PropertyGridBrowseRequestedEventArgs arguments) =>
        arguments.Extensions.Count > 0 ? arguments.Extensions : ["*"];

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
        Model.Text = "Changed " + Random.Shared.Next(100, 999);
        Model.Server.Port = Random.Shared.Next(1, 65536);
    }

    private void OnExpandAll(object sender, RoutedEventArgs e) => Grid.ExpandAllCategories();

    private void OnCollapseAll(object sender, RoutedEventArgs e) => Grid.CollapseAllCategories();

    private void OnToggleReadOnly(object sender, RoutedEventArgs e) =>
        Grid.IsReadOnly = ReadOnlyToggle.IsChecked == true;

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        if (Content is not FrameworkElement root)
        {
            return;
        }

        // ActualTheme, not RequestedTheme. RequestedTheme starts at Default, so asking it what the
        // window looks like right now gets the wrong answer: on a machine set to dark, the first
        // press would set Dark - changing nothing anybody can see - and only the second would work.
        root.RequestedTheme = root.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
    }
}

