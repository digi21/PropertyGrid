using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class SmokeTests
{
    [Fact]
    public void LibraryAssemblyLoads()
    {
        Assert.Equal("Digi21.WinUI.PropertyGrid", typeof(PropertyGrid).Assembly.GetName().Name);
    }
}
