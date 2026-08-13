# Translations for Digi21.WinUI.PropertyGrid

Nine languages, contributed by the application that asked for them: lop, a point-cloud editor shipped
on the Microsoft Store in these same nine.

There are two halves, because the library has two. The five strings a template paints are resource
keys, so they go in a `ResourceDictionary` merged into `App.xaml`; the ones the grid builds at run
time are `PropertyGridStrings`, set once from code.

**Set them early, but not too early.** In an unpackaged WinUI 3 application that means `OnLaunched`
and *not* the constructor of `App`: reading `Application.Current.Resources` there throws a
`COMException 0x8000FFFF` — the application dictionary does not exist yet, `InitializeComponent` only
registers where it comes from — and the process dies before showing a window, with no message.

About the placeholders: `NotAValidFormat` takes the offending text and then the type name,
`RequiredValueFormat` takes the type name, `CannotConvertFormat` takes both type names, and
`CollectionSummaryFormat` takes a count. The order is the same in all nine here.

The type names are deliberately what a person would say — "whole number", not `Int32` — because they
are read inside a sentence explaining why an edit was rejected.

Fourteen strings per language, and every one of them reaches the screen. The contributed file had
sixteen: `MultipleValues` and `NullValue` were declared on `PropertyGridStrings` before anything used
them, and rather than have nine translators spend an afternoon on text that never appears they were
taken off the class. `MultipleValues` comes back with multiple selection; `NullValue` when a summary
cell has somewhere to put it that is not the text a user edits.

`LocalisationTests` reads all fourteen, so this file cannot quietly drift away from the class.


## English (`en`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Search properties</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancel</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Browse…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Edit…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "General";
PropertyGridStrings.NotAValidFormat = "“{0}” is not a valid value for a {1} field.";
PropertyGridStrings.RequiredValueFormat = "A {0} value is required.";
PropertyGridStrings.CannotConvertFormat = "A {0} cannot be turned into a {1}.";
PropertyGridStrings.CollectionSummaryFormat = "{0} items";
PropertyGridStrings.WholeNumberName = "whole number";
PropertyGridStrings.NumberName = "number";
PropertyGridStrings.BooleanName = "true or false";
PropertyGridStrings.CharacterName = "character";
PropertyGridStrings.TextName = "text";
PropertyGridStrings.DateTimeName = "date and time";
PropertyGridStrings.DateName = "date";
PropertyGridStrings.TimeName = "time";
PropertyGridStrings.DurationName = "duration";
```


## Español (`es`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Buscar propiedades</x:String>
    <x:String x:Key="PropertyGridOkButtonText">Aceptar</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancelar</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Examinar…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editar…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "General";
PropertyGridStrings.NotAValidFormat = "«{0}» no es un valor válido para un campo de tipo {1}.";
PropertyGridStrings.RequiredValueFormat = "Hace falta un valor de tipo {0}.";
PropertyGridStrings.CannotConvertFormat = "No se puede pasar de {0} a {1}.";
PropertyGridStrings.CollectionSummaryFormat = "{0} elementos";
PropertyGridStrings.WholeNumberName = "número entero";
PropertyGridStrings.NumberName = "número";
PropertyGridStrings.BooleanName = "sí o no";
PropertyGridStrings.CharacterName = "carácter";
PropertyGridStrings.TextName = "texto";
PropertyGridStrings.DateTimeName = "fecha y hora";
PropertyGridStrings.DateName = "fecha";
PropertyGridStrings.TimeName = "hora";
PropertyGridStrings.DurationName = "duración";
```


## Català (`ca`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Cerca propietats</x:String>
    <x:String x:Key="PropertyGridOkButtonText">D’acord</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancel·la</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Examina…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Edita…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "General";
PropertyGridStrings.NotAValidFormat = "«{0}» no és un valor vàlid per a un camp de tipus {1}.";
PropertyGridStrings.RequiredValueFormat = "Cal un valor de tipus {0}.";
PropertyGridStrings.CannotConvertFormat = "No es pot passar de {0} a {1}.";
PropertyGridStrings.CollectionSummaryFormat = "{0} elements";
PropertyGridStrings.WholeNumberName = "nombre enter";
PropertyGridStrings.NumberName = "nombre";
PropertyGridStrings.BooleanName = "sí o no";
PropertyGridStrings.CharacterName = "caràcter";
PropertyGridStrings.TextName = "text";
PropertyGridStrings.DateTimeName = "data i hora";
PropertyGridStrings.DateName = "data";
PropertyGridStrings.TimeName = "hora";
PropertyGridStrings.DurationName = "durada";
```


## Galego (`gl`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Buscar propiedades</x:String>
    <x:String x:Key="PropertyGridOkButtonText">Aceptar</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancelar</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Examinar…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editar…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "Xeral";
PropertyGridStrings.NotAValidFormat = "«{0}» non é un valor válido para un campo de tipo {1}.";
PropertyGridStrings.RequiredValueFormat = "Fai falta un valor de tipo {0}.";
PropertyGridStrings.CannotConvertFormat = "Non se pode pasar de {0} a {1}.";
PropertyGridStrings.CollectionSummaryFormat = "{0} elementos";
PropertyGridStrings.WholeNumberName = "número enteiro";
PropertyGridStrings.NumberName = "número";
PropertyGridStrings.BooleanName = "si ou non";
PropertyGridStrings.CharacterName = "carácter";
PropertyGridStrings.TextName = "texto";
PropertyGridStrings.DateTimeName = "data e hora";
PropertyGridStrings.DateName = "data";
PropertyGridStrings.TimeName = "hora";
PropertyGridStrings.DurationName = "duración";
```


