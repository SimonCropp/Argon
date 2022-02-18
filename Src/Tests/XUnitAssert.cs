using System.Text.RegularExpressions;
using Xunit;

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
            message = string.Format(message, args);
        }

        Assert.True(false, message);
    }
    static readonly Regex Regex = new(@"\r\n|\n\r|\n|\r", RegexOptions.CultureInvariant);

    public static void AreEqualNormalized(string expected, string actual)
    {
        expected = Normalize(expected);
        actual = Normalize(actual);

        Assert.Equal(expected, actual);
    }

    public static bool EqualsNormalized(string s1, string s2)
    {
        s1 = Normalize(s1);
        s2 = Normalize(s2);

        return string.Equals(s1, s2);
    }

    public static string Normalize(string s)
    {
        if (s != null)
        {
            s = Regex.Replace(s, "\r\n");
        }

        return s;
    }


    public static TException Throws<TException>(Action action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            action();

            Fail("Exception of type " + typeof(TException).Name + " expected. No exception thrown.");
            return null;
        }
        catch (TException ex)
        {
            if (possibleMessages == null || possibleMessages.Length == 0)
            {
                return ex;
            }
            foreach (var possibleMessage in possibleMessages)
            {
                if (EqualsNormalized(possibleMessage, ex.Message))
                {
                    return ex;
                }
            }

            throw new Exception($"Unexpected exception message.{Environment.NewLine}Expected one of: {string.Join(Environment.NewLine, possibleMessages)}{Environment.NewLine}Got: {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception of type {typeof(TException).Name} expected; got exception of type {ex.GetType().Name}.", ex);
        }
    }

    public static async Task<TException> ThrowsAsync<TException>(Func<Task> action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            await action();

            Fail("Exception of type " + typeof(TException).Name + " expected. No exception thrown.");
            return null;
        }
        catch (TException ex)
        {
            if (possibleMessages == null || possibleMessages.Length == 0)
            {
                return ex;
            }
            foreach (var possibleMessage in possibleMessages)
            {
                if (EqualsNormalized(possibleMessage, ex.Message))
                {
                    return ex;
                }
            }

            throw new Exception("Unexpected exception message." + Environment.NewLine + "Expected one of: " + string.Join(Environment.NewLine, possibleMessages) + Environment.NewLine + "Got: " + ex.Message + Environment.NewLine + Environment.NewLine + ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception of type {typeof(TException).Name} expected; got exception of type {ex.GetType().Name}.", ex);
        }
    }
}