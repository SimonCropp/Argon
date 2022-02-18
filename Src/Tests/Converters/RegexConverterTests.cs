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
using Xunit;

namespace Argon.Tests.Converters;

public class RegexConverterTests : TestFixtureBase
{
    public class RegexTestClass
    {
        public Regex Regex { get; set; }
    }

    [Fact]
    public void WriteJsonNull()
    {
        var sw = new StringWriter();
        var jsonWriter = new JsonTextWriter(sw);

        var converter = new RegexConverter();
        converter.WriteJson(jsonWriter, null, null);

        StringAssert.AreEqual(@"null", sw.ToString());
    }

    [Fact]
    public void SerializeToText()
    {
        var regex = new Regex("abc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var json = JsonConvert.SerializeObject(regex, Formatting.Indented, new RegexConverter());

        StringAssert.AreEqual(@"{
  ""Pattern"": ""abc"",
  ""Options"": 513
}", json);
    }

    [Fact]
    public void SerializeCamelCaseAndStringEnums()
    {
        var regex = new Regex("abc", RegexOptions.IgnoreCase);

        var json = JsonConvert.SerializeObject(regex, Formatting.Indented, new JsonSerializerSettings
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Converters = { new RegexConverter(), new StringEnumConverter { CamelCaseText = true } },
#pragma warning restore CS0618 // Type or member is obsolete
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        StringAssert.AreEqual(@"{
  ""pattern"": ""abc"",
  ""options"": ""ignoreCase""
}", json);
    }

    [Fact]
    public void DeserializeCamelCaseAndStringEnums()
    {
        var json = @"{
  ""pattern"": ""abc"",
  ""options"": ""ignoreCase""
}";

        var regex = JsonConvert.DeserializeObject<Regex>(json, new JsonSerializerSettings
        {
            Converters = { new RegexConverter() }
        });

        Assert.Equal("abc", regex.ToString());
        Assert.Equal(RegexOptions.IgnoreCase, regex.Options);
    }

    [Fact]
    public void DeserializeISerializeRegexJson()
    {
        var json = @"{
                        ""Regex"": {
                          ""pattern"": ""(hi)"",
                          ""options"": 5,
                          ""matchTimeout"": -10000
                        }
                      }";

        var r = JsonConvert.DeserializeObject<RegexTestClass>(json);

        Assert.Equal("(hi)", r.Regex.ToString());
        Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, r.Regex.Options);
    }

    [Fact]
    public void DeserializeStringRegex()
    {
        var json = @"{
  ""Regex"": ""\/abc\/""
}";

        var c = JsonConvert.DeserializeObject<RegexTestClass>(json, new JsonSerializerSettings
        {
            Converters = { new RegexConverter() }
        });

        Assert.Equal("abc", c.Regex.ToString());
        Assert.Equal(RegexOptions.None, c.Regex.Options);
    }

    [Fact]
    public void DeserializeStringRegex_NoStartSlash_Error()
    {
        var json = @"{
  ""Regex"": ""abc\/""
}";

        ExceptionAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<RegexTestClass>(json, new JsonSerializerSettings
            {
                Converters = { new RegexConverter() }
            }),
            "Regex pattern must be enclosed by slashes. Path 'Regex', line 2, position 18.");
    }

    [Fact]
    public void DeserializeStringRegex_NoEndSlash_Error()
    {
        var json = @"{
  ""Regex"": ""\/abc""
}";

        ExceptionAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<RegexTestClass>(json, new JsonSerializerSettings
            {
                Converters = {new RegexConverter()}
            }),
            "Regex pattern must be enclosed by slashes. Path 'Regex', line 2, position 18.");
    }

    [Fact]
    public void DeserializeStringRegex_NoStartAndEndSlashes_Error()
    {
        var json = @"{
  ""Regex"": ""abc""
}";

        ExceptionAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<RegexTestClass>(json, new JsonSerializerSettings
            {
                Converters = { new RegexConverter() }
            }),
            "Regex pattern must be enclosed by slashes. Path 'Regex', line 2, position 16.");
    }

