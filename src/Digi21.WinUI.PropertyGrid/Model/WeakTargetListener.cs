using System.ComponentModel;

namespace Digi21.WinUI.PropertyGrid;

// What the grid wants to hear from the object it is showing.
internal interface ITargetObserver
{
    void OnTargetPropertyChanged(object target, string? propertyName);

    void OnTargetErrorsChanged(object target, string? propertyName);
}

// Subscribes to an object's change notifications without keeping the grid alive.
//
// The object being shown routinely outlives the grid showing it: a view model owned by the
// application, inspected in a dialog that is opened and closed all afternoon. A plain
// `target.PropertyChanged += grid.Handler` would make every one of those dialogs immortal, together
// with its rows and its whole visual tree, and the leak would only show up under memory pressure.
//
// So the listener holds the observer weakly and the target strongly. The target keeps the listener
// alive, the listener keeps nothing alive, and the first notification after the grid is collected
// unsubscribes on the spot.
internal sealed class WeakTargetListener
{
    private readonly object target;
    private readonly WeakReference<ITargetObserver> observer;

    internal WeakTargetListener(object target, ITargetObserver observer)
    {
        this.target = target;
        this.observer = new WeakReference<ITargetObserver>(observer);

        if (target is INotifyPropertyChanged notifiesChanges)
        {
            notifiesChanges.PropertyChanged += OnPropertyChanged;
        }

        if (target is INotifyDataErrorInfo notifiesErrors)
        {
            notifiesErrors.ErrorsChanged += OnErrorsChanged;
        }
    }

    internal void Detach()
    {
        if (target is INotifyPropertyChanged notifiesChanges)
        {
            notifiesChanges.PropertyChanged -= OnPropertyChanged;
        }

        if (target is INotifyDataErrorInfo notifiesErrors)
        {
            notifiesErrors.ErrorsChanged -= OnErrorsChanged;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs arguments)
    {
        if (observer.TryGetTarget(out ITargetObserver? alive))
        {
            alive.OnTargetPropertyChanged(target, arguments.PropertyName);
        }
        else
        {
            Detach();
        }
    }

    private void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs arguments)
    {
        if (observer.TryGetTarget(out ITargetObserver? alive))
        {
            alive.OnTargetErrorsChanged(target, arguments.PropertyName);
        }
        else
        {
            Detach();
        }
    }
}
