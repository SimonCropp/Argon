public class XUnitAssert
{
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
        s.ReplaceLineEndings("\n");

    public static TException Throws<TException>(Action action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            action();

            Assert.Fail($"Exception of type {typeof(TException).Name} expected. No exception thrown.");
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
}