using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>One member of an enumeration, as a combo box shows it.</summary>
/// <param name="Value">The member.</param>
/// <param name="Name">The name of the member as it is declared in code.</param>
/// <param name="DisplayName">The label to show, taken from the member's attributes when it has them.</param>
/// <param name="Description">The sentence explaining the member, if it carries one.</param>
public sealed record EnumMemberRow(object Value, string Name, string DisplayName, string? Description)
{
    /// <inheritdoc />
    public override string ToString() => DisplayName;
}

/// <summary>One member of a <see cref="FlagsAttribute"/> enumeration, as a checklist shows it.</summary>
public sealed class FlagMemberRow : INotifyPropertyChanged
{
    private bool isChecked;

    internal FlagMemberRow(EnumMemberRow member, ulong bits)
    {
        Member = member;
        Bits = bits;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the member this entry stands for.</summary>
    public EnumMemberRow Member { get; }

    /// <summary>Gets the label to show.</summary>
    public string DisplayName => Member.DisplayName;

    /// <summary>Gets the sentence explaining the member, if it carries one.</summary>
    public string? Description => Member.Description;

    /// <summary>Gets or sets a value indicating whether the flag is set.</summary>
    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value)
            {
                return;
            }

            isChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            Toggled?.Invoke(this, EventArgs.Empty);
        }
    }

    internal ulong Bits { get; }

    internal event EventHandler? Toggled;

    internal void SetCheckedQuietly(bool value)
    {
        if (isChecked == value)
        {
            return;
        }

        isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
    }
}

// Reads the members of an enumeration once per type: the values, the labels their attributes ask
// for, and the bit patterns a flags enumeration needs.
internal static class EnumInfo
{
    private static readonly Dictionary<Type, EnumMemberRow[]> Cache = [];

    internal static IReadOnlyList<EnumMemberRow> MembersOf(Type enumType)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(enumType, out EnumMemberRow[]? cached))
            {
                return cached;
            }

            List<EnumMemberRow> members = [];
            foreach (object value in Enum.GetValues(enumType))
            {
                string name = Enum.GetName(enumType, value) ?? value.ToString() ?? string.Empty;
                FieldInfo? field = enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);

                DisplayAttribute? display = field?.GetCustomAttribute<DisplayAttribute>();
                DescriptionAttribute? description = field?.GetCustomAttribute<DescriptionAttribute>();

                members.Add(new EnumMemberRow(
                    value,
                    name,
                    display?.GetName() ?? name,
                    description?.Description ?? display?.GetDescription()));
            }

            EnumMemberRow[] result = [.. members];
            Cache[enumType] = result;
            return result;
        }
    }

    internal static bool IsFlags(Type enumType) => enumType.IsDefined(typeof(FlagsAttribute), inherit: false);

    internal static ulong BitsOf(object value) =>
        Type.GetTypeCode(value.GetType()) switch
        {
            TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 =>
                unchecked((ulong)Convert.ToInt64(value, null)),
            _ => Convert.ToUInt64(value, null),
        };
}
