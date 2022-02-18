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

using System.Collections.ObjectModel;
using System.Xml;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;

namespace Argon.Tests;

public class JsonConvertTest : TestFixtureBase
{
    [Fact]
    public void ToStringEnsureEscapedArrayLength()
    {
        const char nonAsciiChar = (char)257;
        const char escapableNonQuoteAsciiChar = '\0';

        var value = nonAsciiChar + @"\" + escapableNonQuoteAsciiChar;

        var convertedValue = JsonConvert.ToString((object)value);
        Assert.Equal(@"""" + nonAsciiChar + @"\\\u0000""", convertedValue);
    }

    public class PopulateTestObject
    {
        public decimal Prop { get; set; }
    }

    [Fact]
    public void PopulateObjectWithHeaderComment()
    {
        var json = @"// file header
{
  ""prop"": 1.0
}";

        var o = new PopulateTestObject();
        JsonConvert.PopulateObject(json, o);

        Assert.Equal(1m, o.Prop);
    }

    [Fact]
    public void PopulateObjectWithMultipleHeaderComment()
    {
        var json = @"// file header
// another file header?
{
  ""prop"": 1.0
}";

        var o = new PopulateTestObject();
        JsonConvert.PopulateObject(json, o);

        Assert.Equal(1m, o.Prop);
    }

    [Fact]
    public void PopulateObjectWithNoContent()
    {
        XUnitAssert.Throws<JsonSerializationException>(() =>
        {
            var json = @"";

            var o = new PopulateTestObject();
            JsonConvert.PopulateObject(json, o);
        }, "No JSON content found. Path '', line 0, position 0.");
    }

    [Fact]
    public void PopulateObjectWithOnlyComment()
    {
        var ex = XUnitAssert.Throws<JsonSerializationException>(() =>
        {
            var json = @"// file header";

            var o = new PopulateTestObject();
            JsonConvert.PopulateObject(json, o);
        }, "No JSON content found. Path '', line 1, position 14.");

        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(14, ex.LinePosition);
        Assert.Equal(string.Empty, ex.Path);
    }

