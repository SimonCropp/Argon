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
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);

        var converter = new RegexConverter();
        converter.WriteJson(jsonWriter, null, null);

        XUnitAssert.AreEqualNormalized(@"null", stringWriter.ToString());
    }

    [Fact]
    public void SerializeToText()
    {
        var regex = new Regex("abc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var json = JsonConvert.SerializeObject(regex, Formatting.Indented, new RegexConverter());

        XUnitAssert.AreEqualNormalized(@"{
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

        XUnitAssert.AreEqualNormalized(@"{
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

        XUnitAssert.Throws<JsonSerializationException>(
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

        XUnitAssert.Throws<JsonSerializationException>(
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

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<RegexTestClass>(json, new JsonSerializerSettings
            {
                Converters = { new RegexConverter() }
            }),
            "Regex pattern must be enclosed by slashes. Path 'Regex', line 2, position 16.");
    }

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

        XUnitAssert.AreEqualNormalized(@"{
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