#pragma warning disable 618
    [Fact]
    public void SerializeToBson()
    {
        var regex = new Regex("abc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new RegexConverter());

        serializer.Serialize(writer, new RegexTestClass { Regex = regex });

        var expected = "13-00-00-00-0B-52-65-67-65-78-00-61-62-63-00-69-75-00-00";
        var bson = ms.ToArray().BytesToHex();

        Assert.Equal(expected, bson);
    }

    [Fact]
    public void DeserializeFromBson()
    {
        var ms = new MemoryStream("13-00-00-00-0B-52-65-67-65-78-00-61-62-63-00-69-75-00-00".HexToBytes());
        var reader = new BsonReader(ms);
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new RegexConverter());

        var c = serializer.Deserialize<RegexTestClass>(reader);

        Assert.Equal("abc", c.Regex.ToString());
        Assert.Equal(RegexOptions.IgnoreCase, c.Regex.Options);
    }

    [Fact]
    public void ConvertEmptyRegexBson()
    {
        var regex = new Regex(string.Empty);

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new RegexConverter());

        serializer.Serialize(writer, new RegexTestClass { Regex = regex });

        ms.Seek(0, SeekOrigin.Begin);
        var reader = new BsonReader(ms);
        serializer.Converters.Add(new RegexConverter());

        var c = serializer.Deserialize<RegexTestClass>(reader);

        Assert.Equal("", c.Regex.ToString());
        Assert.Equal(RegexOptions.None, c.Regex.Options);
    }

    [Fact]
    public void ConvertRegexWithAllOptionsBson()
    {
        var regex = new Regex(
            "/",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new RegexConverter());

        serializer.Serialize(writer, new RegexTestClass { Regex = regex });

        var expected = "14-00-00-00-0B-52-65-67-65-78-00-2F-00-69-6D-73-75-78-00-00";
        var bson = ms.ToArray().BytesToHex();

        Assert.Equal(expected, bson);

        ms.Seek(0, SeekOrigin.Begin);
        var reader = new BsonReader(ms);
        serializer.Converters.Add(new RegexConverter());

        var c = serializer.Deserialize<RegexTestClass>(reader);

        Assert.Equal("/", c.Regex.ToString());
        Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture, c.Regex.Options);
    }
#pragma warning restore 618

    [Fact]
    public void DeserializeFromText()
    {
        var json = @"{
  ""Pattern"": ""abc"",
  ""Options"": 513
}";

        var newRegex = JsonConvert.DeserializeObject<Regex>(json, new RegexConverter());
        Assert.Equal("abc", newRegex.ToString());
        Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, newRegex.Options);
    }

    [Fact]
    public void ConvertEmptyRegexJson()
    {
        var regex = new Regex("");

        var json = JsonConvert.SerializeObject(new RegexTestClass { Regex = regex }, Formatting.Indented, new RegexConverter());

        StringAssert.AreEqual(@"{
  ""Regex"": {
    ""Pattern"": """",
    ""Options"": 0
  }
}", json);

        var newRegex = JsonConvert.DeserializeObject<RegexTestClass>(json, new RegexConverter());
        Assert.Equal("", newRegex.Regex.ToString());
        Assert.Equal(RegexOptions.None, newRegex.Regex.Options);
    }

    public class SimpleClassWithRegex
    {
        public Regex RegProp { get; set; }
    }

    [Fact]
    public void DeserializeNullRegex()
    {
        var json = JsonConvert.SerializeObject(new SimpleClassWithRegex { RegProp = null });
        Assert.Equal(@"{""RegProp"":null}", json);

        var obj = JsonConvert.DeserializeObject<SimpleClassWithRegex>(json);
        Assert.Equal(null, obj.RegProp);
    }
}