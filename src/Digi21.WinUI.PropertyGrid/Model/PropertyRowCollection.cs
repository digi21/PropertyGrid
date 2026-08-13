using System.Collections;
using System.Collections.Specialized;

namespace Digi21.WinUI.PropertyGrid;

// The rows a grid is showing, as one flat list.
//
// An ObservableCollection has no range operations, so collapsing a category of thirty properties
// would raise thirty separate notifications and make the repeater re-resolve indices thirty times.
// Structural changes here replace the whole run at once and raise a single reset.
internal sealed class PropertyRowCollection : IList<PropertyGridRow>, IReadOnlyList<PropertyGridRow>, IList, INotifyCollectionChanged
{
    private readonly List<PropertyGridRow> rows = [];
    private readonly Dictionary<string, int> indexByKey = new(StringComparer.Ordinal);

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Count => rows.Count;

    public bool IsReadOnly => true;

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => true;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => rows;

    public PropertyGridRow this[int index]
    {
        get => rows[index];
        set => throw new NotSupportedException("The grid owns its rows; change what it is showing instead.");
    }

    object? IList.this[int index]
    {
        get => rows[index];
        set => throw new NotSupportedException("The grid owns its rows; change what it is showing instead.");
    }

    // Every key the collection is currently showing, so a caller can put back what was expanded or
    // selected after the list is rebuilt.
    internal string KeyAt(int index) => rows[index].Key;

    internal int IndexOfKey(string key) => indexByKey.TryGetValue(key, out int index) ? index : -1;

    internal void Replace(List<PropertyGridRow> replacement)
    {
        rows.Clear();
        rows.AddRange(replacement);

        indexByKey.Clear();
        for (int index = 0; index < rows.Count; index++)
        {
            // Duplicate keys would silently break every lookup that relies on them. They can only
            // come from a provider handing out two properties of the same name, so the last one
            // wins rather than throwing in the middle of a rebuild.
            indexByKey[rows[index].Key] = index;
        }

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public int IndexOf(PropertyGridRow item) => rows.IndexOf(item);

    public bool Contains(PropertyGridRow item) => rows.Contains(item);

    public void CopyTo(PropertyGridRow[] array, int arrayIndex) => rows.CopyTo(array, arrayIndex);

    public IEnumerator<PropertyGridRow> GetEnumerator() => rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => rows.GetEnumerator();

    void ICollection.CopyTo(Array array, int index) => ((ICollection)rows).CopyTo(array, index);

    int IList.Add(object? value) => throw new NotSupportedException();

    bool IList.Contains(object? value) => value is PropertyGridRow row && rows.Contains(row);

    int IList.IndexOf(object? value) => value is PropertyGridRow row ? rows.IndexOf(row) : -1;

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();

    void IList.Clear() => throw new NotSupportedException();

    void IList.RemoveAt(int index) => throw new NotSupportedException();

    public void Add(PropertyGridRow item) => throw new NotSupportedException();

    public void Clear() => throw new NotSupportedException();

    public void Insert(int index, PropertyGridRow item) => throw new NotSupportedException();

    public bool Remove(PropertyGridRow item) => throw new NotSupportedException();

    public void RemoveAt(int index) => throw new NotSupportedException();
}
