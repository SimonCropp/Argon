// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

namespace Argon.Tests;

public class JsonTextWriterAsyncTests : TestFixtureBase
{
    public class LazyStringWriter : StringWriter
    {
        public LazyStringWriter(IFormatProvider formatProvider) : base(formatProvider)
        {
        }

        public override Task FlushAsync() =>
            DoDelay(base.FlushAsync());

        public override Task WriteAsync(char value) =>
            DoDelay(base.WriteAsync(value));

        public override Task WriteAsync(char[] buffer, int index, int count) =>
            DoDelay(base.WriteAsync(buffer, index, count));

        public override Task WriteAsync(string value) =>
            DoDelay(base.WriteAsync(value));

        public override Task WriteLineAsync() =>
            DoDelay(base.WriteLineAsync());

        public override Task WriteLineAsync(char value) =>
            DoDelay(base.WriteLineAsync(value));

        public override Task WriteLineAsync(char[] buffer, int index, int count) =>
            DoDelay(base.WriteLineAsync(buffer, index, count));

        public override Task WriteLineAsync(string value) =>
            DoDelay(base.WriteLineAsync(value));

        static async Task DoDelay(Task t)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.01));
            await t;
        }
    }

    [Fact]
    public async Task WriteLazy()
    {
        var stringWriter = new LazyStringWriter(CultureInfo.InvariantCulture);

        using (var writer = new JsonTextWriter(stringWriter))
        {
            writer.Indentation = 4;
            writer.Formatting = Formatting.Indented;

            await writer.WriteStartObjectAsync();

            await writer.WritePropertyNameAsync("PropByte");
            await writer.WriteValueAsync((byte) 1);

            await writer.WritePropertyNameAsync("PropSByte");
            await writer.WriteValueAsync((sbyte) 2);

            await writer.WritePropertyNameAsync("PropShort");
            await writer.WriteValueAsync((short) 3);

            await writer.WritePropertyNameAsync("PropUInt");
            await writer.WriteValueAsync((uint) 4);

            await writer.WritePropertyNameAsync("PropUShort");
            await writer.WriteValueAsync((ushort) 5);

            await writer.WritePropertyNameAsync("PropUri");
            await writer.WriteValueAsync(new Uri("http://localhost/"));

            await writer.WritePropertyNameAsync("PropRaw");
            await writer.WriteRawValueAsync("'raw string'");

            await writer.WritePropertyNameAsync("PropObjectNull");
            await writer.WriteValueAsync((object) null);

            await writer.WritePropertyNameAsync("PropObjectBigInteger");
            await writer.WriteValueAsync(BigInteger.Parse("123456789012345678901234567890"));

            await writer.WritePropertyNameAsync("PropUndefined");
            await writer.WriteUndefinedAsync();

            await writer.WritePropertyNameAsync(@"PropEscaped ""name""", true);
            await writer.WriteNullAsync();

            await writer.WritePropertyNameAsync(@"PropUnescaped", false);
            await writer.WriteNullAsync();

            await writer.WritePropertyNameAsync("PropArray");
            await writer.WriteStartArrayAsync();

            await writer.WriteValueAsync("string!");

            await writer.WriteEndArrayAsync();

            await writer.WritePropertyNameAsync("PropNested");
            await writer.WriteStartArrayAsync();
            await writer.WriteStartArrayAsync();
            await writer.WriteStartArrayAsync();
            await writer.WriteStartArrayAsync();
            await writer.WriteStartArrayAsync();

            await writer.WriteEndArrayAsync();
            await writer.WriteEndArrayAsync();
            await writer.WriteEndArrayAsync();
            await writer.WriteEndArrayAsync();
            await writer.WriteEndArrayAsync();

            await writer.WriteEndObjectAsync();
        }

        XUnitAssert.AreEqualNormalized(@"{
    ""PropByte"": 1,
    ""PropSByte"": 2,
    ""PropShort"": 3,
    ""PropUInt"": 4,
    ""PropUShort"": 5,
    ""PropUri"": ""http://localhost/"",
    ""PropRaw"": 'raw string',
    ""PropObjectNull"": null,
    ""PropObjectBigInteger"": 123456789012345678901234567890,
    ""PropUndefined"": undefined,
    ""PropEscaped \""name\"""": null,
    ""PropUnescaped"": null,
    ""PropArray"": [
        ""string!""
    ],
    ""PropNested"": [
        [
            [
                [
                    []
                ]
            ]
        ]
    ]
}", stringWriter.ToString());
    }

    [Fact]
    public async Task WriteLazy_Property()
    {
        var stringWriter = new LazyStringWriter(CultureInfo.InvariantCulture);

        using (var writer = new JsonTextWriter(stringWriter))
        {
            writer.Indentation = 4;
            writer.Formatting = Formatting.Indented;

            await writer.WriteStartArrayAsync();

            await writer.WriteStartObjectAsync();

            await writer.WritePropertyNameAsync("IncompleteProp");

            await writer.WriteEndArrayAsync();
        }

        XUnitAssert.AreEqualNormalized(@"[
    {
        ""IncompleteProp"": null
    }
]", stringWriter.ToString());
    }

    [Fact]
    public async Task NewLineAsync()
    {
        var ms = new MemoryStream();

        using (var streamWriter = new StreamWriter(ms, new UTF8Encoding(false)) {NewLine = "\n"})
        using (var jsonWriter = new JsonTextWriter(streamWriter)
               {
                   CloseOutput = true,
                   Indentation = 2,
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WritePropertyNameAsync("prop");
            await jsonWriter.WriteValueAsync(true);
            await jsonWriter.WriteEndObjectAsync();
        }

        var data = ms.ToArray();

        var json = Encoding.UTF8.GetString(data, 0, data.Length);

        XUnitAssert.EqualsNormalized(@"{
  ""prop"": true
}", json);
    }

    [Fact]
    public async Task QuoteNameAndStringsAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);
        var writer = new JsonTextWriter(stringWriter)
        {
            QuoteName = false
        };

        await writer.WriteStartObjectAsync();

        await writer.WritePropertyNameAsync("name");
        await writer.WriteValueAsync("value");

        await writer.WriteEndObjectAsync();
        await writer.FlushAsync();

        Assert.Equal(@"{name:""value""}", stringBuilder.ToString());
    }

    [Fact]
    public async Task QuoteValueAndStringsAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);
        var writer = new JsonTextWriter(stringWriter)
        {
            QuoteValue = false
        };

        await writer.WriteStartObjectAsync();

        await writer.WritePropertyNameAsync("name");
        await writer.WriteValueAsync("value");

        await writer.WriteEndObjectAsync();
        await writer.FlushAsync();

        Assert.Equal(@"{""name"":value}", stringBuilder.ToString());
    }

    [Fact]
    public async Task CloseOutputAsync()
    {
        var ms = new MemoryStream();
        var writer = new JsonTextWriter(new StreamWriter(ms));

        Assert.True(ms.CanRead);
        await writer.CloseAsync();
        Assert.False(ms.CanRead);

        ms = new();
        writer = new(new StreamWriter(ms)) {CloseOutput = false};

        Assert.True(ms.CanRead);
        await writer.CloseAsync();
        Assert.True(ms.CanRead);
    }

    [Fact]
    public async Task WriteIConvertableAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        await jsonWriter.WriteValueAsync(new ConvertibleInt(1));

        Assert.Equal("1", stringWriter.ToString());
    }

    [Fact]
    public async Task ValueFormattingAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync('@');
            await jsonWriter.WriteValueAsync("\r\n\t\f\b?{\\r\\n\"\'");
            await jsonWriter.WriteValueAsync(true);
            await jsonWriter.WriteValueAsync(10);
            await jsonWriter.WriteValueAsync(10.99);
            await jsonWriter.WriteValueAsync(0.99);
            await jsonWriter.WriteValueAsync(0.000000000000000001d);
            await jsonWriter.WriteValueAsync(0.000000000000000001m);
            await jsonWriter.WriteValueAsync((string) null);
            await jsonWriter.WriteValueAsync((object) null);
            await jsonWriter.WriteValueAsync("This is a string.");
            await jsonWriter.WriteNullAsync();
            await jsonWriter.WriteUndefinedAsync();
            await jsonWriter.WriteEndArrayAsync();
        }

        var expected = @"[""@"",""\r\n\t\f\b?{\\r\\n\""'"",true,10,10.99,0.99,1E-18,0.000000000000000001,null,null,""This is a string."",null,undefined]";
        var result = stringBuilder.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task NullableValueFormattingAsync()
    {
        var stringWriter = new StringWriter();
        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync((char?) null);
            await jsonWriter.WriteValueAsync((char?) 'c');
            await jsonWriter.WriteValueAsync((bool?) null);
            await jsonWriter.WriteValueAsync((bool?) true);
            await jsonWriter.WriteValueAsync((byte?) null);
            await jsonWriter.WriteValueAsync((byte?) 1);
            await jsonWriter.WriteValueAsync((sbyte?) null);
            await jsonWriter.WriteValueAsync((sbyte?) 1);
            await jsonWriter.WriteValueAsync((short?) null);
            await jsonWriter.WriteValueAsync((short?) 1);
            await jsonWriter.WriteValueAsync((ushort?) null);
            await jsonWriter.WriteValueAsync((ushort?) 1);
            await jsonWriter.WriteValueAsync((int?) null);
            await jsonWriter.WriteValueAsync((int?) 1);
            await jsonWriter.WriteValueAsync((uint?) null);
            await jsonWriter.WriteValueAsync((uint?) 1);
            await jsonWriter.WriteValueAsync((long?) null);
            await jsonWriter.WriteValueAsync((long?) 1);
            await jsonWriter.WriteValueAsync((ulong?) null);
            await jsonWriter.WriteValueAsync((ulong?) 1);
            await jsonWriter.WriteValueAsync((double?) null);
            await jsonWriter.WriteValueAsync((double?) 1.1);
            await jsonWriter.WriteValueAsync((float?) null);
            await jsonWriter.WriteValueAsync((float?) 1.1);
            await jsonWriter.WriteValueAsync((decimal?) null);
            await jsonWriter.WriteValueAsync((decimal?) 1.1m);
            await jsonWriter.WriteValueAsync((DateTime?) null);
            await jsonWriter.WriteValueAsync((DateTime?) new DateTime(ParseTests.InitialJavaScriptDateTicks, DateTimeKind.Utc));
            await jsonWriter.WriteValueAsync((DateTimeOffset?) null);
            await jsonWriter.WriteValueAsync((DateTimeOffset?) new DateTimeOffset(ParseTests.InitialJavaScriptDateTicks, TimeSpan.Zero));
            await jsonWriter.WriteEndArrayAsync();
        }

        var json = stringWriter.ToString();
        await Verify(json);
    }

    [Fact]
    public async Task WriteValueObjectWithNullableAsync()
    {
        var stringWriter = new StringWriter();
        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            char? value = 'c';

            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync((object) value);
            await jsonWriter.WriteEndArrayAsync();
        }

        var json = stringWriter.ToString();
        var expected = @"[""c""]";

        Assert.Equal(expected, json);
    }

    [Fact]
    public async Task WriteValueObjectWithUnsupportedValueAsync()
    {
        var stringWriter = new StringWriter();
        using var jsonWriter = new JsonTextWriter(stringWriter);
        await jsonWriter.WriteStartArrayAsync();
        await XUnitAssert.ThrowsAsync<JsonWriterException>(
            () => jsonWriter.WriteValueAsync(new Version(1, 1, 1, 1)),
            @"Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation. Path ''.");
    }

    [Fact]
    public async Task StringEscapingAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(@"""These pretzels are making me thirsty!""");
            await jsonWriter.WriteValueAsync("Jeff's house was burninated.");
            await jsonWriter.WriteValueAsync("1. You don't talk about fight club.\r\n2. You don't talk about fight club.");
            await jsonWriter.WriteValueAsync("35% of\t statistics\n are made\r up.");
            await jsonWriter.WriteEndArrayAsync();
        }

        var expected = @"[""\""These pretzels are making me thirsty!\"""",""Jeff's house was burninated."",""1. You don't talk about fight club.\r\n2. You don't talk about fight club."",""35% of\t statistics\n are made\r up.""]";
        var result = stringBuilder.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task WriteEndAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WritePropertyNameAsync("CPU");
            await jsonWriter.WriteValueAsync("Intel");
            await jsonWriter.WritePropertyNameAsync("PSU");
            await jsonWriter.WriteValueAsync("500W");
            await jsonWriter.WritePropertyNameAsync("Drives");
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync("DVD read/writer");
            await jsonWriter.WriteCommentAsync("(broken)");
            await jsonWriter.WriteValueAsync("500 gigabyte hard drive");
            await jsonWriter.WriteValueAsync("200 gigabyte hard drive");
            await jsonWriter.WriteEndObjectAsync();
            Assert.Equal(WriteState.Start, jsonWriter.WriteState);
        }

        var expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabyte hard drive""
  ]
}";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task CloseWithRemainingContentAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WritePropertyNameAsync("CPU");
            await jsonWriter.WriteValueAsync("Intel");
            await jsonWriter.WritePropertyNameAsync("PSU");
            await jsonWriter.WriteValueAsync("500W");
            await jsonWriter.WritePropertyNameAsync("Drives");
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync("DVD read/writer");
            await jsonWriter.WriteCommentAsync("(broken)");
            await jsonWriter.WriteValueAsync("500 gigabyte hard drive");
            await jsonWriter.WriteValueAsync("200 gigabyte hard drive");
            await jsonWriter.CloseAsync();
        }

        var expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabyte hard drive""
  ]
}";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task IndentingAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WritePropertyNameAsync("CPU");
            await jsonWriter.WriteValueAsync("Intel");
            await jsonWriter.WritePropertyNameAsync("PSU");
            await jsonWriter.WriteValueAsync("500W");
            await jsonWriter.WritePropertyNameAsync("Drives");
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync("DVD read/writer");
            await jsonWriter.WriteCommentAsync("(broken)");
            await jsonWriter.WriteValueAsync("500 gigabyte hard drive");
            await jsonWriter.WriteValueAsync("200 gigabyte hard drive");
            await jsonWriter.WriteEndAsync();
            await jsonWriter.WriteEndObjectAsync();
            Assert.Equal(WriteState.Start, jsonWriter.WriteState);
        }

        // {
        //   "CPU": "Intel",
        //   "PSU": "500W",
        //   "Drives": [
        //     "DVD read/writer"
        //     /*(broken)*/,
        //     "500 gigabyte hard drive",
        //     "200 gigabyte hard drive"
        //   ]
        // }

        var expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabyte hard drive""
  ]
}";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task StateAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using var jsonWriter = new JsonTextWriter(stringWriter);
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);

        await jsonWriter.WriteStartObjectAsync();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);
        Assert.Equal("", jsonWriter.Path);

        await jsonWriter.WritePropertyNameAsync("CPU");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);
        Assert.Equal("CPU", jsonWriter.Path);

        await jsonWriter.WriteValueAsync("Intel");
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);
        Assert.Equal("CPU", jsonWriter.Path);

        await jsonWriter.WritePropertyNameAsync("Drives");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);
        Assert.Equal("Drives", jsonWriter.Path);

        await jsonWriter.WriteStartArrayAsync();
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        await jsonWriter.WriteValueAsync("DVD read/writer");
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);
        Assert.Equal("Drives[0]", jsonWriter.Path);

        await jsonWriter.WriteEndAsync();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);
        Assert.Equal("Drives", jsonWriter.Path);

        await jsonWriter.WriteEndObjectAsync();
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);
        Assert.Equal("", jsonWriter.Path);
    }

    [Fact]
    public async Task FloatingPointNonFiniteNumbers_SymbolAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented,
                   FloatFormatHandling = FloatFormatHandling.Symbol
               })
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(double.NaN);
            await jsonWriter.WriteValueAsync(double.PositiveInfinity);
            await jsonWriter.WriteValueAsync(double.NegativeInfinity);
            await jsonWriter.WriteValueAsync(float.NaN);
            await jsonWriter.WriteValueAsync(float.PositiveInfinity);
            await jsonWriter.WriteValueAsync(float.NegativeInfinity);
            await jsonWriter.WriteEndArrayAsync();

            await jsonWriter.FlushAsync();
        }

        var expected = @"[
  NaN,
  Infinity,
  -Infinity,
  NaN,
  Infinity,
  -Infinity
]";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task FloatingPointNonFiniteNumbers_ZeroAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented,
                   FloatFormatHandling = FloatFormatHandling.DefaultValue
               })
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(double.NaN);
            await jsonWriter.WriteValueAsync(double.PositiveInfinity);
            await jsonWriter.WriteValueAsync(double.NegativeInfinity);
            await jsonWriter.WriteValueAsync(float.NaN);
            await jsonWriter.WriteValueAsync(float.PositiveInfinity);
            await jsonWriter.WriteValueAsync(float.NegativeInfinity);
            await jsonWriter.WriteValueAsync((double?) double.NaN);
            await jsonWriter.WriteValueAsync((double?) double.PositiveInfinity);
            await jsonWriter.WriteValueAsync((double?) double.NegativeInfinity);
            await jsonWriter.WriteValueAsync((float?) float.NaN);
            await jsonWriter.WriteValueAsync((float?) float.PositiveInfinity);
            await jsonWriter.WriteValueAsync((float?) float.NegativeInfinity);
            await jsonWriter.WriteEndArrayAsync();

            await jsonWriter.FlushAsync();
        }

        var expected = @"[
  0.0,
  0.0,
  0.0,
  0.0,
  0.0,
  0.0,
  null,
  null,
  null,
  null,
  null,
  null
]";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task FloatingPointNonFiniteNumbers_StringAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented,
                   FloatFormatHandling = FloatFormatHandling.String
               })
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(double.NaN);
            await jsonWriter.WriteValueAsync(double.PositiveInfinity);
            await jsonWriter.WriteValueAsync(double.NegativeInfinity);
            await jsonWriter.WriteValueAsync(float.NaN);
            await jsonWriter.WriteValueAsync(float.PositiveInfinity);
            await jsonWriter.WriteValueAsync(float.NegativeInfinity);
            await jsonWriter.WriteEndArrayAsync();

            await jsonWriter.FlushAsync();
        }

        var expected = @"[
  ""NaN"",
  ""Infinity"",
  ""-Infinity"",
  ""NaN"",
  ""Infinity"",
  ""-Infinity""
]";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task FloatingPointNonFiniteNumbers_QuoteCharAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented,
                   FloatFormatHandling = FloatFormatHandling.String,
                   QuoteChar = '\''
               })
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(double.NaN);
            await jsonWriter.WriteValueAsync(double.PositiveInfinity);
            await jsonWriter.WriteValueAsync(double.NegativeInfinity);
            await jsonWriter.WriteValueAsync(float.NaN);
            await jsonWriter.WriteValueAsync(float.PositiveInfinity);
            await jsonWriter.WriteValueAsync(float.NegativeInfinity);
            await jsonWriter.WriteEndArrayAsync();

            await jsonWriter.FlushAsync();
        }

        var expected = @"[
  'NaN',
  'Infinity',
  '-Infinity',
  'NaN',
  'Infinity',
  '-Infinity'
]";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task WriteRawInStartAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented,
                   FloatFormatHandling = FloatFormatHandling.Symbol
               })
        {
            await jsonWriter.WriteRawAsync("[1,2,3,4,5]");
            await jsonWriter.WriteWhitespaceAsync("  ");
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(double.NaN);
            await jsonWriter.WriteEndArrayAsync();
        }

        var expected = @"[1,2,3,4,5]  [
  NaN
]";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task WriteRawInArrayAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented,
                   FloatFormatHandling = FloatFormatHandling.Symbol
               })
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(double.NaN);
            await jsonWriter.WriteRawAsync(",[1,2,3,4,5]");
            await jsonWriter.WriteRawAsync(",[1,2,3,4,5]");
            await jsonWriter.WriteValueAsync(float.NaN);
            await jsonWriter.WriteEndArrayAsync();
        }

        var expected = @"[
  NaN,[1,2,3,4,5],[1,2,3,4,5],
  NaN
]";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task WriteRawInObjectAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WriteRawAsync(@"""PropertyName"":[1,2,3,4,5]");
            await jsonWriter.WriteEndAsync();
        }

        var expected = @"{""PropertyName"":[1,2,3,4,5]}";
        var result = stringBuilder.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task WriteTokenAsync()
    {
        var cancel = CancellationToken.None;
        var reader = new JsonTextReader(new StringReader("[1,2,3,4,5]"));
        reader.Read();
        reader.Read();

        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        await jsonWriter.WriteTokenAsync(reader, cancel);

        Assert.Equal("1", stringWriter.ToString());
    }

    [Fact]
    public async Task WriteRawValueAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            var i = 0;
            var rawJson = "[1,2]";

            await jsonWriter.WriteStartObjectAsync();

            while (i < 3)
            {
                await jsonWriter.WritePropertyNameAsync($"d{i}");
                await jsonWriter.WriteRawValueAsync(rawJson);

                i++;
            }

            await jsonWriter.WriteEndObjectAsync();
        }

        Assert.Equal(@"{""d0"":[1,2],""d1"":[1,2],""d2"":[1,2]}", stringBuilder.ToString());
    }

    [Fact]
    public async Task WriteFloatingPointNumberAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

            await jsonWriter.WriteStartArrayAsync();

            await jsonWriter.WriteValueAsync(0.0);
            await jsonWriter.WriteValueAsync(0f);
            await jsonWriter.WriteValueAsync(0.1);
            await jsonWriter.WriteValueAsync(1.0);
            await jsonWriter.WriteValueAsync(1.000001);
            await jsonWriter.WriteValueAsync(0.000001);
            await jsonWriter.WriteValueAsync(double.Epsilon);
            await jsonWriter.WriteValueAsync(double.PositiveInfinity);
            await jsonWriter.WriteValueAsync(double.NegativeInfinity);
            await jsonWriter.WriteValueAsync(double.NaN);
            await jsonWriter.WriteValueAsync(double.MaxValue);
            await jsonWriter.WriteValueAsync(double.MinValue);
            await jsonWriter.WriteValueAsync(float.PositiveInfinity);
            await jsonWriter.WriteValueAsync(float.NegativeInfinity);
            await jsonWriter.WriteValueAsync(float.NaN);

            await jsonWriter.WriteEndArrayAsync();
        }

