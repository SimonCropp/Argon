using Xunit;

namespace Argon.Tests;

public class XUnitAssert
{
    public static void AreEqual(double expected, double actual, double r)
    {
        Assert.Equal(expected, actual, 5); // hack
    }

    public static void False(object actual)
    {
        Assert.IsType<bool>(actual);
        Assert.NotNull(actual);
        Assert.False((bool) actual);
    }
    
    public static void True(object actual)
    {
        Assert.IsType<bool>(actual);
        Assert.NotNull(actual);
        Assert.True((bool) actual);
    }

    public static void Fail(string message = null, params object[] args)
    {
        if (message != null)
        {
            message = message.FormatWith(CultureInfo.InvariantCulture, args);
        }

        Assert.True(false, message);
    }
}