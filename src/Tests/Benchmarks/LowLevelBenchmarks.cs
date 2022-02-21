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

using BenchmarkDotNet.Attributes;
using MemberTypes = System.Reflection.MemberTypes;
using BindingFlags = System.Reflection.BindingFlags;

public class LowLevelBenchmarks
{
    const string FloatText = "123.123";
    static readonly char[] FloatChars = FloatText.ToCharArray();

    static readonly Dictionary<string, object> NormalDictionary = new();

    static readonly ConcurrentDictionary<string, object> ConcurrentDictionary = new();

    static LowLevelBenchmarks()
    {
        for (var i = 0; i < 10; i++)
        {
            var key = i.ToString();
            var value = new object();

            NormalDictionary.Add(key, value);
            ConcurrentDictionary.TryAdd(key, value);
        }
    }

    [Benchmark]
    public void DictionaryGet()
    {
        NormalDictionary.TryGetValue("1", out var _);
    }

    [Benchmark]
    public void ConcurrentDictionaryGet()
    {
        ConcurrentDictionary.TryGetValue("1", out var _);
    }

    [Benchmark]
    public void ConcurrentDictionaryGetOrCreate()
    {
        ConcurrentDictionary.GetOrAdd("1", Dummy);
    }

    static object Dummy(string arg)
    {
        throw new("Should never get here.");
    }

    [Benchmark]
    public void DecimalTryParseString()
    {
        decimal value;
        decimal.TryParse(FloatText, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value);
    }

    [Benchmark]
    public void GetMemberWithMemberTypeAndBindingFlags()
    {
        typeof(LowLevelBenchmarks).GetMember("AName", MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [Benchmark]
    public void GetPropertyGetField()
    {
        typeof(LowLevelBenchmarks).GetProperty("AName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        typeof(LowLevelBenchmarks).GetField("AName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [Benchmark]
    public void DecimalTryParseChars()
    {
        decimal value;
        ConvertUtils.DecimalTryParse(FloatChars, 0, FloatChars.Length, out value);
    }

    [Benchmark]
    public void WriteEscapedJavaScriptString()
    {
        var text = @"The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.

The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.

The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.
";

        using var w = StringUtils.CreateStringWriter(text.Length);
        char[] buffer = null;
        JavaScriptUtils.WriteEscapedJavaScriptString(w, text, '"', true, JavaScriptUtils.DoubleQuoteCharEscapeFlags, StringEscapeHandling.Default, null, ref buffer);
    }
}