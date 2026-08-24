# Translations for Digi21.WinUI.PropertyGrid

Nine languages, contributed by the application that asked for them: lop, a point-cloud editor shipped
on the Microsoft Store in these same nine.

Every word is a resource key, so a language is one `ResourceDictionary` and nothing else — no code
to run, nothing to remember to call. Merge the one you want into `App.xaml` and the grid speaks it:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            <ResourceDictionary Source="ms-appx:///Strings/es.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Or add it in code, which is what an application that switches language at run time does. Nothing is
cached, so a dictionary merged after the first grid already exists still reaches every sentence
built from then on; what the rows already show follows the next time they are built.

**If you do it in code, do it in `OnLaunched` and not in the constructor of `App`.** Touching
`Application.Current.Resources` there throws a `COMException 0x8000FFFF` — the application dictionary
does not exist yet, `InitializeComponent` only registers where it comes from — and the process dies
before showing a window, with no message.

Twenty keys per language, and every one of them reaches the screen. The first six are painted by a
template; the rest the grid builds itself.

About the placeholders: `PropertyGridNotAValidFormat` takes the offending text and then the type
name, `PropertyGridRequiredValueFormat` takes the type name, `PropertyGridCannotConvertFormat` takes
both type names, and `PropertyGridCollectionSummaryFormat` takes a count. The order is the same in
all nine here.

The type names are deliberately what a person would say — "whole number", not `Int32` — because they
are read inside a sentence explaining why an edit was rejected.

The contributed file had sixteen strings rather than the fourteen the grid builds: `MultipleValues`
and `NullValue` were declared before anything used them, and rather than have nine translators spend
an afternoon on text that never appears they were dropped. `MultipleValues` comes back with multiple
selection; `NullValue` when a summary cell has somewhere to put it that is not the text a user edits.