    [Fact]
    public void DefaultSettings()
    {
        try
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(new { test = new[] { 1, 2, 3 } });

            XUnitAssert.AreEqualNormalized(@"{
  ""test"": [
    1,
    2,
    3
  ]
}", json);
        }
        finally
        {
            JsonConvert.DefaultSettings = null;
        }
    }

    public class NameTableTestClass
    {
        public string Value { get; set; }
    }

    public class NameTableTestClassConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            reader.Read();

            var jsonTextReader = (JsonTextReader)reader;
            Assert.NotNull(jsonTextReader.PropertyNameTable);

            var s = serializer.Deserialize<string>(reader);
            Assert.Equal("hi", s);
            Assert.NotNull(jsonTextReader.PropertyNameTable);

            var o = new NameTableTestClass
            {
                Value = s
            };

            return o;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NameTableTestClass);
        }
    }

    [Fact]
    public void NameTableTest()
    {
        var sr = new StringReader("{'property':'hi'}");
        var jsonTextReader = new JsonTextReader(sr);

        Assert.Null(jsonTextReader.PropertyNameTable);

        var serializer = new JsonSerializer();
        serializer.Converters.Add(new NameTableTestClassConverter());
        var o = serializer.Deserialize<NameTableTestClass>(jsonTextReader);

        Assert.Null(jsonTextReader.PropertyNameTable);
        Assert.Equal("hi", o.Value);
    }

    public class CustonNameTable : JsonNameTable
    {
        public override string Get(char[] key, int start, int length)
        {
            return "_" + new string(key, start, length);
        }
    }

    [Fact]
    public void CustonNameTableTest()
    {
        var sr = new StringReader("{'property':'hi'}");
        var jsonTextReader = new JsonTextReader(sr);

        Assert.Null(jsonTextReader.PropertyNameTable);
        var nameTable = jsonTextReader.PropertyNameTable = new CustonNameTable();

        var serializer = new JsonSerializer();
        var o = serializer.Deserialize<Dictionary<string, string>>(jsonTextReader);
        Assert.Equal("hi", o["_property"]);

        Assert.Equal(nameTable, jsonTextReader.PropertyNameTable);
    }

    [Fact]
    public void DefaultSettings_Example()
    {
        try
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var e = new Employee
            {
                FirstName = "Eric",
                LastName = "Example",
                BirthDate = new DateTime(1980, 4, 20, 0, 0, 0, DateTimeKind.Utc),
                Department = "IT",
                JobTitle = "Web Dude"
            };

            var json = JsonConvert.SerializeObject(e);
            // {
            //   "firstName": "Eric",
            //   "lastName": "Example",
            //   "birthDate": "1980-04-20T00:00:00Z",
            //   "department": "IT",
            //   "jobTitle": "Web Dude"
            // }

            XUnitAssert.AreEqualNormalized(@"{
  ""firstName"": ""Eric"",
  ""lastName"": ""Example"",
  ""birthDate"": ""1980-04-20T00:00:00Z"",
  ""department"": ""IT"",
  ""jobTitle"": ""Web Dude""
}", json);
        }
        finally
        {
            JsonConvert.DefaultSettings = null;
        }
    }

    [Fact]
    public void DefaultSettings_Override()
    {
        try
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(new { test = new[] { 1, 2, 3 } }, new JsonSerializerSettings
            {
                Formatting = Formatting.None
            });

            Assert.Equal(@"{""test"":[1,2,3]}", json);
        }
        finally
        {
            JsonConvert.DefaultSettings = null;
        }
    }

    [Fact]
    public void DefaultSettings_Override_JsonConverterOrder()
    {
        try
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy" } }
            };

            var json = JsonConvert.SerializeObject(new[] { new DateTime(2000, 12, 12, 4, 2, 4, DateTimeKind.Utc) }, new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                Converters =
                {
                    // should take precedence
                    new JavaScriptDateTimeConverter(),
                    new IsoDateTimeConverter { DateTimeFormat = "dd" }
                }
            });

            Assert.Equal(@"[new Date(976593724000)]", json);
        }
        finally
        {
            JsonConvert.DefaultSettings = null;
        }
    }

    [Fact]
    public void DefaultSettings_Create()
    {
        try
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            IList<int> l = new List<int> { 1, 2, 3 };

            var sw = new StringWriter();
            var serializer = JsonSerializer.CreateDefault();
            serializer.Serialize(sw, l);

            XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", sw.ToString());

            sw = new StringWriter();
            serializer.Formatting = Formatting.None;
            serializer.Serialize(sw, l);

            Assert.Equal(@"[1,2,3]", sw.ToString());

            sw = new StringWriter();
            serializer = new JsonSerializer();
            serializer.Serialize(sw, l);

            Assert.Equal(@"[1,2,3]", sw.ToString());

            sw = new StringWriter();
            serializer = JsonSerializer.Create();
            serializer.Serialize(sw, l);

            Assert.Equal(@"[1,2,3]", sw.ToString());
        }
        finally
        {
            JsonConvert.DefaultSettings = null;
        }
    }

    [Fact]
    public void DefaultSettings_CreateWithSettings()
    {
        try
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            IList<int> l = new List<int> { 1, 2, 3 };

            var sw = new StringWriter();
            var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                Converters = { new IntConverter() }
            });
            serializer.Serialize(sw, l);

            XUnitAssert.AreEqualNormalized(@"[
  2,
  4,
  6
]", sw.ToString());

            sw = new StringWriter();
            serializer.Converters.Clear();
            serializer.Serialize(sw, l);

            XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", sw.ToString());

            sw = new StringWriter();
            serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented });
            serializer.Serialize(sw, l);

            XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", sw.ToString());
        }
        finally
        {
            JsonConvert.DefaultSettings = null;
        }
    }

    public class IntConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var i = (int)value;
            writer.WriteValue(i * 2);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }
    }

    [Fact]
    public void DeserializeObject_EmptyString()
    {
        var result = JsonConvert.DeserializeObject(string.Empty);
        Assert.Null(result);
    }

    [Fact]
    public void DeserializeObject_Integer()
    {
        var result = JsonConvert.DeserializeObject("1");
        Assert.Equal(1L, result);
    }

    [Fact]
    public void DeserializeObject_Integer_EmptyString()
    {
        var value = JsonConvert.DeserializeObject<int?>("");
        Assert.Null(value);
    }

    [Fact]
    public void DeserializeObject_Decimal_EmptyString()
    {
        var value = JsonConvert.DeserializeObject<decimal?>("");
        Assert.Null(value);
    }

    [Fact]
    public void DeserializeObject_DateTime_EmptyString()
    {
        var value = JsonConvert.DeserializeObject<DateTime?>("");
        Assert.Null(value);
    }

    [Fact]
    public void EscapeJavaScriptString()
    {
        var result = JavaScriptUtils.ToEscapedJavaScriptString("How now brown cow?", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""How now brown cow?""", result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("How now 'brown' cow?", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""How now 'brown' cow?""", result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("How now <brown> cow?", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""How now <brown> cow?""", result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("How \r\nnow brown cow?", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""How \r\nnow brown cow?""", result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007""", result);

        result =
            JavaScriptUtils.ToEscapedJavaScriptString("\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013""", result);

        result =
            JavaScriptUtils.ToEscapedJavaScriptString(
                "\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f ", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f """, result);

        result =
            JavaScriptUtils.ToEscapedJavaScriptString(
                "!\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""!\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]""", result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("^_`abcdefghijklmnopqrstuvwxyz{|}~", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""^_`abcdefghijklmnopqrstuvwxyz{|}~""", result);

        var data =
            "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        var expected =
            @"""\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~""";

        result = JavaScriptUtils.ToEscapedJavaScriptString(data, '"', true, StringEscapeHandling.Default);
        Assert.Equal(expected, result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("Fred's cat.", '\'', true, StringEscapeHandling.Default);
        Assert.Equal(result, @"'Fred\'s cat.'");

        result = JavaScriptUtils.ToEscapedJavaScriptString(@"""How are you gentlemen?"" said Cats.", '"', true, StringEscapeHandling.Default);
        Assert.Equal(result, @"""\""How are you gentlemen?\"" said Cats.""");

        result = JavaScriptUtils.ToEscapedJavaScriptString(@"""How are' you gentlemen?"" said Cats.", '"', true, StringEscapeHandling.Default);
        Assert.Equal(result, @"""\""How are' you gentlemen?\"" said Cats.""");

        result = JavaScriptUtils.ToEscapedJavaScriptString(@"Fred's ""cat"".", '\'', true, StringEscapeHandling.Default);
        Assert.Equal(result, @"'Fred\'s ""cat"".'");

        result = JavaScriptUtils.ToEscapedJavaScriptString("\u001farray\u003caddress", '"', true, StringEscapeHandling.Default);
        Assert.Equal(result, @"""\u001farray<address""");
    }

    [Fact]
    public void EscapeJavaScriptString_UnicodeLinefeeds()
    {
        var result = JavaScriptUtils.ToEscapedJavaScriptString("before" + '\u0085' + "after", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""before\u0085after""", result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("before" + '\u2028' + "after", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""before\u2028after""", result);

        result = JavaScriptUtils.ToEscapedJavaScriptString("before" + '\u2029' + "after", '"', true, StringEscapeHandling.Default);
        Assert.Equal(@"""before\u2029after""", result);
    }

    [Fact]
    public void ToStringInvalid()
    {
        XUnitAssert.Throws<ArgumentException>(() => { JsonConvert.ToString(new Version(1, 0)); }, "Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation.");
    }

    [Fact]
    public void GuidToString()
    {
        var guid = new Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D");
        var json = JsonConvert.ToString(guid);
        Assert.Equal(@"""bed7f4ea-1a96-11d2-8f08-00a0c9a6186d""", json);
    }

    [Fact]
    public void EnumToString()
    {
        var json = JsonConvert.ToString(StringComparison.CurrentCultureIgnoreCase);
        Assert.Equal("1", json);
    }

    [Fact]
    public void ObjectToString()
    {
        object value = 1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = 1.1;
        Assert.Equal("1.1", JsonConvert.ToString(value));

        value = 1.1m;
        Assert.Equal("1.1", JsonConvert.ToString(value));

        value = (float)1.1;
        Assert.Equal("1.1", JsonConvert.ToString(value));

        value = (short)1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = (long)1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = (byte)1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = (uint)1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = (ushort)1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = (sbyte)1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = (ulong)1;
        Assert.Equal("1", JsonConvert.ToString(value));

        value = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
        Assert.Equal(@"""1970-01-01T00:00:00Z""", JsonConvert.ToString(value));

        value = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
        Assert.Equal(@"""\/Date(0)\/""", JsonConvert.ToString((DateTime)value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.RoundtripKind));

        value = new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero);
        Assert.Equal(@"""1970-01-01T00:00:00+00:00""", JsonConvert.ToString(value));

        value = new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero);
        Assert.Equal(@"""\/Date(0+0000)\/""", JsonConvert.ToString((DateTimeOffset)value, DateFormatHandling.MicrosoftDateFormat));

        value = null;
        Assert.Equal("null", JsonConvert.ToString(value));

#if !NET5_0_OR_GREATER
            value = DBNull.Value;
            Assert.Equal("null", JsonConvert.ToString(value));
#endif

        value = "I am a string";
        Assert.Equal(@"""I am a string""", JsonConvert.ToString(value));

        value = true;
        Assert.Equal("true", JsonConvert.ToString(value));

        value = 'c';
        Assert.Equal(@"""c""", JsonConvert.ToString(value));
    }

    [Fact]
    public void TestInvalidStrings()
    {
        XUnitAssert.Throws<JsonReaderException>(() =>
        {
            var orig = @"this is a string ""that has quotes"" ";

            var serialized = JsonConvert.SerializeObject(orig);

            // *** Make string invalid by stripping \" \"
            serialized = serialized.Replace(@"\""", "\"");

            JsonConvert.DeserializeObject<string>(serialized);
        }, "Additional text encountered after finished reading JSON content: t. Path '', line 1, position 19.");
    }

    [Fact]
    public void DeserializeValueObjects()
    {
        var i = JsonConvert.DeserializeObject<int>("1");
        Assert.Equal(1, i);

        var d = JsonConvert.DeserializeObject<DateTimeOffset>(@"""\/Date(-59011455539000+0000)\/""");
        Assert.Equal(new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)), d);

        var b = JsonConvert.DeserializeObject<bool>("true");
        XUnitAssert.True(b);

        var n = JsonConvert.DeserializeObject<object>("null");
        Assert.Equal(null, n);

        var u = JsonConvert.DeserializeObject<object>("undefined");
        Assert.Equal(null, u);
    }

    [Fact]
    public void FloatToString()
    {
        Assert.Equal("1.1", JsonConvert.ToString(1.1));
        Assert.Equal("1.11", JsonConvert.ToString(1.11));
        Assert.Equal("1.111", JsonConvert.ToString(1.111));
        Assert.Equal("1.1111", JsonConvert.ToString(1.1111));
        Assert.Equal("1.11111", JsonConvert.ToString(1.11111));
        Assert.Equal("1.111111", JsonConvert.ToString(1.111111));
        Assert.Equal("1.0", JsonConvert.ToString(1.0));
        Assert.Equal("1.0", JsonConvert.ToString(1d));
        Assert.Equal("-1.0", JsonConvert.ToString(-1d));
        Assert.Equal("1.01", JsonConvert.ToString(1.01));
        Assert.Equal("1.001", JsonConvert.ToString(1.001));
        Assert.Equal(JsonConvert.PositiveInfinity, JsonConvert.ToString(Double.PositiveInfinity));
        Assert.Equal(JsonConvert.NegativeInfinity, JsonConvert.ToString(Double.NegativeInfinity));
        Assert.Equal(JsonConvert.NaN, JsonConvert.ToString(Double.NaN));
    }

    [Fact]
    public void DecimalToString()
    {
        Assert.Equal("1.1", JsonConvert.ToString(1.1m));
        Assert.Equal("1.11", JsonConvert.ToString(1.11m));
        Assert.Equal("1.111", JsonConvert.ToString(1.111m));
        Assert.Equal("1.1111", JsonConvert.ToString(1.1111m));
        Assert.Equal("1.11111", JsonConvert.ToString(1.11111m));
        Assert.Equal("1.111111", JsonConvert.ToString(1.111111m));
        Assert.Equal("1.0", JsonConvert.ToString(1.0m));
        Assert.Equal("-1.0", JsonConvert.ToString(-1.0m));
        Assert.Equal("-1.0", JsonConvert.ToString(-1m));
        Assert.Equal("1.0", JsonConvert.ToString(1m));
        Assert.Equal("1.01", JsonConvert.ToString(1.01m));
        Assert.Equal("1.001", JsonConvert.ToString(1.001m));
        Assert.Equal("79228162514264337593543950335.0", JsonConvert.ToString(Decimal.MaxValue));
        Assert.Equal("-79228162514264337593543950335.0", JsonConvert.ToString(Decimal.MinValue));
    }

    [Fact]
    public void StringEscaping()
    {
        var v = "It's a good day\r\n\"sunshine\"";

        var json = JsonConvert.ToString(v);
        Assert.Equal(@"""It's a good day\r\n\""sunshine\""""", json);
    }

    [Fact]
    public void ToStringStringEscapeHandling()
    {
        var v = "<b>hi " + '\u20AC' + "</b>";

        var json = JsonConvert.ToString(v, '"');
        Assert.Equal(@"""<b>hi " + '\u20AC' + @"</b>""", json);

        json = JsonConvert.ToString(v, '"', StringEscapeHandling.EscapeHtml);
        Assert.Equal(@"""\u003cb\u003ehi " + '\u20AC' + @"\u003c/b\u003e""", json);

        json = JsonConvert.ToString(v, '"', StringEscapeHandling.EscapeNonAscii);
        Assert.Equal(@"""<b>hi \u20ac</b>""", json);
    }

    [Fact]
    public void WriteDateTime()
    {
        var result = TestDateTime("DateTime Max", DateTime.MaxValue);
        Assert.Equal("9999-12-31T23:59:59.9999999", result.IsoDateRoundtrip);
        Assert.Equal("9999-12-31T23:59:59.9999999" + DateTime.MaxValue.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("9999-12-31T23:59:59.9999999", result.IsoDateUnspecified);
        Assert.Equal("9999-12-31T23:59:59.9999999Z", result.IsoDateUtc);
        Assert.Equal(@"\/Date(253402300799999)\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(253402300799999" + DateTime.MaxValue.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(253402300799999)\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(253402300799999)\/", result.MsDateUtc);

        var year2000local = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Local);
        var localToUtcDate = year2000local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");

        result = TestDateTime("DateTime Local", year2000local);
        Assert.Equal("2000-01-01T01:01:01" + year2000local.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateRoundtrip);
        Assert.Equal("2000-01-01T01:01:01" + year2000local.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("2000-01-01T01:01:01", result.IsoDateUnspecified);
        Assert.Equal(localToUtcDate, result.IsoDateUtc);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + year2000local.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + year2000local.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + year2000local.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + @")\/", result.MsDateUtc);

        var millisecondsLocal = new DateTime(2000, 1, 1, 1, 1, 1, 999, DateTimeKind.Local);
        localToUtcDate = millisecondsLocal.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");

        result = TestDateTime("DateTime Local with milliseconds", millisecondsLocal);
        Assert.Equal("2000-01-01T01:01:01.999" + millisecondsLocal.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateRoundtrip);
        Assert.Equal("2000-01-01T01:01:01.999" + millisecondsLocal.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("2000-01-01T01:01:01.999", result.IsoDateUnspecified);
        Assert.Equal(localToUtcDate, result.IsoDateUtc);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + millisecondsLocal.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + millisecondsLocal.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + millisecondsLocal.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + @")\/", result.MsDateUtc);

        var ticksLocal = new DateTime(636556897826822481, DateTimeKind.Local);
        localToUtcDate = ticksLocal.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");

        result = TestDateTime("DateTime Local with ticks", ticksLocal);
        Assert.Equal("2018-03-03T16:03:02.6822481" + ticksLocal.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateRoundtrip);
        Assert.Equal("2018-03-03T16:03:02.6822481" + ticksLocal.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("2018-03-03T16:03:02.6822481", result.IsoDateUnspecified);
        Assert.Equal(localToUtcDate, result.IsoDateUtc);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + ticksLocal.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + ticksLocal.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + ticksLocal.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + @")\/", result.MsDateUtc);

        var year2000Unspecified = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified);

        result = TestDateTime("DateTime Unspecified", year2000Unspecified);
        Assert.Equal("2000-01-01T01:01:01", result.IsoDateRoundtrip);
        Assert.Equal("2000-01-01T01:01:01" + year2000Unspecified.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("2000-01-01T01:01:01", result.IsoDateUnspecified);
        Assert.Equal("2000-01-01T01:01:01Z", result.IsoDateUtc);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified) + year2000Unspecified.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified) + year2000Unspecified.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified) + year2000Unspecified.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified.ToLocalTime()) + @")\/", result.MsDateUtc);

        var year2000Utc = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
        var utcTolocalDate = year2000Utc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss");

        result = TestDateTime("DateTime Utc", year2000Utc);
        Assert.Equal("2000-01-01T01:01:01Z", result.IsoDateRoundtrip);
        Assert.Equal(utcTolocalDate + year2000Utc.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("2000-01-01T01:01:01", result.IsoDateUnspecified);
        Assert.Equal("2000-01-01T01:01:01Z", result.IsoDateUtc);
        Assert.Equal(@"\/Date(946688461000)\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(946688461000" + year2000Utc.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(DateTime.SpecifyKind(year2000Utc, DateTimeKind.Unspecified)) + year2000Utc.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(946688461000)\/", result.MsDateUtc);

        var unixEpoc = new DateTime(621355968000000000, DateTimeKind.Utc);
        utcTolocalDate = unixEpoc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss");

        result = TestDateTime("DateTime Unix Epoc", unixEpoc);
        Assert.Equal("1970-01-01T00:00:00Z", result.IsoDateRoundtrip);
        Assert.Equal(utcTolocalDate + unixEpoc.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("1970-01-01T00:00:00", result.IsoDateUnspecified);
        Assert.Equal("1970-01-01T00:00:00Z", result.IsoDateUtc);
        Assert.Equal(@"\/Date(0)\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(0" + unixEpoc.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(DateTime.SpecifyKind(unixEpoc, DateTimeKind.Unspecified)) + unixEpoc.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(0)\/", result.MsDateUtc);

        result = TestDateTime("DateTime Min", DateTime.MinValue);
        Assert.Equal("0001-01-01T00:00:00", result.IsoDateRoundtrip);
        Assert.Equal("0001-01-01T00:00:00" + DateTime.MinValue.GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("0001-01-01T00:00:00", result.IsoDateUnspecified);
        Assert.Equal("0001-01-01T00:00:00Z", result.IsoDateUtc);
        Assert.Equal(@"\/Date(-62135596800000)\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(-62135596800000" + DateTime.MinValue.GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(-62135596800000)\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(-62135596800000)\/", result.MsDateUtc);

        result = TestDateTime("DateTime Default", default(DateTime));
        Assert.Equal("0001-01-01T00:00:00", result.IsoDateRoundtrip);
        Assert.Equal("0001-01-01T00:00:00" + default(DateTime).GetOffset(DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
        Assert.Equal("0001-01-01T00:00:00", result.IsoDateUnspecified);
        Assert.Equal("0001-01-01T00:00:00Z", result.IsoDateUtc);
        Assert.Equal(@"\/Date(-62135596800000)\/", result.MsDateRoundtrip);
        Assert.Equal(@"\/Date(-62135596800000" + default(DateTime).GetOffset(DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
        Assert.Equal(@"\/Date(-62135596800000)\/", result.MsDateUnspecified);
        Assert.Equal(@"\/Date(-62135596800000)\/", result.MsDateUtc);

        result = TestDateTime("DateTimeOffset TimeSpan Zero", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));
        Assert.Equal("2000-01-01T01:01:01+00:00", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(946688461000+0000)\/", result.MsDateRoundtrip);

        result = TestDateTime("DateTimeOffset TimeSpan 1 hour", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(1)));
        Assert.Equal("2000-01-01T01:01:01+01:00", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(946684861000+0100)\/", result.MsDateRoundtrip);

        result = TestDateTime("DateTimeOffset TimeSpan 1.5 hour", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(1.5)));
        Assert.Equal("2000-01-01T01:01:01+01:30", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(946683061000+0130)\/", result.MsDateRoundtrip);

        result = TestDateTime("DateTimeOffset TimeSpan 13 hour", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)));
        Assert.Equal("2000-01-01T01:01:01+13:00", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(946641661000+1300)\/", result.MsDateRoundtrip);

        result = TestDateTime("DateTimeOffset TimeSpan with ticks", new DateTimeOffset(634663873826822481, TimeSpan.Zero));
        Assert.Equal("2012-03-03T16:03:02.6822481+00:00", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(1330790582682+0000)\/", result.MsDateRoundtrip);

        result = TestDateTime("DateTimeOffset Min", DateTimeOffset.MinValue);
        Assert.Equal("0001-01-01T00:00:00+00:00", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(-62135596800000+0000)\/", result.MsDateRoundtrip);

        result = TestDateTime("DateTimeOffset Max", DateTimeOffset.MaxValue);
        Assert.Equal("9999-12-31T23:59:59.9999999+00:00", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(253402300799999+0000)\/", result.MsDateRoundtrip);

        result = TestDateTime("DateTimeOffset Default", default(DateTimeOffset));
        Assert.Equal("0001-01-01T00:00:00+00:00", result.IsoDateRoundtrip);
        Assert.Equal(@"\/Date(-62135596800000+0000)\/", result.MsDateRoundtrip);
    }

    public class DateTimeResult
    {
        public string IsoDateRoundtrip { get; set; }
        public string IsoDateLocal { get; set; }
        public string IsoDateUnspecified { get; set; }
        public string IsoDateUtc { get; set; }

        public string MsDateRoundtrip { get; set; }
        public string MsDateLocal { get; set; }
        public string MsDateUnspecified { get; set; }
        public string MsDateUtc { get; set; }
    }

    DateTimeResult TestDateTime<T>(string name, T value)
    {
        Console.WriteLine(name);

        var result = new DateTimeResult
        {
            IsoDateRoundtrip = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind)
        };

        if (value is DateTime)
        {
            result.IsoDateLocal = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.Local);
            result.IsoDateUnspecified = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.Unspecified);
            result.IsoDateUtc = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.Utc);
        }

        result.MsDateRoundtrip = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.RoundtripKind);
        if (value is DateTime)
        {
            result.MsDateLocal = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.Local);
            result.MsDateUnspecified = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.Unspecified);
            result.MsDateUtc = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.Utc);
        }

        TestDateTimeFormat(value, new IsoDateTimeConverter());

        if (value is DateTime)
        {
            Console.WriteLine(XmlConvert.ToString((DateTime)(object)value, XmlDateTimeSerializationMode.RoundtripKind));
        }
        else
        {
            Console.WriteLine(XmlConvert.ToString((DateTimeOffset)(object)value));
        }

        var ms = new MemoryStream();
        var s = new DataContractSerializer(typeof(T));
        s.WriteObject(ms, value);
        var json = Encoding.UTF8.GetString(ms.ToArray(), 0, Convert.ToInt32(ms.Length));
        Console.WriteLine(json);

        Console.WriteLine();

        return result;
    }

    static string TestDateTimeFormat<T>(T value, DateFormatHandling format, DateTimeZoneHandling timeZoneHandling)
    {
        string date = null;

        if (value is DateTime)
        {
            date = JsonConvert.ToString((DateTime)(object)value, format, timeZoneHandling);
        }
        else
        {
            date = JsonConvert.ToString((DateTimeOffset)(object)value, format);
        }

        Console.WriteLine(format.ToString("g") + "-" + timeZoneHandling.ToString("g") + ": " + date);

        if (timeZoneHandling == DateTimeZoneHandling.RoundtripKind)
        {
            var parsed = JsonConvert.DeserializeObject<T>(date);
            if (!value.Equals(parsed))
            {
                var valueTicks = GetTicks(value);
                var parsedTicks = GetTicks(parsed);

                valueTicks = valueTicks / 10000 * 10000;

                Assert.Equal(valueTicks, parsedTicks);
            }
        }

        return date.Trim('"');
    }

    static void TestDateTimeFormat<T>(T value, JsonConverter converter)
    {
        var date = Write(value, converter);

        Console.WriteLine(converter.GetType().Name + ": " + date);

        var parsed = Read<T>(date, converter);

        try
        {
            Assert.Equal(value, parsed);
        }
        catch (Exception)
        {
            // JavaScript ticks aren't as precise, recheck after rounding
            var valueTicks = GetTicks(value);
            var parsedTicks = GetTicks(parsed);

            valueTicks = valueTicks / 10000 * 10000;

            Assert.Equal(valueTicks, parsedTicks);
        }
    }

    public static long GetTicks(object value)
    {
        return value is DateTime ? ((DateTime)value).Ticks : ((DateTimeOffset)value).Ticks;
    }

    public static string Write(object value, JsonConverter converter)
    {
        var sw = new StringWriter();
        var writer = new JsonTextWriter(sw);
        converter.WriteJson(writer, value, null);

        writer.Flush();
        return sw.ToString();
    }

    public static T Read<T>(string text, JsonConverter converter)
    {
        var reader = new JsonTextReader(new StringReader(text));
        reader.ReadAsString();

        return (T)converter.ReadJson(reader, typeof(T), null, null);
    }

    [Fact]
    public void SerializeObjectDateTimeZoneHandling()
    {
        var json = JsonConvert.SerializeObject(
            new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified),
            new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });

        Assert.Equal(@"""2000-01-01T01:01:01Z""", json);
    }

    [Fact]
    public void DeserializeObject()
    {
        var json = @"{
        'Name': 'Bad Boys',
        'ReleaseDate': '1995-4-7T00:00:00',
        'Genres': [
          'Action',
          'Comedy'
        ]
      }";

        var m = JsonConvert.DeserializeObject<Movie>(json);

        var name = m.Name;
        // Bad Boys

        Assert.Equal("Bad Boys", m.Name);
    }

    [Fact]
    public void TestJsonDateTimeOffsetRoundtrip()
    {
        var now = DateTimeOffset.Now;
        var dict = new Dictionary<string, object> { { "foo", now } };

        var settings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
        };
        var json = JsonConvert.SerializeObject(dict, settings);

        var newDict = new Dictionary<string, object>();
        JsonConvert.PopulateObject(json, newDict, settings);

        var date = newDict["foo"];

        Assert.Equal(date, now);
    }

    [Fact]
    public void MaximumDateTimeOffsetLength()
    {
        var dt = new DateTimeOffset(2000, 12, 31, 20, 59, 59, new TimeSpan(0, 11, 33, 0, 0));
        dt = dt.AddTicks(9999999);

        var sw = new StringWriter();
        var writer = new JsonTextWriter(sw);

        writer.WriteValue(dt);
        writer.Flush();

        Assert.Equal(@"""2000-12-31T20:59:59.9999999+11:33""", sw.ToString());
    }

    [Fact]
    public void MaximumDateTimeLength()
    {
        var dt = new DateTime(2000, 12, 31, 20, 59, 59, DateTimeKind.Local);
        dt = dt.AddTicks(9999999);

        var sw = new StringWriter();
        var writer = new JsonTextWriter(sw);

        writer.WriteValue(dt);
        writer.Flush();
    }

    [Fact]
    public void MaximumDateTimeMicrosoftDateFormatLength()
    {
        var dt = DateTime.MaxValue;

        var sw = new StringWriter();
        var writer = new JsonTextWriter(sw)
        {
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        };
        writer.WriteValue(dt);
        writer.Flush();
    }

    [Fact]
    public void IntegerLengthOverflows()
    {
        // Maximum javascript number length (in characters) is 380
        var o = JObject.Parse(@"{""biginteger"":" + new String('9', 380) + "}");
        var v = (JValue)o["biginteger"];
        Assert.Equal(JTokenType.Integer, v.Type);
        Assert.Equal(typeof(BigInteger), v.Value.GetType());
        Assert.Equal(BigInteger.Parse(new String('9', 380)), (BigInteger)v.Value);

        XUnitAssert.Throws<JsonReaderException>(() => JObject.Parse(@"{""biginteger"":" + new String('9', 381) + "}"), "JSON integer " + new String('9', 381) + " is too large to parse. Path 'biginteger', line 1, position 395.");
    }

    [Fact]
    public void ParseIsoDate()
    {
        var sr = new StringReader(@"""2014-02-14T14:25:02-13:00""");

        JsonReader jsonReader = new JsonTextReader(sr);

        Assert.True(jsonReader.Read());
        Assert.Equal(typeof(DateTime), jsonReader.ValueType);
    }

#if false
        [Fact]
        public void StackOverflowTest()
        {
            StringBuilder sb = new StringBuilder();

            int depth = 900;
            for (int i = 0; i < depth; i++)
            {
                sb.Append("{'A':");
            }

            // invalid json
            sb.Append("{***}");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("}");
            }

            string json = sb.ToString();
            JsonSerializer serializer = new JsonSerializer() { };
            serializer.Deserialize<Nest>(new JsonTextReader(new StringReader(json)));
        }
#endif

    public class Nest
    {
        public Nest A { get; set; }
    }

    [Fact]
    public void ParametersPassedToJsonConverterConstructor()
    {
        var clobber = new ClobberMyProperties { One = "Red", Two = "Green", Three = "Yellow", Four = "Black" };
        var json = JsonConvert.SerializeObject(clobber);

        Assert.Equal("{\"One\":\"Uno-1-Red\",\"Two\":\"Dos-2-Green\",\"Three\":\"Tres-1337-Yellow\",\"Four\":\"Black\"}", json);
    }

    public class ClobberMyProperties
    {
        [JsonConverter(typeof(ClobberingJsonConverter), "Uno", 1)]
        public string One { get; set; }

        [JsonConverter(typeof(ClobberingJsonConverter), "Dos", 2)]
        public string Two { get; set; }

        [JsonConverter(typeof(ClobberingJsonConverter), "Tres")]
        public string Three { get; set; }

        public string Four { get; set; }
    }

    public class ClobberingJsonConverter : JsonConverter
    {
        public string ClobberValueString { get; private set; }

        public int ClobberValueInt { get; private set; }

        public ClobberingJsonConverter(string clobberValueString, int clobberValueInt)
        {
            ClobberValueString = clobberValueString;
            ClobberValueInt = clobberValueInt;
        }

        public ClobberingJsonConverter(string clobberValueString)
            : this(clobberValueString, 1337)
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ClobberValueString + "-" + ClobberValueInt.ToString() + "-" + value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }

    [Fact]
    public void WrongParametersPassedToJsonConvertConstructorShouldThrow()
    {
        var value = new IncorrectJsonConvertParameters { One = "Boom" };

        XUnitAssert.Throws<JsonException>(() => { JsonConvert.SerializeObject(value); });
    }

    public class IncorrectJsonConvertParameters
    {
        /// <summary>
        /// We deliberately use the wrong number/type of arguments for ClobberingJsonConverter to ensure an
        /// exception is thrown.
        /// </summary>
        [JsonConverter(typeof(ClobberingJsonConverter), "Uno", "Blammo")]
        public string One { get; set; }
    }


    public class OverloadsJsonConverterer : JsonConverter
    {
        readonly string _type;

        // constructor with Type argument

        public OverloadsJsonConverterer(Type typeParam)
        {
            _type = "Type";
        }

        public OverloadsJsonConverterer(object objectParam)
        {
            _type = $"object({objectParam.GetType().FullName})";
        }

        // primitive type conversions

        public OverloadsJsonConverterer(byte byteParam)
        {
            _type = "byte";
        }

        public OverloadsJsonConverterer(short shortParam)
        {
            _type = "short";
        }

        public OverloadsJsonConverterer(int intParam)
        {
            _type = "int";
        }

        public OverloadsJsonConverterer(long longParam)
        {
            _type = "long";
        }

        public OverloadsJsonConverterer(double doubleParam)
        {
            _type = "double";
        }

        // params argument

        public OverloadsJsonConverterer(params int[] intParams)
        {
            _type = "int[]";
        }

        public OverloadsJsonConverterer(bool[] intParams)
        {
            _type = "bool[]";
        }

        // closest type resolution

        public OverloadsJsonConverterer(IEnumerable<string> iEnumerableParam)
        {
            _type = "IEnumerable<string>";
        }

        public OverloadsJsonConverterer(IList<string> iListParam)
        {
            _type = "IList<string>";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(_type);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }

    }

    public class OverloadWithTypeParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), typeof(int))]
        public int Overload { get; set; }
    }

    [Fact]
    public void JsonConverterConstructor_OverloadWithTypeParam()
    {
        var value = new OverloadWithTypeParameter();
        var json = JsonConvert.SerializeObject(value);

        Assert.Equal("{\"Overload\":\"Type\"}", json);
    }

    public class OverloadWithUnhandledParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), "str")]
        public int Overload { get; set; }
    }

    [Fact]
    public void JsonConverterConstructor_OverloadWithUnhandledParam_FallbackToObject()
    {
        var value = new OverloadWithUnhandledParameter();
        var json = JsonConvert.SerializeObject(value);

        Assert.Equal("{\"Overload\":\"object(System.String)\"}", json);
    }

    public class OverloadWithIntParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), 1)]
        public int Overload { get; set; }
    }

    public class OverloadWithUIntParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), 1U)]
        public int Overload { get; set; }
    }

    public class OverloadWithLongParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), 1L)]
        public int Overload { get; set; }
    }

    public class OverloadWithULongParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), 1UL)]
        public int Overload { get; set; }
    }

    public class OverloadWithShortParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), (short)1)]
        public int Overload { get; set; }
    }

    public class OverloadWithUShortParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), (ushort)1)]
        public int Overload { get; set; }
    }

    public class OverloadWithSByteParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), (sbyte)1)]
        public int Overload { get; set; }
    }

    public class OverloadWithByteParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), (byte)1)]
        public int Overload { get; set; }
    }

    public class OverloadWithCharParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), 'a')]
        public int Overload { get; set; }
    }

    public class OverloadWithBoolParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), true)]
        public int Overload { get; set; }
    }

    public class OverloadWithFloatParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), 1.5f)]
        public int Overload { get; set; }
    }

    public class OverloadWithDoubleParameter
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), 1.5)]
        public int Overload { get; set; }
    }

    [Fact]
    public void JsonConverterConstructor_OverloadsWithPrimitiveParams()
    {
        {
            var value = new OverloadWithIntParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"int\"}", json);
        }

        {
            // uint -> long
            var value = new OverloadWithUIntParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"long\"}", json);
        }

        {
            var value = new OverloadWithLongParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"long\"}", json);
        }

        {
            // ulong -> double
            var value = new OverloadWithULongParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"double\"}", json);
        }

        {
            var value = new OverloadWithShortParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"short\"}", json);
        }

        {
            // ushort -> int
            var value = new OverloadWithUShortParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"int\"}", json);
        }

        {
            // sbyte -> short
            var value = new OverloadWithSByteParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"short\"}", json);
        }

        {
            var value = new OverloadWithByteParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"byte\"}", json);
        }

        {
            // char -> int
            var value = new OverloadWithCharParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"int\"}", json);
        }

        {
            // bool -> (object)bool
            var value = new OverloadWithBoolParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"object(System.Boolean)\"}", json);
        }

        {
            // float -> double
            var value = new OverloadWithFloatParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"double\"}", json);
        }

        {
            var value = new OverloadWithDoubleParameter();
            var json = JsonConvert.SerializeObject(value);
            Assert.Equal("{\"Overload\":\"double\"}", json);
        }
    }

    public class OverloadWithArrayParameters
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), new[] { 1, 2, 3 })]
        public int WithParams { get; set; }

        [JsonConverter(typeof(OverloadsJsonConverterer), new[] { true, false })]
        public int WithoutParams { get; set; }
    }

    [Fact]
    public void JsonConverterConstructor_OverloadsWithArrayParams()
    {
        var value = new OverloadWithArrayParameters();
        var json = JsonConvert.SerializeObject(value);

        Assert.Equal("{\"WithParams\":\"int[]\",\"WithoutParams\":\"bool[]\"}", json);
    }

    public class OverloadWithBaseType
    {
        [JsonConverter(typeof(OverloadsJsonConverterer), new object[] { new[] { "a", "b", "c" } })]
        public int Overload { get; set; }
    }

    //[Fact]
    //[Ignore("https://github.com/dotnet/roslyn/issues/36974")]
    //public void JsonConverterConstructor_OverloadsWithBaseTypes()
    //{
    //    OverloadWithBaseType value = new OverloadWithBaseType();
    //    string json = JsonConvert.SerializeObject(value);

    //    Assert.AreEqual("{\"Overload\":\"IList<string>\"}", json);
    //}


    [Fact]
    public void CustomDoubleRounding()
    {
        var measurements = new Measurements
        {
            Loads = new List<double> { 23283.567554707258, 23224.849899771067, 23062.5, 22846.272519910868, 22594.281246368635 },
            Positions = new List<double> { 57.724227689317019, 60.440934405753069, 63.444192925248643, 66.813119113482557, 70.4496501404433 },
            Gain = 12345.67895111213
        };

        var json = JsonConvert.SerializeObject(measurements);

        Assert.Equal("{\"Positions\":[57.72,60.44,63.44,66.81,70.45],\"Loads\":[23284.0,23225.0,23062.0,22846.0,22594.0],\"Gain\":12345.679}", json);
    }

    public class Measurements
    {
        [JsonProperty(ItemConverterType = typeof(RoundingJsonConverter))]
        public List<double> Positions { get; set; }

        [JsonProperty(ItemConverterType = typeof(RoundingJsonConverter), ItemConverterParameters = new object[] { 0, MidpointRounding.ToEven })]
        public List<double> Loads { get; set; }

        [JsonConverter(typeof(RoundingJsonConverter), 4)]
        public double Gain { get; set; }
    }

    public class RoundingJsonConverter : JsonConverter
    {
        int _precision;
        MidpointRounding _rounding;

        public RoundingJsonConverter()
            : this(2)
        {
        }

        public RoundingJsonConverter(int precision)
            : this(precision, MidpointRounding.AwayFromZero)
        {
        }

        public RoundingJsonConverter(int precision, MidpointRounding rounding)
        {
            _precision = precision;
            _rounding = rounding;
        }

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Math.Round((double)value, _precision, _rounding));
        }
    }

    [Fact]
    public void GenericBaseClassSerialization()
    {
        var json = JsonConvert.SerializeObject(new NonGenericChildClass());
        Assert.Equal("{\"Data\":null}", json);
    }

    public class GenericBaseClass<O, T>
    {
        public virtual T Data { get; set; }
    }

    public class GenericIntermediateClass<O> : GenericBaseClass<O, string>
    {
        public override string Data { get; set; }
    }

    public class NonGenericChildClass : GenericIntermediateClass<int>
    {
    }

    [Fact]
    public void ShouldNotPopulateReadOnlyEnumerableObjectWithNonDefaultConstructor()
    {
        object actual = JsonConvert.DeserializeObject<HasReadOnlyEnumerableObject>("{\"foo\":{}}");
        Assert.NotNull(actual);
    }

    [Fact]
    public void ShouldNotPopulateReadOnlyEnumerableObjectWithDefaultConstructor()
    {
        object actual = JsonConvert.DeserializeObject<HasReadOnlyEnumerableObjectAndDefaultConstructor>("{\"foo\":{}}");
        Assert.NotNull(actual);
    }

    [Fact]
    public void ShouldNotPopulateContructorArgumentEnumerableObject()
    {
        object actual = JsonConvert.DeserializeObject<AcceptsEnumerableObjectToConstructor>("{\"foo\":{}}");
        Assert.NotNull(actual);
    }

    [Fact]
    public void ShouldNotPopulateEnumerableObjectProperty()
    {
        object actual = JsonConvert.DeserializeObject<HasEnumerableObject>("{\"foo\":{}}");
        Assert.NotNull(actual);
    }

    [Fact]
    public void ShouldNotPopulateReadOnlyDictionaryObjectWithNonDefaultConstructor()
    {
        object actual = JsonConvert.DeserializeObject<HasReadOnlyDictionary>("{\"foo\":{'key':'value'}}");
        Assert.NotNull(actual);
    }

    public sealed class HasReadOnlyDictionary
    {
        [JsonProperty("foo")]
        public IReadOnlyDictionary<string, string> Foo { get; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        [JsonConstructor]
        public HasReadOnlyDictionary([JsonProperty("bar")] int bar)
        {

        }
    }

    public sealed class HasReadOnlyEnumerableObject
    {
        [JsonProperty("foo")]
        public EnumerableWithConverter Foo { get; } = new();

        [JsonConstructor]
        public HasReadOnlyEnumerableObject([JsonProperty("bar")] int bar)
        {

        }
    }

    public sealed class HasReadOnlyEnumerableObjectAndDefaultConstructor
    {
        [JsonProperty("foo")]
        public EnumerableWithConverter Foo { get; } = new();

        [JsonConstructor]
        public HasReadOnlyEnumerableObjectAndDefaultConstructor()
        {

        }
    }

    public sealed class AcceptsEnumerableObjectToConstructor
    {
        [JsonConstructor]
        public AcceptsEnumerableObjectToConstructor
        (
            [JsonProperty("foo")] EnumerableWithConverter foo,
            [JsonProperty("bar")] int bar
        )
        {

        }
    }

    public sealed class HasEnumerableObject
    {
        [JsonProperty("foo")]
        public EnumerableWithConverter Foo { get; set; } = new();

        [JsonConstructor]
        public HasEnumerableObject([JsonProperty("bar")] int bar)
        {

        }
    }

    [JsonConverter(typeof(Converter))]
    public sealed class EnumerableWithConverter : IEnumerable<int>
    {
        public sealed class Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
                => objectType == typeof(Foo);

            public override object ReadJson
                (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                reader.Skip();
                return new EnumerableWithConverter();
            }

            public override void WriteJson
                (JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void ShouldNotRequireIgnoredPropertiesWithItemsRequired()
    {
        var json = @"{
  ""exp"": 1483228800,
  ""active"": true
}";
        var value = JsonConvert.DeserializeObject<ItemsRequiredObjectWithIgnoredProperty>(json);
        Assert.NotNull(value);
        Assert.Equal(value.Expiration, new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(value.Active, true);
    }

    [JsonObject(ItemRequired = Required.Always)]
    public sealed class ItemsRequiredObjectWithIgnoredProperty
    {
        static readonly DateTime s_unixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [JsonProperty("exp")]
        int _expiration
        {
            get => (int)(Expiration - s_unixEpoch).TotalSeconds;
            set => Expiration = s_unixEpoch.AddSeconds(value);
        }

        public bool Active { get; set; }

        [JsonIgnore]
        public DateTime Expiration { get; set; }
    }
}