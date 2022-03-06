// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using BenchmarkDotNet.Attributes;

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
        NormalDictionary.TryGetValue("1", out _);
    }

    [Benchmark]
    public void ConcurrentDictionaryGet()
    {
        ConcurrentDictionary.TryGetValue("1", out _);
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
        JavaScriptUtils.WriteEscapedJavaScriptString(w, text, '"', true, JavaScriptUtils.DoubleQuoteEscapeFlags, EscapeHandling.Default, null, ref buffer);
    }
}