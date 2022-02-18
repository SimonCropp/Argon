#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Text.RegularExpressions;

namespace Argon.Tests;

public abstract class TestFixtureBase
{
    protected TestFixtureBase()
    {
#if !NET5_0_OR_GREATER
        //CultureInfo turkey = CultureInfo.CreateSpecificCulture("tr");
        //Thread.CurrentThread.CurrentCulture = turkey;
        //Thread.CurrentThread.CurrentUICulture = turkey;

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
#else
        // suppress writing to console with dotnet test to keep build log size small
        Console.SetOut(new StringWriter());
#endif

        JsonConvert.DefaultSettings = null;
    }
}

public static class CustomAssert
{
    public static void Contains(IList collection, object value)
    {
        Contains(collection, value, null);
    }

    public static void Contains(IList collection, object value, string message)
    {
        if (!collection.Cast<object>().Any(i => i.Equals(value)))
        {
            throw new Exception(message ?? "Value not found in collection.");
        }
    }
}

public static class StringAssert
{
    static readonly Regex Regex = new(@"\r\n|\n\r|\n|\r", RegexOptions.CultureInvariant);

    public static void AreEqual(string expected, string actual)
    {
        expected = Normalize(expected);
        actual = Normalize(actual);

        Xunit.Assert.Equal(expected, actual);
    }

    public static bool Equals(string s1, string s2)
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
}

public static class ExceptionAssert
{
    public static TException Throws<TException>(Action action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            action();

            XUnitAssert.Fail("Exception of type " + typeof(TException).Name + " expected. No exception thrown.");
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
                if (StringAssert.Equals(possibleMessage, ex.Message))
                {
                    return ex;
                }
            }

            throw new Exception("Unexpected exception message." + Environment.NewLine + "Expected one of: " + string.Join(Environment.NewLine, possibleMessages) + Environment.NewLine + "Got: " + ex.Message + Environment.NewLine + Environment.NewLine + ex);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Exception of type {0} expected; got exception of type {1}.", typeof(TException).Name, ex.GetType().Name), ex);
        }
    }

    public static async Task<TException> ThrowsAsync<TException>(Func<Task> action, params string[] possibleMessages)
        where TException : Exception
    {
        try
        {
            await action();

            XUnitAssert.Fail("Exception of type " + typeof(TException).Name + " expected. No exception thrown.");
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
                if (StringAssert.Equals(possibleMessage, ex.Message))
                {
                    return ex;
                }
            }

            throw new Exception("Unexpected exception message." + Environment.NewLine + "Expected one of: " + string.Join(Environment.NewLine, possibleMessages) + Environment.NewLine + "Got: " + ex.Message + Environment.NewLine + Environment.NewLine + ex);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Exception of type {0} expected; got exception of type {1}.", typeof(TException).Name, ex.GetType().Name), ex);
        }
    }
}