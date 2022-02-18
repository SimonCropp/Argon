﻿#region License
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
using System.Runtime.Serialization.Json;
using Assert = Argon.Tests.XUnitAssert;
using XAssert = Xunit.Assert;

namespace Argon.Tests;

public class TestReflectionUtils
{
    public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
    {
#if !NET5_0_OR_GREATER
            return type.GetConstructors();
#else
        return type.GetTypeInfo().DeclaredConstructors;
#endif
    }

    public static PropertyInfo GetProperty(Type type, string name)
    {
#if !NET5_0_OR_GREATER
            return type.GetProperty(name);
#else
        return type.GetTypeInfo().GetDeclaredProperty(name);
#endif
    }

    public static FieldInfo GetField(Type type, string name)
    {
#if !NET5_0_OR_GREATER
            return type.GetField(name);
#else
        return type.GetTypeInfo().GetDeclaredField(name);
#endif
    }

    public static MethodInfo GetMethod(Type type, string name)
    {
#if !NET5_0_OR_GREATER
            return type.GetMethod(name);
#else
        return type.GetTypeInfo().GetDeclaredMethod(name);
#endif
    }
}

public class TestFixtureAttribute : Attribute
{
    // xunit doesn't need a test fixture attribute
    // this exists so the project compiles
}

[TestFixture]
public abstract class TestFixtureBase
{
    protected string GetDataContractJsonSerializeResult(object o)
    {
        var ms = new MemoryStream();
        var s = new DataContractJsonSerializer(o.GetType());
        s.WriteObject(ms, o);

        var data = ms.ToArray();
        return Encoding.UTF8.GetString(data, 0, data.Length);
    }

    protected string GetOffset(DateTime d, DateFormatHandling dateFormatHandling)
    {
        var chars = new char[8];
        var pos = DateTimeUtils.WriteDateTimeOffset(chars, 0, DateTime.SpecifyKind(d, DateTimeKind.Local).GetUtcOffset(), dateFormatHandling);

        return new string(chars, 0, pos);
    }

    protected string BytesToHex(byte[] bytes)
    {
        return BytesToHex(bytes, false);
    }

    protected string BytesToHex(byte[] bytes, bool removeDashes)
    {
        var hex = BitConverter.ToString(bytes);
        if (removeDashes)
        {
            hex = hex.Replace("-", "");
        }

        return hex;
    }

    public static byte[] HexToBytes(string hex)
    {
        var fixedHex = hex.Replace("-", string.Empty);

        // array to put the result in
        var bytes = new byte[fixedHex.Length / 2];
        // variable to determine shift of high/low nibble
        var shift = 4;
        // offset of the current byte in the array
        var offset = 0;
        // loop the characters in the string
        foreach (var c in fixedHex)
        {
            // get character code in range 0-9, 17-22
            // the % 32 handles lower case characters
            var b = (c - '0') % 32;
            // correction for a-f
            if (b > 9)
            {
                b -= 7;
            }
            // store nibble (4 bits) in byte array
            bytes[offset] |= (byte)(b << shift);
            // toggle the shift variable between 0 and 4
            shift ^= 4;
            // move to next byte
            if (shift != 0)
            {
                offset++;
            }
        }
        return bytes;
    }

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

    protected void WriteEscapedJson(string json)
    {
        Console.WriteLine(EscapeJson(json));
    }

    protected string EscapeJson(string json)
    {
        return @"@""" + json.Replace(@"""", @"""""") + @"""";
    }

    protected string GetNestedJson(int depth)
    {
        var root = new JObject();
        var current = root;
        for (var i = 0; i < depth - 1; i++)
        {
            var nested = new JObject();
            current[i.ToString()] = nested;

            current = nested;
        }

        return root.ToString();
    }
}

public static class CustomAssert
{
    public static void IsInstanceOfType(Type t, object instance)
    {
        XAssert.IsType(t, instance);
    }

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

        Assert.AreEqual(expected, actual);
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

            Assert.Fail("Exception of type " + typeof(TException).Name + " expected. No exception thrown.");
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

            Assert.Fail("Exception of type " + typeof(TException).Name + " expected. No exception thrown.");
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