## Euskara (`eu`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Bilatu propietateak</x:String>
    <x:String x:Key="PropertyGridOkButtonText">Ados</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Utzi</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Arakatu…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editatu…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "Orokorra";
PropertyGridStrings.NotAValidFormat = "«{0}» ez da {1} motako eremu baterako balio egokia.";
PropertyGridStrings.RequiredValueFormat = "{0} motako balio bat behar da.";
PropertyGridStrings.CannotConvertFormat = "Ezin da {0} batetik {1} batera pasatu.";
PropertyGridStrings.CollectionSummaryFormat = "{0} elementu";
PropertyGridStrings.WholeNumberName = "zenbaki oso";
PropertyGridStrings.NumberName = "zenbaki";
PropertyGridStrings.BooleanName = "bai edo ez";
PropertyGridStrings.CharacterName = "karaktere";
PropertyGridStrings.TextName = "testu";
PropertyGridStrings.DateTimeName = "data eta ordu";
PropertyGridStrings.DateName = "data";
PropertyGridStrings.TimeName = "ordu";
PropertyGridStrings.DurationName = "iraupen";
```


## Français (`fr`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Rechercher des propriétés</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Annuler</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Parcourir…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Modifier…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "Général";
PropertyGridStrings.NotAValidFormat = "« {0} » n’est pas une valeur valide pour un champ de type {1}.";
PropertyGridStrings.RequiredValueFormat = "Une valeur de type {0} est requise.";
PropertyGridStrings.CannotConvertFormat = "Impossible de convertir {0} en {1}.";
PropertyGridStrings.CollectionSummaryFormat = "{0} éléments";
PropertyGridStrings.WholeNumberName = "nombre entier";
PropertyGridStrings.NumberName = "nombre";
PropertyGridStrings.BooleanName = "oui ou non";
PropertyGridStrings.CharacterName = "caractère";
PropertyGridStrings.TextName = "texte";
PropertyGridStrings.DateTimeName = "date et heure";
PropertyGridStrings.DateName = "date";
PropertyGridStrings.TimeName = "heure";
PropertyGridStrings.DurationName = "durée";
```


## Deutsch (`de`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Eigenschaften durchsuchen</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Abbrechen</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Durchsuchen…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Bearbeiten…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "Allgemein";
PropertyGridStrings.NotAValidFormat = "„{0}“ ist kein gültiger Wert für ein Feld vom Typ {1}.";
PropertyGridStrings.RequiredValueFormat = "Ein Wert vom Typ {0} ist erforderlich.";
PropertyGridStrings.CannotConvertFormat = "{0} lässt sich nicht in {1} umwandeln.";
PropertyGridStrings.CollectionSummaryFormat = "{0} Elemente";
PropertyGridStrings.WholeNumberName = "ganze Zahl";
PropertyGridStrings.NumberName = "Zahl";
PropertyGridStrings.BooleanName = "ja oder nein";
PropertyGridStrings.CharacterName = "Zeichen";
PropertyGridStrings.TextName = "Text";
PropertyGridStrings.DateTimeName = "Datum und Uhrzeit";
PropertyGridStrings.DateName = "Datum";
PropertyGridStrings.TimeName = "Uhrzeit";
PropertyGridStrings.DurationName = "Dauer";
```


## Italiano (`it`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Cerca proprietà</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Annulla</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Sfoglia…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Modifica…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "Generale";
PropertyGridStrings.NotAValidFormat = "«{0}» non è un valore valido per un campo di tipo {1}.";
PropertyGridStrings.RequiredValueFormat = "È necessario un valore di tipo {0}.";
PropertyGridStrings.CannotConvertFormat = "Non si può passare da {0} a {1}.";
PropertyGridStrings.CollectionSummaryFormat = "{0} elementi";
PropertyGridStrings.WholeNumberName = "numero intero";
PropertyGridStrings.NumberName = "numero";
PropertyGridStrings.BooleanName = "sì o no";
PropertyGridStrings.CharacterName = "carattere";
PropertyGridStrings.TextName = "testo";
PropertyGridStrings.DateTimeName = "data e ora";
PropertyGridStrings.DateName = "data";
PropertyGridStrings.TimeName = "ora";
PropertyGridStrings.DurationName = "durata";
```


## Português (`pt`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Procurar propriedades</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancelar</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Procurar…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editar…</x:String>
</ResourceDictionary>
```

```csharp
PropertyGridStrings.DefaultCategoryName = "Geral";
PropertyGridStrings.NotAValidFormat = "«{0}» não é um valor válido para um campo do tipo {1}.";
PropertyGridStrings.RequiredValueFormat = "É necessário um valor do tipo {0}.";
PropertyGridStrings.CannotConvertFormat = "Não é possível passar de {0} para {1}.";
PropertyGridStrings.CollectionSummaryFormat = "{0} elementos";
PropertyGridStrings.WholeNumberName = "número inteiro";
PropertyGridStrings.NumberName = "número";
PropertyGridStrings.BooleanName = "sim ou não";
PropertyGridStrings.CharacterName = "carácter";
PropertyGridStrings.TextName = "texto";
PropertyGridStrings.DateTimeName = "data e hora";
PropertyGridStrings.DateName = "data";
PropertyGridStrings.TimeName = "hora";
PropertyGridStrings.DurationName = "duração";
```
