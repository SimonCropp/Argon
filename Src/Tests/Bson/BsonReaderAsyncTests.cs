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

#pragma warning disable 618
using Xunit;

namespace Argon.Tests.Bson;

public class BsonReaderAsyncTests : TestFixtureBase
{
    const char Euro = '\u20ac';

    [Fact]
    public async Task ReadSingleObjectAsync()
    {
        var data = HexToBytes("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00");
        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("Blah", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(1L, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadGuid_TextAsync()
    {
        var data = HexToBytes("31-00-00-00-02-30-00-25-00-00-00-64-38-32-31-65-65-64-37-2D-34-62-35-63-2D-34-33-63-39-2D-38-61-63-32-2D-36-39-32-38-65-35-37-39-62-37-30-35-00-00");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("d821eed7-4b5c-43c9-8ac2-6928e579b705", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);

        ms = new MemoryStream(data);
        reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;

        var serializer = new JsonSerializer();
        var l = serializer.Deserialize<IList<Guid>>(reader);

        Assert.Equal(1, l.Count);
        Assert.Equal(new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705"), l[0]);
    }

    [Fact]
    public async Task ReadGuid_BytesAsync()
    {
        var data = HexToBytes("1D-00-00-00-05-30-00-10-00-00-00-04-D7-EE-21-D8-5C-4B-C9-43-8A-C2-69-28-E5-79-B7-05-00");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        var g = new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705");

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(g, reader.Value);
        Assert.Equal(typeof(Guid), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);

        ms = new MemoryStream(data);
        reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;

        var serializer = new JsonSerializer();
        var l = serializer.Deserialize<IList<Guid>>(reader);

        Assert.Equal(1, l.Count);
        Assert.Equal(g, l[0]);
    }

    [Fact]
    public async Task ReadDoubleAsync()
    {
        var data = HexToBytes("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(99.99d, reader.Value);
        Assert.Equal(typeof(double), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadDouble_DecimalAsync()
    {
        var data = HexToBytes("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);
        reader.FloatParseHandling = FloatParseHandling.Decimal;
        reader.ReadRootValueAsArray = true;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(99.99m, reader.Value);
        Assert.Equal(typeof(decimal), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadValuesAsync()
    {
        var data = HexToBytes("8C-00-00-00-12-30-00-FF-FF-FF-FF-FF-FF-FF-7F-12-31-00-FF-FF-FF-FF-FF-FF-FF-7F-10-32-00-FF-FF-FF-7F-10-33-00-FF-FF-FF-7F-10-34-00-FF-00-00-00-10-35-00-7F-00-00-00-02-36-00-02-00-00-00-61-00-01-37-00-00-00-00-00-00-00-F0-45-01-38-00-FF-FF-FF-FF-FF-FF-EF-7F-01-39-00-00-00-00-E0-FF-FF-EF-47-08-31-30-00-01-05-31-31-00-05-00-00-00-02-00-01-02-03-04-09-31-32-00-40-C5-E2-BA-E3-00-00-00-09-31-33-00-40-C5-E2-BA-E3-00-00-00-00");
        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);
        reader.JsonNet35BinaryCompatibility = true;
        reader.ReadRootValueAsArray = true;
        reader.DateTimeKindHandling = DateTimeKind.Utc;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(long.MaxValue, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(long.MaxValue, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal((long)int.MaxValue, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal((long)int.MaxValue, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal((long)byte.MaxValue, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal((long)sbyte.MaxValue, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("a", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal((double)decimal.MaxValue, reader.Value);
        Assert.Equal(typeof(double), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(double.MaxValue, reader.Value);
        Assert.Equal(typeof(double), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal((double)float.MaxValue, reader.Value);
        Assert.Equal(typeof(double), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.True(reader.Value);
        Assert.Equal(typeof(bool), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(new byte[] { 0, 1, 2, 3, 4 }, (byte[])reader.Value);
        Assert.Equal(typeof(byte[]), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc), reader.Value);
        Assert.Equal(typeof(DateTime), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc), reader.Value);
        Assert.Equal(typeof(DateTime), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadObjectBsonFromSiteAsync()
    {
        var data = HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("0", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("a", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("1", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("b", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("2", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("c", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadArrayBsonFromSiteAsync()
    {
        var data = HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        XUnitAssert.False(reader.ReadRootValueAsArray);
        Assert.Equal(DateTimeKind.Local, reader.DateTimeKindHandling);

        reader.ReadRootValueAsArray = true;
        reader.DateTimeKindHandling = DateTimeKind.Utc;

        XUnitAssert.True(reader.ReadRootValueAsArray);
        Assert.Equal(DateTimeKind.Utc, reader.DateTimeKindHandling);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("a", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("b", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("c", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsInt32BadStringAsync()
    {
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            var data = HexToBytes("20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00");

            var ms = new MemoryStream(data);
            var reader = new BsonReader(ms);

            XUnitAssert.False(reader.ReadRootValueAsArray);
            Assert.Equal(DateTimeKind.Local, reader.DateTimeKindHandling);

            reader.ReadRootValueAsArray = true;
            reader.DateTimeKindHandling = DateTimeKind.Utc;

            XUnitAssert.True(reader.ReadRootValueAsArray);
            Assert.Equal(DateTimeKind.Utc, reader.DateTimeKindHandling);

            Assert.True(await reader.ReadAsync());
            Assert.Equal(JsonToken.StartArray, reader.TokenType);

            await reader.ReadAsInt32Async();
        }, "Could not convert string to integer: a. Path '[0]'.");
    }

    [Fact]
    public async Task ReadBytesAsync()
    {
        var data = HexToBytes("2B-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-05-32-00-0C-00-00-00-02-48-65-6C-6C-6F-20-77-6F-72-6C-64-21-00");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms, true, DateTimeKind.Utc);
        reader.JsonNet35BinaryCompatibility = true;

        XUnitAssert.True(reader.ReadRootValueAsArray);
        Assert.Equal(DateTimeKind.Utc, reader.DateTimeKindHandling);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("a", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("b", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        var encodedStringData = await reader.ReadAsBytesAsync();
        Assert.NotNull(encodedStringData);
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(encodedStringData, reader.Value);
        Assert.Equal(typeof(byte[]), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);

        var decodedString = Encoding.UTF8.GetString(encodedStringData, 0, encodedStringData.Length);
        Assert.Equal("Hello world!", decodedString);
    }

    [Fact]
    public async Task ReadOidAsync()
    {
        var data = HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("_id", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(HexToBytes("4ABBED9D1D8B0F0218000001"), (byte[])reader.Value);
        Assert.Equal(typeof(byte[]), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("test", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("1234£56", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadNestedArrayAsync()
    {
        var hexdoc = "82-00-00-00-07-5F-69-64-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("_id", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(HexToBytes("4A-78-93-79-17-22-00-00-00-00-61-CF"), (byte[])reader.Value);
        Assert.Equal(typeof(byte[]), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("a", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        for (var i = 1; i <= 8; i++)
        {
            Assert.True(await reader.ReadAsync());
            Assert.Equal(JsonToken.Float, reader.TokenType);

            var value = i != 5
                ? Convert.ToDouble(i)
                : 5.78960446186581E+77d;

            Assert.Equal(value, reader.Value);
        }

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("b", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("test", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadRegexAsync()
    {
        var hexdoc = "15-00-00-00-0B-72-65-67-65-78-00-74-65-73-74-00-67-69-6D-00-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("regex", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(@"/test/gim", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadCodeAsync()
    {
        var hexdoc = "1A-00-00-00-0D-63-6F-64-65-00-0B-00-00-00-49-20-61-6D-20-63-6F-64-65-21-00-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("code", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(@"I am code!", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadUndefinedAsync()
    {
        var hexdoc = "10-00-00-00-06-75-6E-64-65-66-69-6E-65-64-00-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("undefined", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);
        Assert.Equal(null, reader.Value);
        Assert.Equal(null, reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadLongAsync()
    {
        var hexdoc = "13-00-00-00-12-6C-6F-6E-67-00-FF-FF-FF-FF-FF-FF-FF-7F-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("long", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(long.MaxValue, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadReferenceAsync()
    {
        var hexdoc = "1E-00-00-00-0C-6F-69-64-00-04-00-00-00-6F-69-64-00-01-02-03-04-05-06-07-08-09-0A-0B-0C-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("oid", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("$ref", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("oid", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("$id", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, (byte[])reader.Value);
        Assert.Equal(typeof(byte[]), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadCodeWScopeAsync()
    {
        var hexdoc = "75-00-00-00-0F-63-6F-64-65-57-69-74-68-53-63-6F-70-65-00-61-00-00-00-35-00-00-00-66-6F-72-20-28-69-6E-74-20-69-20-3D-20-30-3B-20-69-20-3C-20-31-30-30-30-3B-20-69-2B-2B-29-0D-0A-7B-0D-0A-20-20-61-6C-65-72-74-28-61-72-67-31-29-3B-0D-0A-7D-00-24-00-00-00-02-61-72-67-31-00-15-00-00-00-4A-73-6F-6E-2E-4E-45-54-20-69-73-20-61-77-65-73-6F-6D-65-2E-00-00-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("codeWithScope", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("$code", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("for (int i = 0; i < 1000; i++)\r\n{\r\n  alert(arg1);\r\n}", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("$scope", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("arg1", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("Json.NET is awesome.", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadEndOfStreamAsync()
    {
        var reader = new BsonReader(new MemoryStream());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadLargeStringsAsync()
    {
        var bson =
            "4E-02-00-00-02-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-22-01-00-00-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-00";

        var reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

        var largeStringBuilder = new StringBuilder();
        for (var i = 0; i < 100; i++)
        {
            if (i > 0)
            {
                largeStringBuilder.Append("-");
            }

            largeStringBuilder.Append(i.ToString(CultureInfo.InvariantCulture));
        }
        var largeString = largeStringBuilder.ToString();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal(largeString, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(largeString, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadEmptyStringsAsync()
    {
        var bson = "0C-00-00-00-02-00-01-00-00-00-00-00";

        var reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task WriteAndReadEmptyListsAndDictionariesAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("Arguments");
        await writer.WriteStartObjectAsync();
        await writer.WriteEndObjectAsync();
        await writer.WritePropertyNameAsync("List");
        await writer.WriteStartArrayAsync();
        await writer.WriteEndArrayAsync();
        await writer.WriteEndObjectAsync();

        var bson = BitConverter.ToString(ms.ToArray());

        Assert.Equal("20-00-00-00-03-41-72-67-75-6D-65-6E-74-73-00-05-00-00-00-00-04-4C-69-73-74-00-05-00-00-00-00-00", bson);

        var reader = new BsonReader(new MemoryStream(HexToBytes(bson)));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("Arguments", reader.Value.ToString());

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("List", reader.Value.ToString());

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task UnspecifiedDateTimeKindHandlingAsync()
    {
        var value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        writer.DateTimeKindHandling = DateTimeKind.Unspecified;

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("DateTime");
        await writer.WriteValueAsync(value);
        await writer.WriteEndObjectAsync();

        var bson = ms.ToArray();

        var reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Unspecified);
        var o = (JObject)JToken.ReadFrom(reader);
        Assert.Equal(value, (DateTime)o["DateTime"]);
    }

    [Fact]
    public async Task LocalDateTimeKindHandlingAsync()
    {
        var value = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("DateTime");
        await writer.WriteValueAsync(value);
        await writer.WriteEndObjectAsync();

        var bson = ms.ToArray();

        var reader = new BsonReader(new MemoryStream(bson), false, DateTimeKind.Local);
        var o = (JObject)JToken.ReadFrom(reader);
        Assert.Equal(value, (DateTime)o["DateTime"]);
    }

    async Task<string> WriteAndReadStringValueAsync(string val)
    {
        var ms = new MemoryStream();
        var bs = new BsonWriter(ms);
        await bs.WriteStartObjectAsync();
        await bs.WritePropertyNameAsync("StringValue");
        await bs.WriteValueAsync(val);
        await bs.WriteEndAsync();

        ms.Seek(0, SeekOrigin.Begin);

        var reader = new BsonReader(ms);
        // object
        await reader.ReadAsync();
        // property name
        await reader.ReadAsync();
        // string
        await reader.ReadAsync();
        return (string)reader.Value;
    }

    async Task<string> WriteAndReadStringPropertyNameAsync(string val)
    {
        var ms = new MemoryStream();
        var bs = new BsonWriter(ms);
        await bs.WriteStartObjectAsync();
        await bs.WritePropertyNameAsync(val);
        await bs.WriteValueAsync("Dummy");
        await bs.WriteEndAsync();

        ms.Seek(0, SeekOrigin.Begin);

        var reader = new BsonReader(ms);
        // object
        await reader.ReadAsync();
        // property name
        await reader.ReadAsync();
        return (string)reader.Value;
    }

    [Fact]
    public async Task TestReadLenStringValueShortTripleByteAsync()
    {
        var sb = new StringBuilder();
        //sb.Append('1',127); //first char of euro at the end of the boundry.
        //sb.Append(euro, 5);
        //sb.Append('1',128);
        sb.Append(Euro);

        var expected = sb.ToString();
        Assert.Equal(expected, await WriteAndReadStringValueAsync(expected));
    }

    [Fact]
    public async Task TestReadLenStringValueTripleByteCharBufferBoundry0Async()
    {
        var sb = new StringBuilder();
        sb.Append('1', 127); //first char of euro at the end of the boundry.
        sb.Append(Euro, 5);
        sb.Append('1', 128);
        sb.Append(Euro);

        var expected = sb.ToString();
        Assert.Equal(expected, await  WriteAndReadStringValueAsync(expected));
    }

    [Fact]
    public async Task TestReadLenStringValueTripleByteCharBufferBoundry1Async()
    {
        var sb = new StringBuilder();
        sb.Append('1', 126);
        sb.Append(Euro, 5); //middle char of euro at the end of the boundry.
        sb.Append('1', 128);
        sb.Append(Euro);

        var expected = sb.ToString();
        var result = await  WriteAndReadStringValueAsync(expected);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TestReadLenStringValueTripleByteCharOneAsync()
    {
        var sb = new StringBuilder();
        sb.Append(Euro, 1); //Just one triple byte char in the string.

        var expected = sb.ToString();
        Assert.Equal(expected, await  WriteAndReadStringValueAsync(expected));
    }

    [Fact]
    public async Task TestReadLenStringValueTripleByteCharBufferBoundry2Async()
    {
        var sb = new StringBuilder();
        sb.Append('1', 125);
        sb.Append(Euro, 5); //last char of the eruo at the end of the boundry.
        sb.Append('1', 128);
        sb.Append(Euro);

        var expected = sb.ToString();
        Assert.Equal(expected, await  WriteAndReadStringValueAsync(expected));
    }

    [Fact]
    public async Task TestReadStringValueAsync()
    {
        var expected = "test";
        Assert.Equal(expected, await  WriteAndReadStringValueAsync(expected));
    }

    [Fact]
    public async Task TestReadStringValueLongAsync()
    {
        var sb = new StringBuilder();
        sb.Append('t', 150);
        var expected = sb.ToString();
        Assert.Equal(expected, await  WriteAndReadStringValueAsync(expected));
    }

    [Fact]
    public async Task TestReadStringPropertyNameShortTripleByteAsync()
    {
        var sb = new StringBuilder();
        //sb.Append('1',127); //first char of euro at the end of the boundry.
        //sb.Append(euro, 5);
        //sb.Append('1',128);
        sb.Append(Euro);

        var expected = sb.ToString();
        Assert.Equal(expected, await WriteAndReadStringPropertyNameAsync(expected));
    }

    [Fact]
    public async Task TestReadStringPropertyNameTripleByteCharBufferBoundry0Async()
    {
        var sb = new StringBuilder();
        sb.Append('1', 127); //first char of euro at the end of the boundry.
        sb.Append(Euro, 5);
        sb.Append('1', 128);
        sb.Append(Euro);

        var expected = sb.ToString();
        var result = await WriteAndReadStringPropertyNameAsync(expected);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TestReadStringPropertyNameTripleByteCharBufferBoundry1Async()
    {
        var sb = new StringBuilder();
        sb.Append('1', 126);
        sb.Append(Euro, 5); //middle char of euro at the end of the boundry.
        sb.Append('1', 128);
        sb.Append(Euro);

        var expected = sb.ToString();
        Assert.Equal(expected, await WriteAndReadStringPropertyNameAsync(expected));
    }

    [Fact]
    public async Task TestReadStringPropertyNameTripleByteCharOneAsync()
    {
        var sb = new StringBuilder();
        sb.Append(Euro, 1); //Just one triple byte char in the string.

        var expected = sb.ToString();
        Assert.Equal(expected, await WriteAndReadStringPropertyNameAsync(expected));
    }

    [Fact]
    public async Task TestReadStringPropertyNameTripleByteCharBufferBoundry2Async()
    {
        var sb = new StringBuilder();
        sb.Append('1', 125);
        sb.Append(Euro, 5); //last char of the eruo at the end of the boundry.
        sb.Append('1', 128);
        sb.Append(Euro);

        var expected = sb.ToString();
        Assert.Equal(expected, await WriteAndReadStringPropertyNameAsync(expected));
    }

    [Fact]
    public async Task TestReadStringPropertyNameAsync()
    {
        var expected = "test";
        Assert.Equal(expected, await WriteAndReadStringPropertyNameAsync(expected));
    }

    [Fact]
    public async Task TestReadStringPropertyNameLongAsync()
    {
        var sb = new StringBuilder();
        sb.Append('t', 150);
        var expected = sb.ToString();
        Assert.Equal(expected, await WriteAndReadStringPropertyNameAsync(expected));
    }

    [Fact]
    public async Task ReadRegexWithOptionsAsync()
    {
        var hexdoc = "1A-00-00-00-0B-72-65-67-65-78-00-61-62-63-00-69-00-0B-74-65-73-74-00-00-00-00";

        var data = HexToBytes(hexdoc);

        var ms = new MemoryStream(data);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("/abc/i", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("//", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task CanRoundTripStackOverflowDataAsync()
    {
        var doc =
            @"{
""AboutMe"": ""<p>I'm the Director for Research and Development for <a href=\""http://www.prophoenix.com\"" rel=\""nofollow\"">ProPhoenix</a>, a public safety software company.  This position allows me to investigate new and existing technologies and incorporate them into our product line, with the end goal being to help public safety agencies to do their jobs more effeciently and safely.</p>\r\n\r\n<p>I'm an advocate for PowerShell, as I believe it encourages administrative best practices and allows developers to provide additional access to their applications, without needing to explicity write code for each administrative feature.  Part of my advocacy for PowerShell includes <a href=\""http://blog.usepowershell.com\"" rel=\""nofollow\"">my blog</a>, appearances on various podcasts, and acting as a Community Director for <a href=\""http://powershellcommunity.org\"" rel=\""nofollow\"">PowerShellCommunity.Org</a></p>\r\n\r\n<p>I’m also a co-host of Mind of Root (a weekly audio podcast about systems administration, tech news, and topics).</p>\r\n"",
""WebsiteUrl"": ""http://blog.usepowershell.com""
}";
        var parsed = JObject.Parse(doc);
        var memoryStream = new MemoryStream();
        var bsonWriter = new BsonWriter(memoryStream);
        parsed.WriteTo(bsonWriter);
        await bsonWriter.FlushAsync();
        memoryStream.Position = 0;

        var reader = new BsonReader(memoryStream);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("AboutMe", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("<p>I'm the Director for Research and Development for <a href=\"http://www.prophoenix.com\" rel=\"nofollow\">ProPhoenix</a>, a public safety software company.  This position allows me to investigate new and existing technologies and incorporate them into our product line, with the end goal being to help public safety agencies to do their jobs more effeciently and safely.</p>\r\n\r\n<p>I'm an advocate for PowerShell, as I believe it encourages administrative best practices and allows developers to provide additional access to their applications, without needing to explicity write code for each administrative feature.  Part of my advocacy for PowerShell includes <a href=\"http://blog.usepowershell.com\" rel=\"nofollow\">my blog</a>, appearances on various podcasts, and acting as a Community Director for <a href=\"http://powershellcommunity.org\" rel=\"nofollow\">PowerShellCommunity.Org</a></p>\r\n\r\n<p>I’m also a co-host of Mind of Root (a weekly audio podcast about systems administration, tech news, and topics).</p>\r\n", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("WebsiteUrl", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("http://blog.usepowershell.com", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task MultibyteCharacterPropertyNamesAndStringsAsync()
    {
        var json = @"{
  ""ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ"": ""ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ""
}";
        var parsed = JObject.Parse(json);
        var memoryStream = new MemoryStream();
        var bsonWriter = new BsonWriter(memoryStream);
        parsed.WriteTo(bsonWriter);
        await bsonWriter.FlushAsync();
        memoryStream.Position = 0;

        var reader = new BsonReader(memoryStream);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("ΕΝΤΟΛΗ ΧΧΧ ΧΧΧΧΧΧΧΧΧ ΤΑ ΠΡΩΤΑΣΦΑΛΙΣΤΗΡΙΑ ΠΟΥ ΔΕΝ ΕΧΟΥΝ ΥΠΟΛΟΙΠΟ ΝΑ ΤΑ ΣΤΕΛΝΟΥΜΕ ΑΠΕΥΘΕΙΑΣ ΣΤΟΥΣ ΠΕΛΑΤΕΣ", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task GuidsShouldBeProperlyDeserialisedAsync()
    {
        var g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("TheGuid");
        await writer.WriteValueAsync(g);
        await writer.WriteEndObjectAsync();
        await writer.FlushAsync();

        var bytes = ms.ToArray();

        var reader = new BsonReader(new MemoryStream(bytes));
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(typeof(Guid), reader.ValueType);
        Assert.Equal(g, (Guid)reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());

        var serializer = new JsonSerializer
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Default
        };
        var b = serializer.Deserialize<ObjectTestClass>(new BsonReader(new MemoryStream(bytes)));
        Assert.Equal(typeof(Guid), b.TheGuid.GetType());
        Assert.Equal(g, (Guid)b.TheGuid);
    }

    [Fact]
    public async Task GuidsShouldBeProperlyDeserialised_AsBytesAsync()
    {
        var g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("TheGuid");
        await writer.WriteValueAsync(g);
        await writer.WriteEndObjectAsync();
        await writer.FlushAsync();

        var bytes = ms.ToArray();

        var reader = new BsonReader(new MemoryStream(bytes));
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        Assert.Equal(g.ToByteArray(), reader.ReadAsBytes());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(typeof(byte[]), reader.ValueType);
        Assert.Equal(g.ToByteArray(), (byte[])reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());

        var serializer = new JsonSerializer();
        var b = serializer.Deserialize<BytesTestClass>(new BsonReader(new MemoryStream(bytes)));
        Assert.Equal(g.ToByteArray(), b.TheGuid);
    }

    [Fact]
    public async Task GuidsShouldBeProperlyDeserialised_AsBytes_ReadAheadAsync()
    {
        var g = new Guid("822C0CE6-CC42-4753-A3C3-26F0684A4B88");

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("TheGuid");
        await writer.WriteValueAsync(g);
        await writer.WriteEndObjectAsync();
        await writer.FlushAsync();

        var bytes = ms.ToArray();

        var reader = new BsonReader(new MemoryStream(bytes));
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        Assert.Equal(g.ToByteArray(), reader.ReadAsBytes());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(typeof(byte[]), reader.ValueType);
        Assert.Equal(g.ToByteArray(), (byte[])reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());

        var serializer = new JsonSerializer
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        };
        var b = serializer.Deserialize<BytesTestClass>(new BsonReader(new MemoryStream(bytes)));
        Assert.Equal(g.ToByteArray(), b.TheGuid);
    }

    public class BytesTestClass
    {
        public byte[] TheGuid { get; set; }
    }

    public class ObjectTestClass
    {
        public object TheGuid { get; set; }
    }
}

#pragma warning restore 618