**A key is not a name the compiler checks.** A dictionary filed under a key the library no longer
reads is not an error — the entry sits there, nobody reads it, and the string quietly reverts to
English. `PropertyGridText.ResourceKeys` is the list of keys the grid reads, so that an application
can compare its own dictionary against it at startup and fail loudly instead; see
[theming.md](theming.md#checking-a-translation-at-startup). `LocalisationTests` holds that list to
the dictionary the library ships, so this file cannot drift away from either.

The grid does not translate what you put in it. A property's name, its category and its description
reach the grid already in whatever language you chose.

## English (`en`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Search properties</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Pick a date</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Browse…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Edit…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancel</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">General</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">“{0}” is not a valid value for a {1} field.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">A {0} value is required.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">A {0} cannot be turned into a {1}.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} items</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">whole number</x:String>
    <x:String x:Key="PropertyGridNumberName">number</x:String>
    <x:String x:Key="PropertyGridBooleanName">true or false</x:String>
    <x:String x:Key="PropertyGridCharacterName">character</x:String>
    <x:String x:Key="PropertyGridTextName">text</x:String>
    <x:String x:Key="PropertyGridDateTimeName">date and time</x:String>
    <x:String x:Key="PropertyGridDateName">date</x:String>
    <x:String x:Key="PropertyGridTimeName">time</x:String>
    <x:String x:Key="PropertyGridDurationName">duration</x:String>
</ResourceDictionary>
```


## Español (`es`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Buscar propiedades</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Elija una fecha</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Examinar…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editar…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">Aceptar</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancelar</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">General</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">«{0}» no es un valor válido para un campo de tipo {1}.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">Hace falta un valor de tipo {0}.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">No se puede pasar de {0} a {1}.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} elementos</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">número entero</x:String>
    <x:String x:Key="PropertyGridNumberName">número</x:String>
    <x:String x:Key="PropertyGridBooleanName">sí o no</x:String>
    <x:String x:Key="PropertyGridCharacterName">carácter</x:String>
    <x:String x:Key="PropertyGridTextName">texto</x:String>
    <x:String x:Key="PropertyGridDateTimeName">fecha y hora</x:String>
    <x:String x:Key="PropertyGridDateName">fecha</x:String>
    <x:String x:Key="PropertyGridTimeName">hora</x:String>
    <x:String x:Key="PropertyGridDurationName">duración</x:String>
</ResourceDictionary>
```


## Català (`ca`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Cerca propietats</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Trieu una data</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Examina…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Edita…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">D’acord</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancel·la</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">General</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">«{0}» no és un valor vàlid per a un camp de tipus {1}.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">Cal un valor de tipus {0}.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">No es pot passar de {0} a {1}.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} elements</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">nombre enter</x:String>
    <x:String x:Key="PropertyGridNumberName">nombre</x:String>
    <x:String x:Key="PropertyGridBooleanName">sí o no</x:String>
    <x:String x:Key="PropertyGridCharacterName">caràcter</x:String>
    <x:String x:Key="PropertyGridTextName">text</x:String>
    <x:String x:Key="PropertyGridDateTimeName">data i hora</x:String>
    <x:String x:Key="PropertyGridDateName">data</x:String>
    <x:String x:Key="PropertyGridTimeName">hora</x:String>
    <x:String x:Key="PropertyGridDurationName">durada</x:String>
</ResourceDictionary>
```


## Galego (`gl`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Buscar propiedades</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Escolla unha data</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Examinar…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editar…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">Aceptar</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancelar</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">Xeral</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">«{0}» non é un valor válido para un campo de tipo {1}.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">Fai falta un valor de tipo {0}.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">Non se pode pasar de {0} a {1}.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} elementos</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">número enteiro</x:String>
    <x:String x:Key="PropertyGridNumberName">número</x:String>
    <x:String x:Key="PropertyGridBooleanName">si ou non</x:String>
    <x:String x:Key="PropertyGridCharacterName">carácter</x:String>
    <x:String x:Key="PropertyGridTextName">texto</x:String>
    <x:String x:Key="PropertyGridDateTimeName">data e hora</x:String>
    <x:String x:Key="PropertyGridDateName">data</x:String>
    <x:String x:Key="PropertyGridTimeName">hora</x:String>
    <x:String x:Key="PropertyGridDurationName">duración</x:String>
</ResourceDictionary>
```


## Euskara (`eu`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Bilatu propietateak</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Aukeratu data bat</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Arakatu…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editatu…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">Ados</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Utzi</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">Orokorra</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">«{0}» ez da {1} motako eremu baterako balio egokia.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">{0} motako balio bat behar da.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">Ezin da {0} batetik {1} batera pasatu.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} elementu</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">zenbaki oso</x:String>
    <x:String x:Key="PropertyGridNumberName">zenbaki</x:String>
    <x:String x:Key="PropertyGridBooleanName">bai edo ez</x:String>
    <x:String x:Key="PropertyGridCharacterName">karaktere</x:String>
    <x:String x:Key="PropertyGridTextName">testu</x:String>
    <x:String x:Key="PropertyGridDateTimeName">data eta ordu</x:String>
    <x:String x:Key="PropertyGridDateName">data</x:String>
    <x:String x:Key="PropertyGridTimeName">ordu</x:String>
    <x:String x:Key="PropertyGridDurationName">iraupen</x:String>
</ResourceDictionary>
```


## Français (`fr`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Rechercher des propriétés</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Choisissez une date</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Parcourir…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Modifier…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Annuler</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">Général</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">« {0} » n’est pas une valeur valide pour un champ de type {1}.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">Une valeur de type {0} est requise.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">Impossible de convertir {0} en {1}.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} éléments</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">nombre entier</x:String>
    <x:String x:Key="PropertyGridNumberName">nombre</x:String>
    <x:String x:Key="PropertyGridBooleanName">oui ou non</x:String>
    <x:String x:Key="PropertyGridCharacterName">caractère</x:String>
    <x:String x:Key="PropertyGridTextName">texte</x:String>
    <x:String x:Key="PropertyGridDateTimeName">date et heure</x:String>
    <x:String x:Key="PropertyGridDateName">date</x:String>
    <x:String x:Key="PropertyGridTimeName">heure</x:String>
    <x:String x:Key="PropertyGridDurationName">durée</x:String>
</ResourceDictionary>
```


## Deutsch (`de`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Eigenschaften durchsuchen</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Datum wählen</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Durchsuchen…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Bearbeiten…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Abbrechen</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">Allgemein</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">„{0}“ ist kein gültiger Wert für ein Feld vom Typ {1}.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">Ein Wert vom Typ {0} ist erforderlich.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">{0} lässt sich nicht in {1} umwandeln.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} Elemente</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">ganze Zahl</x:String>
    <x:String x:Key="PropertyGridNumberName">Zahl</x:String>
    <x:String x:Key="PropertyGridBooleanName">ja oder nein</x:String>
    <x:String x:Key="PropertyGridCharacterName">Zeichen</x:String>
    <x:String x:Key="PropertyGridTextName">Text</x:String>
    <x:String x:Key="PropertyGridDateTimeName">Datum und Uhrzeit</x:String>
    <x:String x:Key="PropertyGridDateName">Datum</x:String>
    <x:String x:Key="PropertyGridTimeName">Uhrzeit</x:String>
    <x:String x:Key="PropertyGridDurationName">Dauer</x:String>
</ResourceDictionary>
```


## Italiano (`it`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Cerca proprietà</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Scegli una data</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Sfoglia…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Modifica…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Annulla</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">Generale</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">«{0}» non è un valore valido per un campo di tipo {1}.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">È necessario un valore di tipo {0}.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">Non si può passare da {0} a {1}.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} elementi</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">numero intero</x:String>
    <x:String x:Key="PropertyGridNumberName">numero</x:String>
    <x:String x:Key="PropertyGridBooleanName">sì o no</x:String>
    <x:String x:Key="PropertyGridCharacterName">carattere</x:String>
    <x:String x:Key="PropertyGridTextName">testo</x:String>
    <x:String x:Key="PropertyGridDateTimeName">data e ora</x:String>
    <x:String x:Key="PropertyGridDateName">data</x:String>
    <x:String x:Key="PropertyGridTimeName">ora</x:String>
    <x:String x:Key="PropertyGridDurationName">durata</x:String>
</ResourceDictionary>
```


## Português (`pt`)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <x:String x:Key="PropertyGridSearchPlaceholderText">Procurar propriedades</x:String>
    <x:String x:Key="PropertyGridSelectDatePlaceholderText">Escolha uma data</x:String>
    <x:String x:Key="PropertyGridBrowseToolTipText">Procurar…</x:String>
    <x:String x:Key="PropertyGridEditToolTipText">Editar…</x:String>
    <x:String x:Key="PropertyGridOkButtonText">OK</x:String>
    <x:String x:Key="PropertyGridCancelButtonText">Cancelar</x:String>

    <x:String x:Key="PropertyGridDefaultCategoryName">Geral</x:String>
    <x:String x:Key="PropertyGridNotAValidFormat">«{0}» não é um valor válido para um campo do tipo {1}.</x:String>
    <x:String x:Key="PropertyGridRequiredValueFormat">É necessário um valor do tipo {0}.</x:String>
    <x:String x:Key="PropertyGridCannotConvertFormat">Não é possível passar de {0} para {1}.</x:String>
    <x:String x:Key="PropertyGridCollectionSummaryFormat">{0} elementos</x:String>
    <x:String x:Key="PropertyGridWholeNumberName">número inteiro</x:String>
    <x:String x:Key="PropertyGridNumberName">número</x:String>
    <x:String x:Key="PropertyGridBooleanName">sim ou não</x:String>
    <x:String x:Key="PropertyGridCharacterName">carácter</x:String>
    <x:String x:Key="PropertyGridTextName">texto</x:String>
    <x:String x:Key="PropertyGridDateTimeName">data e hora</x:String>
    <x:String x:Key="PropertyGridDateName">data</x:String>
    <x:String x:Key="PropertyGridTimeName">hora</x:String>
    <x:String x:Key="PropertyGridDurationName">duração</x:String>
</ResourceDictionary>
```
