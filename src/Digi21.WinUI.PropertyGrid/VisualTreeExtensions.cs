using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Digi21.WinUI.PropertyGrid;

internal static class VisualTreeExtensions
{
    // Walks up the visual tree looking for the nearest ancestor of type T.
    //
    // WinUI has no RelativeSource AncestorType, so this is the only way for something inside a data
    // template to reach the control that owns it.
    internal static T? FindAncestor<T>(this DependencyObject start)
        where T : DependencyObject
    {
        DependencyObject? current = VisualTreeHelper.GetParent(start);
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    // Walks down the visual tree looking for the first descendant of type T.
    internal static T? FindDescendant<T>(this DependencyObject start)
        where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(start);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(start, index);
            if (child is T match)
            {
                return match;
            }

            if (child.FindDescendant<T>() is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }
}
