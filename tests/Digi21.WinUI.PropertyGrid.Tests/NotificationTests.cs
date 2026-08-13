using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class NotificationTests
{
    [Fact]
    public void RefreshesARowWhenTheObjectChangesUnderneathIt()
    {
        ObservableSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Name")!;

        subject.Name = "changed elsewhere";

        Assert.Equal("changed elsewhere", row.Value);
        Assert.Equal("changed elsewhere", row.Text);
    }

    [Fact]
    public void RefreshesEveryRowWhenTheObjectSaysEverythingChanged()
    {
        ObservableSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);

        // An empty or null property name is the documented way to say "all of me", and a grid that
        // ignores it quietly stops matching the object it is showing.
        typeof(ObservableSubject).GetProperty("Name")!.SetValue(subject, "quietly");
        typeof(ObservableSubject).GetProperty("Count")!.SetValue(subject, 9);
        subject.RaiseEverythingChanged();

        Assert.Equal("quietly", source.FindRow("Name")!.Value);
        Assert.Equal(9, source.FindRow("Count")!.Value);
    }

    [Fact]
    public void AnnouncesAValueChangeToTheBindings()
    {
        ObservableSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Name")!;

        List<string?> announced = [];
        row.PropertyChanged += (_, arguments) => announced.Add(arguments.PropertyName);

        row.Value = "typed";

        Assert.Contains(nameof(PropertyGridPropertyRow.Value), announced);
        Assert.Contains(nameof(PropertyGridPropertyRow.Text), announced);
    }

    [Fact]
    public void DoesNotEchoBackAndForthWhenTheObjectAnnouncesTheWriteItJustReceived()
    {
        ObservableSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Count")!;

        int writes = 0;
        source.ValueChanged = (_, _, _) => writes++;

        row.Value = 3;

        Assert.Equal(1, writes);
        Assert.Equal(3, subject.Count);
    }

    [Fact]
    public void RaisesTheChangeCallbackWithBothValues()
    {
        ObservableSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);

        (object? Old, object? New) seen = (null, null);
        source.ValueChanged = (_, oldValue, newValue) => seen = (oldValue, newValue);

        source.FindRow("Name")!.Value = "after";

        Assert.Equal(("initial", "after"), seen);
    }

    [Fact]
    public void LetsTheChangingCallbackVetoAWrite()
    {
        ObservableSubject subject = new();
        PropertyGridSource source = new()
        {
            ValueChanging = (_, _, newValue) => Equals(newValue, "forbidden") ? "That name is taken." : null,
        };
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Name")!;

        row.Value = "forbidden";

        Assert.Equal("initial", subject.Name);
        Assert.Equal("That name is taken.", row.ErrorMessage);
    }

    [Fact]
    public void StopsListeningWhenTheGridIsCollectedEvenThoughTheObjectOutlivesIt()
    {
        // The object being shown routinely outlives the grid showing it. A strong handler would keep
        // the grid, its rows and its whole visual tree alive for as long as the view model lives.
        ObservableSubject subject = new();
        WeakReference collected = BuildAndAbandonGrid(subject);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(collected.IsAlive);

        // And the object is still usable: the dead listener detaches itself on the next notification
        // rather than throwing.
        subject.Name = "after the grid is gone";
        Assert.Equal("after the grid is gone", subject.Name);
    }

    [Fact]
    public void StopsListeningToTheOldObjectWhenItIsGivenANewOne()
    {
        ObservableSubject first = new();
        ObservableSubject second = new();
        PropertyGridSource source = new();
        source.SetTarget(first);

        source.SetTarget(second);
        first.Name = "no longer shown";

        Assert.Equal("initial", source.FindRow("Name")!.Value);
    }

    [Fact]
    public void ListensToANestedObjectOnceItIsOpened()
    {
        NestedSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        source.FindRow("Address")!.IsExpanded = true;

        subject.Address.City = "Sevilla";

        Assert.Equal("Sevilla", source.FindRow("Address.City")!.Value);
    }

    [Fact]
    public void IgnoresANotificationForAPropertyItIsNotShowing()
    {
        ObservableSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);

        int announced = 0;
        source.FindRow("Name")!.PropertyChanged += (_, _) => announced++;

        subject.Count = 5;

        Assert.Equal(0, announced);
    }

    // Kept out of the test body so the source has no local rooting it when the collection runs.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference BuildAndAbandonGrid(INotifyPropertyChanged subject)
    {
        PropertyGridSource source = new();
        source.SetTarget(subject);
        Assert.NotEmpty(source.Rows);
        return new WeakReference(source);
    }
}
