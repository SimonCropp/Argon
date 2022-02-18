using Xunit;

namespace Argon.Tests;

public class XUnitAssert
{
    public static void AreEqual(double expected, double actual, double r)
    {
        Assert.Equal(expected, actual, 5); // hack
    }

    public static void AreEqual(object expected, object actual)
    {
        Assert.Equal(expected, actual);
    }

    public static void AreEqual<T>(T expected, T actual)
    {
        Assert.Equal(expected, actual);
    }

    public static void AreNotEqual(object expected, object actual)
    {
        Assert.NotEqual(expected, actual);
    }

    public static void AreNotEqual<T>(T expected, T actual)
    {
        Assert.NotEqual(expected, actual);
    }

    public static void Fail(string message = null, params object[] args)
    {
        if (message != null)
        {
            message = message.FormatWith(CultureInfo.InvariantCulture, args);
        }

        Assert.True(false, message);
    }

    public static void IsTrue(bool condition, string message = null)
    {
        Assert.True(condition);
    }

    public static void IsFalse(bool condition)
    {
        Assert.False(condition);
    }

    public static void IsNull(object o)
    {
        Assert.Null(o);
    }

    public static void IsNotNull(object o)
    {
        Assert.NotNull(o);
    }

    public static void AreNotSame(object expected, object actual)
    {
        Assert.NotSame(expected, actual);
    }

    public static void AreSame(object expected, object actual)
    {
        Assert.Same(expected, actual);
    }
}