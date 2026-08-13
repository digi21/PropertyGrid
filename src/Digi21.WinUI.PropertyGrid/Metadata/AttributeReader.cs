using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Digi21.WinUI.PropertyGrid;

// Turns the attributes on a property into the fields of a PropertyDescription.
//
// Two families say the same things. System.ComponentModel ([DisplayName], [Description],
// [Category]) is what WinForms and WPF property grids have always read; DataAnnotations [Display]
// packs the same four values into one attribute and is what a model shared with a web API tends to
// carry. Both are honoured, and the single-purpose System.ComponentModel attribute wins where they
// disagree: it can only mean one thing, so it is the clearer statement of intent.
internal static class AttributeReader
{
    internal static PropertyDescription Describe(PropertyInfo property, PropertyAccessor accessor)
    {
        Attribute[] attributes = Attribute.GetCustomAttributes(property, inherit: true);

        DisplayAttribute? display = Find<DisplayAttribute>(attributes);
        DisplayNameAttribute? displayName = Find<DisplayNameAttribute>(attributes);
        DescriptionAttribute? description = Find<DescriptionAttribute>(attributes);
        CategoryAttribute? category = Find<CategoryAttribute>(attributes);
        DefaultValueAttribute? defaultValue = Find<DefaultValueAttribute>(attributes);
        ExpandableAttribute? expandable = Find<ExpandableAttribute>(attributes)
            ?? (ExpandableAttribute?)Attribute.GetCustomAttribute(property.PropertyType, typeof(ExpandableAttribute), inherit: true);

        return new PropertyDescription
        {
            Name = property.Name,
            PropertyType = property.PropertyType,
            DeclaringType = property.DeclaringType,
            Accessor = accessor,
            DisplayName = displayName?.DisplayName ?? display?.GetName() ?? property.Name,
            HelpText = description?.Description ?? display?.GetDescription(),
            CategoryName = category?.Category ?? display?.GetGroupName(),
            Order = Find<PropertyOrderAttribute>(attributes)?.Order ?? display?.GetOrder() ?? int.MaxValue,
            IsBrowsable = IsBrowsable(attributes),
            IsReadOnly = IsReadOnly(attributes, accessor),
            HasDefaultValue = defaultValue is not null,
            DefaultValue = defaultValue?.Value,
            IsMergable = Find<MergablePropertyAttribute>(attributes)?.AllowMerge ?? true,
            EditorKey = Find<PropertyEditorAttribute>(attributes)?.Key,
            IsExpandable = expandable?.IsExpandable,
            Attributes = attributes,
        };
    }

    private static bool IsBrowsable(Attribute[] attributes)
    {
        if (Find<BrowsableAttribute>(attributes) is { } browsable)
        {
            return browsable.Browsable;
        }

        // [EditorBrowsable(Never)] is aimed at IntelliSense rather than at a property grid, but a
        // member hidden from the person writing the code has no business being offered to the person
        // running it either.
        return Find<EditorBrowsableAttribute>(attributes)?.State != EditorBrowsableState.Never;
    }

    private static bool IsReadOnly(Attribute[] attributes, PropertyAccessor accessor)
    {
        if (!accessor.CanWrite)
        {
            return true;
        }

        if (Find<ReadOnlyAttribute>(attributes)?.IsReadOnly == true)
        {
            return true;
        }

        return Find<EditableAttribute>(attributes) is { AllowEdit: false };
    }

    private static T? Find<T>(Attribute[] attributes)
        where T : Attribute
    {
        foreach (Attribute attribute in attributes)
        {
            if (attribute is T match)
            {
                return match;
            }
        }

        return null;
    }
}
