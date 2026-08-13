namespace Digi21.WinUI.PropertyGrid;

// Puts a type's properties in the order the grid shows them.
//
// The result is a flat list, not a tree of categories, because the grid renders one flat list of
// rows: in every categorized mode the properties of a category come out consecutively, so building
// the headers is a walk over consecutive runs rather than a second grouping pass.
internal static class PropertyDescriptionSorter
{
    // The category a property lands in when it never said. Last in every categorized mode, because a
    // pile of unclassified properties above the ones somebody bothered to classify is backwards.
    internal const string DefaultCategoryName = "Misc";

    internal static string CategoryOf(PropertyDescription description, string defaultCategoryName) =>
        string.IsNullOrWhiteSpace(description.CategoryName) ? defaultCategoryName : description.CategoryName;

    internal static IReadOnlyList<PropertyDescription> Sort(
        IReadOnlyList<PropertyDescription> properties,
        PropertySort sort,
        string defaultCategoryName = DefaultCategoryName)
    {
        if (properties.Count < 2)
        {
            return properties;
        }

        bool byName = sort is PropertySort.Alphabetical or PropertySort.CategorizedAlphabetical;
        bool categorized = sort is PropertySort.Categorized or PropertySort.CategorizedAlphabetical;

        Dictionary<string, int> categoryRanks = categorized
            ? RankCategories(properties, sort, defaultCategoryName)
            : [];

        // The input order is the tie-breaker for everything, so it has to be captured before sorting
        // rather than read back off the sorted result.
        List<(PropertyDescription Description, int Index)> indexed = [];
        for (int index = 0; index < properties.Count; index++)
        {
            indexed.Add((properties[index], index));
        }

        IOrderedEnumerable<(PropertyDescription Description, int Index)> ordered = categorized
            ? indexed.OrderBy(entry => categoryRanks[CategoryOf(entry.Description, defaultCategoryName)])
                .ThenBy(entry => entry.Description.Order)
            : indexed.OrderBy(entry => entry.Description.Order);

        ordered = byName
            ? ordered.ThenBy(entry => entry.Description.DisplayName, StringComparer.CurrentCulture)
            : ordered;

        return [.. ordered.ThenBy(entry => entry.Index).Select(entry => entry.Description)];
    }

    private static Dictionary<string, int> RankCategories(
        IReadOnlyList<PropertyDescription> properties,
        PropertySort sort,
        string defaultCategoryName)
    {
        List<string> names = [];
        foreach (PropertyDescription description in properties)
        {
            string category = CategoryOf(description, defaultCategoryName);
            if (!names.Contains(category, StringComparer.Ordinal))
            {
                names.Add(category);
            }
        }

        if (sort == PropertySort.CategorizedAlphabetical)
        {
            names.Sort(StringComparer.CurrentCulture);
        }

        Dictionary<string, int> ranks = new(StringComparer.Ordinal);
        for (int rank = 0; rank < names.Count; rank++)
        {
            ranks[names[rank]] = rank;
        }

        // Whatever order the rest came out in, the catch-all category goes to the end.
        if (ranks.ContainsKey(defaultCategoryName))
        {
            ranks[defaultCategoryName] = int.MaxValue;
        }

        return ranks;
    }
}
