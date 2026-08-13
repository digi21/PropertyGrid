using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PropertyGridGallery;

// Opens the gallery, lets it settle, saves a picture of it and exits. Run it with:
//
//     dotnet run --project samples/PropertyGridGallery -- --screenshot gallery.png [dark]
//
// It is how the picture in the README is produced, and how a layout change can be checked without
// somebody sitting in front of the window.
internal sealed class ScreenshotRun(string outputPath, ElementTheme theme)
{
    private MainWindow? window;
    private int frame;

    internal void Run()
    {
        window = new MainWindow();

        if (window.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme;
        }

        window.Activate();
        CompositionTarget.Rendering += OnRendering;
    }

    private async void OnRendering(object? sender, object e)
    {
        frame++;

        if (frame == 5)
        {
            // Open one nested property, so the picture shows the indented child rows lining up with
            // everything else - which is the point of the whole layout.
            window?.OpenNestedRowForPicture();
        }

        if (frame < 20 || window?.Content is not FrameworkElement content)
        {
            return;
        }

        CompositionTarget.Rendering -= OnRendering;

        try
        {
            await Diagnostics.CaptureAsync(window, content, outputPath);
        }
        finally
        {
            window.Close();
            Application.Current.Exit();
        }
    }
}
