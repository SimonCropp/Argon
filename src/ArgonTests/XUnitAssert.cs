public class XUnitAssert
{
    public static void AreEqual(double expected, double actual, double r) =>
        Assert.Equal(expected, actual, 5); // hack

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

    public static string Normalize(string s) =>
        s
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

    public static TException Throws<TException>(Action action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            action();

            Fail($"Exception of type {typeof(TException).Name} expected. No exception thrown.");
            return null;
        }
        catch (TException exception)
        {
            if (possibleMessages == null || possibleMessages.Length == 0)
            {
                return exception;
            }

            foreach (var possibleMessage in possibleMessages)
            {
                if (EqualsNormalized(possibleMessage, exception.Message))
                {
                    return exception;
                }
            }

            throw new($"""
                       Unexpected exception message.
                       Expected one of:
                        * {string.Join(Environment.NewLine + " * ", possibleMessages)}
                       Got: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception}
                       """);
        }
        catch (Exception exception)
        {
            throw new($"Exception of type {typeof(TException).Name} expected; got exception of type {exception.GetType().Name}.", exception);
        }
    }

    public static async Task<TException> ThrowsAsync<TException>(Func<Task> action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            await action();

            Fail($"Exception of type {typeof(TException).Name} expected. No exception thrown.");
            return null;
        }
        catch (TException exception)
        {
            if (possibleMessages == null || possibleMessages.Length == 0)
            {
                return exception;
            }

            foreach (var possibleMessage in possibleMessages)
            {
                if (EqualsNormalized(possibleMessage, exception.Message))
                {
                    return exception;
                }
            }

            throw new($"Unexpected exception message.{Environment.NewLine}Expected one of: {string.Join(Environment.NewLine, possibleMessages)}{Environment.NewLine}Got: {exception.Message}{Environment.NewLine}{Environment.NewLine}{exception}");
        }
        catch (Exception exception)
        {
            throw new($"Exception of type {typeof(TException).Name} expected; got exception of type {exception.GetType().Name}.", exception);
        }
    }
}