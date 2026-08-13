using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Digi21.WinUI.PropertyGrid.Tests;

// The shapes the tests reflect over. They are deliberately awkward: every one of these declarations
// is a case the discovery code has to get right, and several of them (init-only setters, `new`
// shadowing, a getter that throws) are the ones that look like ordinary properties until they are not.
#pragma warning disable CA1822 // members are instance members on purpose: that is what is being tested

internal class DiscoverySubject
{
    public string Plain { get; set; } = string.Empty;

    public string NoSetter => string.Empty;

    public string PrivateSetter { get; private set; } = string.Empty;

    public string InitOnly { get; init; } = string.Empty;

    [ReadOnly(true)]
    public string MarkedReadOnly { get; set; } = string.Empty;

    [Editable(false)]
    public string NotEditable { get; set; } = string.Empty;

    public string WriteOnly
    {
        set => _ = value;
    }

    [Browsable(false)]
    public string NotBrowsable { get; set; } = string.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string HiddenFromEditors { get; set; } = string.Empty;

    public static string Static { get; set; } = string.Empty;

    internal string Internal { get; set; } = string.Empty;

    public string this[int index] => index.ToString();
}

internal class ThrowingSubject
{
    public string Fine { get; set; } = "fine";

    public string Broken => throw new InvalidOperationException("the getter said no");

    public string Rejecting
    {
        get => string.Empty;
        set => throw new ArgumentException("the setter said no");
    }
}

internal class DescribedSubject
{
    [DisplayName("Full name")]
    [Description("What to call the person.")]
    [Category("Identity")]
    [PropertyOrder(1)]
    public string Name { get; set; } = string.Empty;

    [DefaultValue(42)]
    public int WithDefault { get; set; } = 42;

    [MergableProperty(false)]
    public string NotMergable { get; set; } = string.Empty;

    [PropertyEditor("Percent")]
    public int Percentage { get; set; }

    [Expandable]
    public object? Openable { get; set; }

    [Expandable(false)]
    public ExpandableByType? ClosedByProperty { get; set; }

    public ExpandableByType? OpenByType { get; set; }
}

[Expandable]
internal class ExpandableByType
{
    public int Value { get; set; }
}

internal class AnnotatedSubject
{
    [Display(Name = "Etiqueta", Description = "Una descripcion.", GroupName = "Grupo", Order = 3)]
    public string FromDisplay { get; set; } = string.Empty;

    [Display(Name = "From Display", Description = "From Display", GroupName = "From Display", Order = 7)]
    [DisplayName("From DisplayName")]
    [Description("From Description")]
    [Category("From Category")]
    [PropertyOrder(2)]
    public string Both { get; set; } = string.Empty;
}

internal class ShadowBase
{
    public virtual string Value { get; set; } = "base";

    public string OnlyOnBase { get; set; } = string.Empty;
}

internal class ShadowDerived : ShadowBase
{
    public new string Value { get; set; } = "derived";

    public string OnlyOnDerived { get; set; } = string.Empty;
}

internal class UncategorizedFirstSubject
{
    public int Loose { get; set; }

    [Category("Zulu")]
    public int Zebra { get; set; }

    [Category("Alpha")]
    public int Apple { get; set; }

    [Category("Zulu")]
    public int Zulu { get; set; }
}

internal class UnorderedSubject
{
    public int Zebra { get; set; }

    public int Apple { get; set; }

    public int Mango { get; set; }
}

internal interface INamed
{
    string Name { get; set; }
}

internal interface IDescribed : INamed
{
    string Summary { get; set; }
}

#pragma warning restore CA1822
