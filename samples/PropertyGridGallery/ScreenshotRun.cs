using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace PropertyGridGallery;

// Opens the gallery, lets it settle, saves a picture of it and exits. Run it with:
//
//     dotnet run --project samples/PropertyGridGallery -- --screenshot gallery.png [light|dark]
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

        window.AppWindow.Resize(new Windows.Graphics.SizeInt32(1180, 1180));
        window.Activate();
        CompositionTarget.Rendering += OnRendering;
    }

    private static async Task CaptureAsync(FrameworkElement element, string path)
    {
        RenderTargetBitmap bitmap = new();
        await bitmap.RenderAsync(element);

        IBuffer pixels = await bitmap.GetPixelsAsync();

        using InMemoryRandomAccessStream stream = new();
        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            (uint)bitmap.PixelWidth,
            (uint)bitmap.PixelHeight,
            96,
            96,
            pixels.ToArray());
        await encoder.FlushAsync();

        using Stream managed = stream.AsStreamForRead();
        using FileStream file = File.Create(path);
        await managed.CopyToAsync(file);
    }

    private async void OnRendering(object? sender, object e)
    {
        frame++;

        if (frame == 5)
        {
            // Open one nested property, so the picture shows the indented child rows lining up with
            // everything else - which is the point of the whole layout.
            window?.OpenNestedRowForPicture();

            // With no theme asked for, press the toggle once. That is the case where the window is
            // still on ElementTheme.Default, and it is the one that used to need two presses.
            if (theme == ElementTheme.Default)
            {
                window?.ToggleThemeForPicture();
            }
        }

        if (frame < 20 || window?.Content is not FrameworkElement content)
        {
            return;
        }

        CompositionTarget.Rendering -= OnRendering;

        try
        {
            await CaptureAsync(content, outputPath);
        }
        finally
        {
            window.Close();
            Application.Current.Exit();
        }
    }
}
