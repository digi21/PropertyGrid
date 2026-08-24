using Microsoft.UI.Xaml;

namespace Digi21.WinUI.PropertyGrid;

// Makes the library's brushes, metrics and editor templates reachable from the application's
// resources, which is what lets an application override any of them.
//
// A {ThemeResource} used inside a control template resolves against the dictionary the template was
// parsed in before it looks at Application.Resources. Keeping the keys in Themes/Generic.xaml next
// to the templates would therefore make them win over anything the application declares, and the
// grid could only be recoloured - or given a different editor for booleans - by retemplating it.
//
// The dictionary is merged at the bottom of the collection, so it acts as a set of defaults: keys
// the application declares directly, and dictionaries it merges itself, are looked up first. This
// mirrors what XamlControlsResources does for WinUI's own controls, except that the application
// does not have to add anything to its App.xaml.
internal static class PropertyGridThemeResources
{
    private const string Source = "ms-appx:///Digi21.WinUI.PropertyGrid/Themes/PropertyGridResources.xaml";

    private static bool merged;
    private static bool unhosted;

    // The application whose resources these keys are read from, where there is one.
    //
    // Application.Current answers null while the XAML designer or a unit test builds something
    // without an application. In a process where no WinUI runtime was ever activated it does not
    // answer at all: the property is a WinRT activation, and asking for it throws
    // REGDB_E_CLASSNOTREG. Both mean the same thing here - nothing is declared, so the built-in
    // defaults stand - and the second is remembered, because a throw is expensive and a sentence is
    // built for every rejected edit.
    private static Application? Host
    {
        get
        {
            if (unhosted)
            {
                return null;
            }

            try
            {
                return Application.Current;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException)
            {
                unhosted = true;
                return null;
            }
        }
    }

    // Merges the dictionary the first time a grid control is created, which is always before any of
    // their templates is applied.
    internal static void Ensure()
    {
        if (merged || Host is not { } application)
        {
            return;
        }

        merged = true;
        application.Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = new Uri(Source) });
    }

    // Stands in for the application's resources where there is no application to have any. Only a
    // test assigns it, and only so that the one thing about these keys that cannot be checked by
    // reading them - that an override reaches the grid whether it was declared before the first
    // grid existed or after - can be checked at all.
    internal static IReadOnlyDictionary<string, object>? Substitute { get; set; }

    // Reads one of the library's keys from the application's resources, falling back to the
    // built-in value when it is not defined (or when there is no application, as in a test).
    //
    // For the values no template can supply: the ones the layout imposes from code, because the
    // layout cannot express them - a splitter's thickness, the width of an indent - and the
    // sentences the grid builds where there is no element to ask. Being the same in every theme,
    // they live in the root of the dictionary and not in its theme dictionaries.
    internal static T Value<T>(string key, T fallback)
    {
        Ensure();

        if (Substitute is { } substitute)
        {
            return substitute.TryGetValue(key, out object? stood) && stood is T typedStand ? typedStand : fallback;
        }

        return Host?.Resources is { } resources
            && resources.TryGetValue(key, out object? value)
            && value is T typed
                ? typed
                : fallback;
    }

    // Looks up an editor template by name. Missing keys are normal rather than exceptional: the
    // library reserves names it ships no template for, and a property is free to ask for one an
    // application never declared.
    internal static DataTemplate? Template(string key)
    {
        Ensure();

        return Host?.Resources is { } resources
            && resources.TryGetValue(key, out object? value)
                ? value as DataTemplate
                : null;
    }
}