#if NET5_0_OR_GREATER
        Assert.Equal(@"[0.0,0.0,0.1,1.0,1.000001,1E-06,5E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN]", stringBuilder.ToString());
#else
        Assert.Equal(@"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN]", stringBuilder.ToString());
#endif
    }

    [Fact]
    public async Task WriteIntegerNumberAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteStartArrayAsync();

            await jsonWriter.WriteValueAsync(int.MaxValue);
            await jsonWriter.WriteValueAsync(int.MinValue);
            await jsonWriter.WriteValueAsync(0);
            await jsonWriter.WriteValueAsync(-0);
            await jsonWriter.WriteValueAsync(9L);
            await jsonWriter.WriteValueAsync(9UL);
            await jsonWriter.WriteValueAsync(long.MaxValue);
            await jsonWriter.WriteValueAsync(long.MinValue);
            await jsonWriter.WriteValueAsync(ulong.MaxValue);
            await jsonWriter.WriteValueAsync(ulong.MinValue);

            await jsonWriter.WriteEndArrayAsync();
        }

        Console.WriteLine(stringBuilder.ToString());

        XUnitAssert.AreEqualNormalized(@"[
  2147483647,
  -2147483648,
  0,
  0,
  9,
  9,
  9223372036854775807,
  -9223372036854775808,
  18446744073709551615,
  0
]", stringBuilder.ToString());
    }

    [Fact]
    public async Task WriteTokenDirectAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            await jsonWriter.WriteTokenAsync(JsonToken.StartArray);
            await jsonWriter.WriteTokenAsync(JsonToken.Integer, 1);
            await jsonWriter.WriteTokenAsync(JsonToken.StartObject);
            await jsonWriter.WriteTokenAsync(JsonToken.PropertyName, "string");
            await jsonWriter.WriteTokenAsync(JsonToken.Integer, int.MaxValue);
            await jsonWriter.WriteTokenAsync(JsonToken.EndObject);
            await jsonWriter.WriteTokenAsync(JsonToken.EndArray);
        }

        Assert.Equal(@"[1,{""string"":2147483647}]", stringBuilder.ToString());
    }

    [Fact]
    public async Task WriteTokenDirect_BadValueAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using var jsonWriter = new JsonTextWriter(stringWriter);
        await jsonWriter.WriteTokenAsync(JsonToken.StartArray);

        await XUnitAssert.ThrowsAsync<FormatException>(
            () => jsonWriter.WriteTokenAsync(JsonToken.Integer, "three"),
            "Input string was not in a correct format.");
    }

    [Fact]
    public async Task TokenTypeOutOfRangeAsync()
    {
        using var jsonWriter = new JsonTextWriter(new StringWriter());
        var ex = await XUnitAssert.ThrowsAsync<ArgumentOutOfRangeException>(() => jsonWriter.WriteTokenAsync((JsonToken) int.MinValue));
        Assert.Equal("token", ex.ParamName);

        ex = await XUnitAssert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => jsonWriter.WriteTokenAsync((JsonToken) int.MinValue,
                "test"));
        Assert.Equal("token", ex.ParamName);
    }

    [Fact]
    public async Task BadWriteEndArrayAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using var jsonWriter = new JsonTextWriter(stringWriter);
        await jsonWriter.WriteStartArrayAsync();

        await jsonWriter.WriteValueAsync(0.0);

        await jsonWriter.WriteEndArrayAsync();
        await XUnitAssert.ThrowsAsync<JsonWriterException>(
            () => jsonWriter.WriteEndArrayAsync(),
            "No token to close. Path ''.");
    }

    [Fact]
    public async Task IndentationAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented,
                   FloatFormatHandling = FloatFormatHandling.Symbol
               })
        {
            jsonWriter.Indentation = 5;
            jsonWriter.IndentChar = '_';
            jsonWriter.QuoteChar = '\'';

            await jsonWriter.WriteStartObjectAsync();

            await jsonWriter.WritePropertyNameAsync("propertyName");
            await jsonWriter.WriteValueAsync(double.NaN);

            jsonWriter.IndentChar = '?';
            jsonWriter.Indentation = 6;

            await jsonWriter.WritePropertyNameAsync("prop2");
            await jsonWriter.WriteValueAsync(123);

            await jsonWriter.WriteEndObjectAsync();
        }

        var expected = @"{
_____'propertyName': NaN,
??????'prop2': 123
}";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task WriteSingleBytesAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        var text = "Hello world.";
        var data = Encoding.UTF8.GetBytes(text);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteValueAsync(data);
        }

        var expected = @"""SGVsbG8gd29ybGQu""";
        var result = stringBuilder.ToString();

        Assert.Equal(expected, result);

        var d2 = Convert.FromBase64String(result.Trim('"'));

        Assert.Equal(text, Encoding.UTF8.GetString(d2, 0, d2.Length));
    }

    [Fact]
    public async Task WriteBytesInArrayAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        var text = "Hello world.";
        var data = Encoding.UTF8.GetBytes(text);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            Assert.Equal(Formatting.Indented, jsonWriter.Formatting);

            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(data);
            await jsonWriter.WriteValueAsync(data);
            await jsonWriter.WriteValueAsync((object) data);
            await jsonWriter.WriteValueAsync((byte[]) null);
            await jsonWriter.WriteValueAsync((Uri) null);
            await jsonWriter.WriteEndArrayAsync();
        }

        var expected = @"[
  ""SGVsbG8gd29ybGQu"",
  ""SGVsbG8gd29ybGQu"",
  ""SGVsbG8gd29ybGQu"",
  null,
  null
]";
        var result = stringBuilder.ToString();

        XUnitAssert.AreEqualNormalized(expected, result);
    }

    [Fact]
    public async Task DateTimeZoneHandlingAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter)
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        await jsonWriter.WriteValueAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

        Assert.Equal(@"""2000-01-01T01:01:01Z""", stringWriter.ToString());
    }

    [Fact]
    public async Task HtmlEscapeHandlingAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter)
        {
            EscapeHandling = EscapeHandling.EscapeHtml
        };

        var script = @"<script type=""text/javascript"">alert('hi');</script>";

        await jsonWriter.WriteValueAsync(script);

        var json = stringWriter.ToString();

        Assert.Equal(@"""\u003cscript type=\u0022text/javascript\u0022\u003ealert(\u0027hi\u0027);\u003c/script\u003e""", json);

        var reader = new JsonTextReader(new StringReader(json));

        Assert.Equal(script, reader.ReadAsString());
    }

    [Fact]
    public async Task NonAsciiEscapeHandlingAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter)
        {
            EscapeHandling = EscapeHandling.EscapeNonAscii
        };

        var unicode = "\u5f20";

        await jsonWriter.WriteValueAsync(unicode);

        var json = stringWriter.ToString();

        Assert.Equal(8, json.Length);
        Assert.Equal(@"""\u5f20""", json);

        var reader = new JsonTextReader(new StringReader(json));

        Assert.Equal(unicode, reader.ReadAsString());

        stringWriter = new();
        jsonWriter = new(stringWriter)
        {
            EscapeHandling = EscapeHandling.Default
        };

        await jsonWriter.WriteValueAsync(unicode);

        json = stringWriter.ToString();

        Assert.Equal(3, json.Length);
        Assert.Equal("\"\u5f20\"", json);
    }

    [Fact]
    public async Task NoEscapeHandlingAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter)
        {
            EscapeHandling = EscapeHandling.None
        };

        var unicode = "\u5f20";

        await jsonWriter.WriteValueAsync(unicode);

        var json = stringWriter.ToString();

        Assert.Equal(3, json.Length);
        Assert.Equal(@"""张""", json);

        var reader = new JsonTextReader(new StringReader(json));

        Assert.Equal(unicode, reader.ReadAsString());

        stringWriter = new();
        jsonWriter = new(stringWriter)
        {
            EscapeHandling = EscapeHandling.Default
        };

        await jsonWriter.WriteValueAsync(unicode);

        json = stringWriter.ToString();

        Assert.Equal(3, json.Length);
        Assert.Equal("\"\u5f20\"", json);
    }

    [Fact]
    public async Task WriteEndOnPropertyAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.QuoteChar = '\'';

        await jsonWriter.WriteStartObjectAsync();
        await jsonWriter.WritePropertyNameAsync("Blah");
        await jsonWriter.WriteEndAsync();

        Assert.Equal("{'Blah':null}", stringWriter.ToString());
    }

    [Fact]
    public async Task QuoteCharAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter)
        {
            Formatting = Formatting.Indented,
            QuoteChar = '\''
        };

        await jsonWriter.WriteStartArrayAsync();

        await jsonWriter.WriteValueAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
        await jsonWriter.WriteValueAsync(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

        jsonWriter.DateFormatString = "yyyy gg";
        await jsonWriter.WriteValueAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
        await jsonWriter.WriteValueAsync(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

        await jsonWriter.WriteValueAsync(new byte[] {1, 2, 3});
        await jsonWriter.WriteValueAsync(TimeSpan.Zero);
        await jsonWriter.WriteValueAsync(new Uri("http://www.google.com/"));
        await jsonWriter.WriteValueAsync(Guid.Empty);

        await jsonWriter.WriteEndAsync();

        XUnitAssert.AreEqualNormalized(@"[
  '2000-01-01T01:01:01Z',
  '2000-01-01T01:01:01+00:00',
  '2000 A.D.',
  '2000 A.D.',
  'AQID',
  '00:00:00',
  'http://www.google.com/',
  '00000000-0000-0000-0000-000000000000'
]", stringWriter.ToString());
    }

    [Fact]
    public async Task CultureAsync()
    {
        var culture = new CultureInfo("en-NZ")
        {
            DateTimeFormat =
            {
                AMDesignator = "a.m.",
                PMDesignator = "p.m."
            }
        };

        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter)
        {
            QuoteChar = '\'',
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy tt",
            Culture = culture
        };

        await jsonWriter.WriteStartArrayAsync();

        await jsonWriter.WriteValueAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
        await jsonWriter.WriteValueAsync(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

        await jsonWriter.WriteEndAsync();

        XUnitAssert.AreEqualNormalized(@"[
  '2000 a.m.',
  '2000 a.m.'
]", stringWriter.ToString());
    }

    [Fact]
    public async Task CompareNewStringEscapingWithOldAsync()
    {
        var c = (char) 0;

        do
        {
            var swNew = new StringWriter();
            var jsonWriter = new JsonTextWriter(new StreamWriter(Stream.Null));
            await JavaScriptUtils.WriteEscapedJavaScriptStringAsync(swNew, c.ToString(), '"', true, JavaScriptUtils.DoubleQuoteEscapeFlags, EscapeHandling.Default, jsonWriter, null);

            var swOld = new StringWriter();
            WriteEscapedJavaScriptStringOld(swOld, c.ToString(), '"', true);

            var newText = swNew.ToString();
            var oldText = swOld.ToString();

            if (newText != oldText)
            {
                throw new($"Difference for char '{c}' (value {(int) c}). Old text: {oldText}, New text: {newText}");
            }

            c++;
        } while (c != char.MaxValue);
    }

    const string EscapedUnicodeText = "!";

    static void WriteEscapedJavaScriptStringOld(TextWriter writer, string s, char delimiter, bool appendDelimiters)
    {
        // leading delimiter
        if (appendDelimiters)
        {
            writer.Write(delimiter);
        }

        if (s != null)
        {
            char[] chars = null;
            char[] unicodeBuffer = null;
            var lastWritePosition = 0;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];

                // don't escape standard text/numbers except '\' and the text delimiter
                if (c >= ' ' && c < 128 && c != '\\' && c != delimiter)
                {
                    continue;
                }

                string escapedValue;

                switch (c)
                {
                    case '\t':
                        escapedValue = @"\t";
                        break;
                    case '\n':
                        escapedValue = @"\n";
                        break;
                    case '\r':
                        escapedValue = @"\r";
                        break;
                    case '\f':
                        escapedValue = @"\f";
                        break;
                    case '\b':
                        escapedValue = @"\b";
                        break;
                    case '\\':
                        escapedValue = @"\\";
                        break;
                    case '\u0085': // Next Line
                        escapedValue = @"\u0085";
                        break;
                    case '\u2028': // Line Separator
                        escapedValue = @"\u2028";
                        break;
                    case '\u2029': // Paragraph Separator
                        escapedValue = @"\u2029";
                        break;
                    case '\'':
                        // this charater is being used as the delimiter
                        escapedValue = @"\'";
                        break;
                    case '"':
                        // this charater is being used as the delimiter
                        escapedValue = "\\\"";
                        break;
                    default:
                        if (c <= '\u001f')
                        {
                            unicodeBuffer ??= new char[6];

                            StringUtils.ToCharAsUnicode(c, unicodeBuffer);

                            // slightly hacky but it saves multiple conditions in if test
                            escapedValue = EscapedUnicodeText;
                        }
                        else
                        {
                            escapedValue = null;
                        }

                        break;
                }

                if (escapedValue == null)
                {
                    continue;
                }

                if (i > lastWritePosition)
                {
                    chars ??= s.ToCharArray();

                    // write unchanged chars before writing escaped text
                    writer.Write(chars, lastWritePosition, i - lastWritePosition);
                }

                lastWritePosition = i + 1;
                if (string.Equals(escapedValue, EscapedUnicodeText))
                {
                    writer.Write(unicodeBuffer);
                }
                else
                {
                    writer.Write(escapedValue);
                }
            }

            if (lastWritePosition == 0)
            {
                // no escaped text, write entire string
                writer.Write(s);
            }
            else
            {
                chars ??= s.ToCharArray();

                // write remaining text
                writer.Write(chars, lastWritePosition, s.Length - lastWritePosition);
            }
        }

        // trailing delimiter
        if (appendDelimiters)
        {
            writer.Write(delimiter);
        }
    }

    [Fact]
    public async Task CustomJsonTextWriterTestsAsync()
    {
        var stringWriter = new StringWriter();
        CustomJsonTextWriter writer = new CustomAsyncJsonTextWriter(stringWriter) {Formatting = Formatting.Indented};
        await writer.WriteStartObjectAsync();
        Assert.Equal(WriteState.Object, writer.WriteState);
        await writer.WritePropertyNameAsync("Property1");
        Assert.Equal(WriteState.Property, writer.WriteState);
        Assert.Equal("Property1", writer.Path);
        await writer.WriteNullAsync();
        Assert.Equal(WriteState.Object, writer.WriteState);
        await writer.WriteEndObjectAsync();
        Assert.Equal(WriteState.Start, writer.WriteState);

        XUnitAssert.AreEqualNormalized(@"{{{
  ""1ytreporP"": NULL!!!
}}}", stringWriter.ToString());
    }

    [Fact]
    public async Task QuoteDictionaryNamesAsync()
    {
        var d = new Dictionary<string, int>
        {
            {"a", 1}
        };
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };
        var serializer = JsonSerializer.Create(settings);
        using var stringWriter = new StringWriter();
        using (var writer = new JsonTextWriter(stringWriter)
               {
                   QuoteName = false
               })
        {
            serializer.Serialize(writer, d);
            await writer.CloseAsync();
        }

        XUnitAssert.AreEqualNormalized(@"{
  a: 1
}",
            stringWriter.ToString());
    }

    [Fact]
    public async Task QuoteDictionaryValuesAsync()
    {
        var d = new Dictionary<string, string>
        {
            {"a", "b"}
        };
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };
        var serializer = JsonSerializer.Create(settings);
        using var stringWriter = new StringWriter();
        using (var writer = new JsonTextWriter(stringWriter)
               {
                   QuoteValue = false
               })
        {
            serializer.Serialize(writer, d);
            await writer.CloseAsync();
        }

        XUnitAssert.AreEqualNormalized(@"{
  ""a"": b
}",
            stringWriter.ToString());
    }

    [Fact]
    public async Task WriteCommentsAsync()
    {
        var json = $@"//comment*//*hi*/
{{//comment
Name://comment
true//comment after true{StringUtils.CarriageReturn}
,//comment after comma{StringUtils.CarriageReturnLineFeed}
ExpiryDate:'2014-06-04T00:00:00Z',
        Price: 3.99,
        Sizes: //comment
[//comment

          ""Small""//comment
]//comment
}}//comment 
//comment 1 ";

        var reader = new JsonTextReader(new StringReader(json));

        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter)
        {
            Formatting = Formatting.Indented
        };

        await jsonWriter.WriteTokenAsync(reader, true);

        XUnitAssert.AreEqualNormalized(@"/*comment*//*hi*/*/{/*comment*/
  ""Name"": /*comment*/ true/*comment after true*//*comment after comma*/,
  ""ExpiryDate"": ""2014-06-04T00:00:00Z"",
  ""Price"": 3.99,
  ""Sizes"": /*comment*/ [
    /*comment*/
    ""Small""
    /*comment*/
  ]/*comment*/
}/*comment *//*comment 1 */", stringWriter.ToString());
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelled()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        var writer = new JsonTextWriter(new StreamWriter(Stream.Null));

        Assert.True(writer.CloseAsync(token).IsCanceled);
        Assert.True(writer.FlushAsync(token).IsCanceled);
        Assert.True(writer.WriteCommentAsync("test", token).IsCanceled);
        Assert.True(writer.WriteEndArrayAsync(token).IsCanceled);
        Assert.True(writer.WriteEndAsync(token).IsCanceled);
        Assert.True(writer.WriteEndObjectAsync(token).IsCanceled);
        Assert.True(writer.WriteNullAsync(token).IsCanceled);
        Assert.True(writer.WritePropertyNameAsync("test", token).IsCanceled);
        Assert.True(writer.WritePropertyNameAsync("test", false, token).IsCanceled);
        Assert.True(writer.WriteRawAsync("{}", token).IsCanceled);
        Assert.True(writer.WriteRawValueAsync("{}", token).IsCanceled);
        Assert.True(writer.WriteStartArrayAsync(token).IsCanceled);
        Assert.True(writer.WriteStartObjectAsync(token).IsCanceled);
        Assert.True(writer.WriteTokenAsync(JsonToken.Comment, token).IsCanceled);
        Assert.True(writer.WriteTokenAsync(JsonToken.Boolean, true, token).IsCanceled);
        var reader = new JsonTextReader(new StringReader("[1,2,3,4,5]"));
        Assert.True(writer.WriteTokenAsync(reader, token).IsCanceled);
        Assert.True(writer.WriteUndefinedAsync(token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(bool), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(bool?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte[]), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(char), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(char?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTime), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTime?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTimeOffset), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTimeOffset?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(decimal), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(decimal?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(double), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(double?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(float), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(float?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Guid), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Guid?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(int), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(int?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(long), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(long?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(object), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(sbyte), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(sbyte?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(short), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(short?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(TimeSpan), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(TimeSpan?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(uint), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(uint?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ulong), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ulong?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Uri), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ushort), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ushort?), token).IsCanceled);
        Assert.True(writer.WriteWhitespaceAsync(" ", token).IsCanceled);
    }

    class NoOverridesDerivedJsonTextWriter : JsonTextWriter
    {
        public NoOverridesDerivedJsonTextWriter(TextWriter textWriter) : base(textWriter)
        {
        }
    }

    class MinimalOverridesDerivedJsonWriter : JsonWriter
    {
        public override void Flush()
        {
        }
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelledOnTextWriterSubclass()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        var writer = new NoOverridesDerivedJsonTextWriter(new StreamWriter(Stream.Null));

        Assert.True(writer.CloseAsync(token).IsCanceled);
        Assert.True(writer.FlushAsync(token).IsCanceled);
        Assert.True(writer.WriteCommentAsync("test", token).IsCanceled);
        Assert.True(writer.WriteEndArrayAsync(token).IsCanceled);
        Assert.True(writer.WriteEndAsync(token).IsCanceled);
        Assert.True(writer.WriteEndObjectAsync(token).IsCanceled);
        Assert.True(writer.WriteNullAsync(token).IsCanceled);
        Assert.True(writer.WritePropertyNameAsync("test", token).IsCanceled);
        Assert.True(writer.WritePropertyNameAsync("test", false, token).IsCanceled);
        Assert.True(writer.WriteRawAsync("{}", token).IsCanceled);
        Assert.True(writer.WriteRawValueAsync("{}", token).IsCanceled);
        Assert.True(writer.WriteStartArrayAsync(token).IsCanceled);
        Assert.True(writer.WriteStartObjectAsync(token).IsCanceled);
        Assert.True(writer.WriteTokenAsync(JsonToken.Comment, token).IsCanceled);
        Assert.True(writer.WriteTokenAsync(JsonToken.Boolean, true, token).IsCanceled);
        var reader = new JsonTextReader(new StringReader("[1,2,3,4,5]"));
        Assert.True(writer.WriteTokenAsync(reader, token).IsCanceled);
        Assert.True(writer.WriteUndefinedAsync(token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(bool), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(bool?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte[]), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(char), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(char?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTime), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTime?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTimeOffset), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTimeOffset?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(decimal), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(decimal?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(double), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(double?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(float), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(float?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Guid), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Guid?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(int), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(int?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(long), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(long?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(object), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(sbyte), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(sbyte?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(short), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(short?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(TimeSpan), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(TimeSpan?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(uint), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(uint?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ulong), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ulong?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Uri), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ushort), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ushort?), token).IsCanceled);
        Assert.True(writer.WriteWhitespaceAsync(" ", token).IsCanceled);
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelledOnWriterSubclass()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        var writer = new MinimalOverridesDerivedJsonWriter();

        Assert.True(writer.CloseAsync(token).IsCanceled);
        Assert.True(writer.FlushAsync(token).IsCanceled);
        Assert.True(writer.WriteCommentAsync("test", token).IsCanceled);
        Assert.True(writer.WriteEndArrayAsync(token).IsCanceled);
        Assert.True(writer.WriteEndAsync(token).IsCanceled);
        Assert.True(writer.WriteEndObjectAsync(token).IsCanceled);
        Assert.True(writer.WriteNullAsync(token).IsCanceled);
        Assert.True(writer.WritePropertyNameAsync("test", token).IsCanceled);
        Assert.True(writer.WritePropertyNameAsync("test", false, token).IsCanceled);
        Assert.True(writer.WriteRawAsync("{}", token).IsCanceled);
        Assert.True(writer.WriteRawValueAsync("{}", token).IsCanceled);
        Assert.True(writer.WriteStartArrayAsync(token).IsCanceled);
        Assert.True(writer.WriteStartObjectAsync(token).IsCanceled);
        Assert.True(writer.WriteTokenAsync(JsonToken.Comment, token).IsCanceled);
        Assert.True(writer.WriteTokenAsync(JsonToken.Boolean, true, token).IsCanceled);
        var reader = new JsonTextReader(new StringReader("[1,2,3,4,5]"));
        Assert.True(writer.WriteTokenAsync(reader, token).IsCanceled);
        Assert.True(writer.WriteUndefinedAsync(token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(bool), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(bool?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(byte[]), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(char), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(char?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTime), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTime?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTimeOffset), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(DateTimeOffset?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(decimal), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(decimal?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(double), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(double?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(float), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(float?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Guid), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Guid?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(int), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(int?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(long), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(long?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(object), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(sbyte), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(sbyte?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(short), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(short?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(TimeSpan), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(TimeSpan?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(uint), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(uint?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ulong), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ulong?), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(Uri), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ushort), token).IsCanceled);
        Assert.True(writer.WriteValueAsync(default(ushort?), token).IsCanceled);
        Assert.True(writer.WriteWhitespaceAsync(" ", token).IsCanceled);
    }

    [Fact]
    public async Task FailureOnStartWriteProperty()
    {
        var writer = new JsonTextWriter(new ThrowingWriter(' '));
        writer.Formatting = Formatting.Indented;
        await writer.WriteStartObjectAsync();
        await XUnitAssert.ThrowsAsync<InvalidOperationException>(() => writer.WritePropertyNameAsync("aa"));
    }

    [Fact]
    public async Task FailureOnStartWriteObject()
    {
        var writer = new JsonTextWriter(new ThrowingWriter('{'));
        await XUnitAssert.ThrowsAsync<InvalidOperationException>(() => writer.WriteStartObjectAsync());
    }

    public class ThrowingWriter : TextWriter
    {
        // allergic to certain characters, this null-stream writer throws on any attempt to write them.

        char[] singleCharBuffer = new char[1];

        public ThrowingWriter(params char[] throwChars) =>
            ThrowChars = throwChars;

        public char[] ThrowChars { get; set; }

        public override Encoding Encoding => Encoding.UTF8;

        public override Task WriteAsync(char value)
        {
            singleCharBuffer[0] = value;
            return WriteAsync(singleCharBuffer, 0, 1);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            if (buffer.Skip(index).Take(count).Any(c => ThrowChars.Contains(c)))
            {
                // Pre-4.6 equivalent to .FromException()
                var tcs = new TaskCompletionSource<bool>();
                tcs.SetException(new InvalidOperationException());
                return tcs.Task;
            }

            return Task.Delay(0);
        }

        public override Task WriteAsync(string value) =>
            WriteAsync(value.ToCharArray(), 0, value.Length);

        public override void Write(char value) =>
            throw new NotImplementedException();
    }
}

public class CustomAsyncJsonTextWriter : CustomJsonTextWriter
{
    public CustomAsyncJsonTextWriter(TextWriter textWriter) : base(textWriter)
    {
    }

    public override Task WritePropertyNameAsync(string name, CancellationToken cancellation = default) =>
        WritePropertyNameAsync(name, true, cancellation);

    public override async Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellation = default)
    {
        await SetWriteStateAsync(JsonToken.PropertyName, name, cancellation);

        if (QuoteName)
        {
            await writer.WriteAsync(QuoteChar);
        }

        await writer.WriteAsync(new string(name.ToCharArray().Reverse().ToArray()));

        if (QuoteName)
        {
            await writer.WriteAsync(QuoteChar);
        }

        await writer.WriteAsync(':');
    }

    public override async Task WriteNullAsync(CancellationToken cancellation = default)
    {
        await SetWriteStateAsync(JsonToken.Null, null, cancellation);

        await writer.WriteAsync("NULL!!!");
    }

    public override async Task WriteStartObjectAsync(CancellationToken cancellation = default)
    {
        await SetWriteStateAsync(JsonToken.StartObject, null, cancellation);

        await writer.WriteAsync("{{{");
    }

    public override Task WriteEndObjectAsync(CancellationToken cancellation = default) =>
        SetWriteStateAsync(JsonToken.EndObject, null, cancellation);

    protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellation)
    {
        if (token == JsonToken.EndObject)
        {
            return writer.WriteAsync("}}}");
        }

        return base.WriteEndAsync(token, cancellation);
    }
}