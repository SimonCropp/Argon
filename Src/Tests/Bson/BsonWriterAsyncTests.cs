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

#pragma warning disable 618
using Xunit;

namespace Argon.Tests.Bson;

public class BsonWriterAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task CloseOutputAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        Assert.True(ms.CanRead);
        await writer.CloseAsync();
        Assert.False(ms.CanRead);

        ms = new MemoryStream();
        writer = new BsonWriter(ms) { CloseOutput = false };

        Assert.True(ms.CanRead);
        await writer.CloseAsync();
        Assert.True(ms.CanRead);
    }

    [Fact]
    public async Task WriteSingleObjectAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("Blah");
        await writer.WriteValueAsync(1);
        await writer.WriteEndObjectAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00", bson);
    }

    [Fact]
    public async Task WriteValuesAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(long.MaxValue);
        await writer.WriteValueAsync((ulong)long.MaxValue);
        await writer.WriteValueAsync(int.MaxValue);
        await writer.WriteValueAsync((uint)int.MaxValue);
        await writer.WriteValueAsync(byte.MaxValue);
        await writer.WriteValueAsync(sbyte.MaxValue);
        await writer.WriteValueAsync('a');
        await writer.WriteValueAsync(decimal.MaxValue);
        await writer.WriteValueAsync(double.MaxValue);
        await writer.WriteValueAsync(float.MaxValue);
        await writer.WriteValueAsync(true);
        await writer.WriteValueAsync(new byte[] { 0, 1, 2, 3, 4 });
        await writer.WriteValueAsync(new DateTimeOffset(2000, 12, 29, 12, 30, 0, TimeSpan.Zero));
        await writer.WriteValueAsync(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc));
        await writer.WriteEndAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("8C-00-00-00-12-30-00-FF-FF-FF-FF-FF-FF-FF-7F-12-31-00-FF-FF-FF-FF-FF-FF-FF-7F-10-32-00-FF-FF-FF-7F-10-33-00-FF-FF-FF-7F-10-34-00-FF-00-00-00-10-35-00-7F-00-00-00-02-36-00-02-00-00-00-61-00-01-37-00-00-00-00-00-00-00-F0-45-01-38-00-FF-FF-FF-FF-FF-FF-EF-7F-01-39-00-00-00-00-E0-FF-FF-EF-47-08-31-30-00-01-05-31-31-00-05-00-00-00-00-00-01-02-03-04-09-31-32-00-40-C5-E2-BA-E3-00-00-00-09-31-33-00-40-C5-E2-BA-E3-00-00-00-00", bson);
    }

    [Fact]
    public async Task WriteDoubleAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(99.99d);
        await writer.WriteEndAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00", bson);
    }

    [Fact]
    public async Task WriteGuidAsync()
    {
        var g = new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705");

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(g);
        await writer.WriteEndAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("1D-00-00-00-05-30-00-10-00-00-00-04-D7-EE-21-D8-5C-4B-C9-43-8A-C2-69-28-E5-79-B7-05-00", bson);
    }

    [Fact]
    public async Task WriteArrayBsonFromSiteAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync("a");
        await writer.WriteValueAsync("b");
        await writer.WriteValueAsync("c");
        await writer.WriteEndArrayAsync();

        await writer.FlushAsync();

        ms.Seek(0, SeekOrigin.Begin);

        var expected = "20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00";
        var bson = ms.ToArray().BytesToHex();

        Assert.Equal(expected, bson);
    }

    [Fact]
    public async Task WriteBytesAsync()
    {
        var data = Encoding.UTF8.GetBytes("Hello world!");

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync("a");
        await writer.WriteValueAsync("b");
        await writer.WriteValueAsync(data);
        await writer.WriteEndArrayAsync();

        await writer.FlushAsync();

        ms.Seek(0, SeekOrigin.Begin);

        var expected = "2B-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-05-32-00-0C-00-00-00-00-48-65-6C-6C-6F-20-77-6F-72-6C-64-21-00";
        var bson = ms.ToArray().BytesToHex();

        Assert.Equal(expected, bson);

        var reader = new BsonReader(new MemoryStream(ms.ToArray()));
        reader.ReadRootValueAsArray = true;
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(data, (byte[])reader.Value);
    }

    [Fact]
    public async Task WriteNestedArrayAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        await writer.WriteStartObjectAsync();

        await writer.WritePropertyNameAsync("_id");
        await writer.WriteValueAsync("4A-78-93-79-17-22-00-00-00-00-61-CF".HexToBytes());

        await writer.WritePropertyNameAsync("a");
        await writer.WriteStartArrayAsync();
        for (var i = 1; i <= 8; i++)
        {
            var value = i != 5
                ? Convert.ToDouble(i)
                : 5.78960446186581E+77d;

            await writer.WriteValueAsync(value);
        }
        await writer.WriteEndArrayAsync();

        await writer.WritePropertyNameAsync("b");
        await writer.WriteValueAsync("test");

        await writer.WriteEndObjectAsync();

        await writer.FlushAsync();

        ms.Seek(0, SeekOrigin.Begin);

        var expected = "87-00-00-00-05-5F-69-64-00-0C-00-00-00-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";
        var bson = ms.ToArray().BytesToHex();

        Assert.Equal(expected, bson);
    }

    [Fact]
    public async Task WriteLargeStringsAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

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

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync(largeString);
        await writer.WriteValueAsync(largeString);
        await writer.WriteEndObjectAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("4E-02-00-00-02-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-22-01-00-00-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-00", bson);
    }

    [Fact]
    public async Task WriteEmptyStringsAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("");
        await writer.WriteValueAsync("");
        await writer.WriteEndObjectAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("0C-00-00-00-02-00-01-00-00-00-00-00", bson);
    }

    [Fact]
    public async Task WriteCommentAsync()
    {
        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () =>
        {
            var ms = new MemoryStream();
            var writer = new BsonWriter(ms);

            await writer.WriteStartArrayAsync();
            await writer.WriteCommentAsync("fail");
        }, "Cannot write JSON comment as BSON. Path ''.");
    }

    [Fact]
    public async Task WriteConstructorAsync()
    {
        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () =>
        {
            var ms = new MemoryStream();
            var writer = new BsonWriter(ms);

            await writer.WriteStartArrayAsync();
            await writer.WriteStartConstructorAsync("fail");
        }, "Cannot write JSON constructor as BSON. Path ''.");
    }

    [Fact]
    public async Task WriteRawAsync()
    {
        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () =>
        {
            var ms = new MemoryStream();
            var writer = new BsonWriter(ms);

            await writer.WriteStartArrayAsync();
            await writer.WriteRawAsync("fail");
        }, "Cannot write raw JSON as BSON. Path ''.");
    }

    [Fact]
    public async Task WriteRawValueAsync()
    {
        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () =>
        {
            var ms = new MemoryStream();
            var writer = new BsonWriter(ms);

            await writer.WriteStartArrayAsync();
            await writer.WriteRawValueAsync("fail");
        }, "Cannot write raw JSON as BSON. Path ''.");
    }

    [Fact]
    public async Task WriteOidAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        var oid = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("_oid");
        writer.WriteObjectId(oid);
        await writer.WriteEndObjectAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("17-00-00-00-07-5F-6F-69-64-00-01-02-03-04-05-06-07-08-09-0A-0B-0C-00", bson);

        ms.Seek(0, SeekOrigin.Begin);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(oid, (byte[])reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task WriteOidPlusContentAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("_id");
        writer.WriteObjectId("4ABBED9D1D8B0F0218000001".HexToBytes());
        await writer.WritePropertyNameAsync("test");
        await writer.WriteValueAsync("1234£56");
        await writer.WriteEndObjectAsync();

        var expected = "29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000".HexToBytes();

        Assert.Equal(expected, ms.ToArray());
    }

    [Fact]
    public async Task WriteRegexPlusContentAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("regex");
        writer.WriteRegex("abc", "i");
        await writer.WritePropertyNameAsync("test");
        writer.WriteRegex(string.Empty, null);
        await writer.WriteEndObjectAsync();

        var expected = "1A-00-00-00-0B-72-65-67-65-78-00-61-62-63-00-69-00-0B-74-65-73-74-00-00-00-00".HexToBytes();

        Assert.Equal(expected, ms.ToArray());
    }

    [Fact]
    public async Task WriteReadEmptyAndNullStringsAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync("Content!");
        await writer.WriteValueAsync("");
        await writer.WriteValueAsync((string)null);
        await writer.WriteEndArrayAsync();

        ms.Seek(0, SeekOrigin.Begin);

        var reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("Content!", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Null, reader.TokenType);
        Assert.Equal(null, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task WriteDateTimesAsync()
    {
        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);
        writer.DateTimeKindHandling = DateTimeKind.Unspecified;

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc));
        await writer.WriteValueAsync(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Local));
        await writer.WriteValueAsync(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Unspecified));
        await writer.WriteEndArrayAsync();

        ms.Seek(0, SeekOrigin.Begin);

        var reader = new BsonReader(ms);
        reader.ReadRootValueAsArray = true;
        reader.DateTimeKindHandling = DateTimeKind.Utc;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task WriteValueOutsideOfObjectOrArrayAsync()
    {
        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () =>
        {
            var stream = new MemoryStream();

            using (var writer = new BsonWriter(stream))
            {
                await writer.WriteValueAsync("test");
                await writer.FlushAsync();
            }
        }, "Error writing String value. BSON must start with an Object or Array. Path ''.");
    }

    [Fact]
    public async Task DateTimeZoneHandlingAsync()
    {
        var ms = new MemoryStream();
        JsonWriter writer = new BsonWriter(ms)
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));
        await writer.WriteEndArrayAsync();

        Assert.Equal("10-00-00-00-09-30-00-C8-88-07-6B-DC-00-00-00-00", BitConverter.ToString(ms.ToArray()));
    }

    [Fact]
    public async Task WriteBigIntegerAsync()
    {
        var i = BigInteger.Parse("1999999999999999999999999999999999999999999999999999999999990");

        var ms = new MemoryStream();
        var writer = new BsonWriter(ms);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("Blah");
        await writer.WriteValueAsync(i);
        await writer.WriteEndObjectAsync();

        var bson = ms.ToArray().BytesToHex();
        Assert.Equal("2A-00-00-00-05-42-6C-61-68-00-1A-00-00-00-00-F6-FF-FF-FF-FF-FF-FF-1F-B2-21-CB-28-59-84-C4-AE-03-8A-44-34-2F-4C-4E-9E-3E-01-00", bson);

        ms.Seek(0, SeekOrigin.Begin);
        var reader = new BsonReader(ms);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.Equal(new byte[] { 246, 255, 255, 255, 255, 255, 255, 31, 178, 33, 203, 40, 89, 132, 196, 174, 3, 138, 68, 52, 47, 76, 78, 158, 62, 1 }, (byte[])reader.Value);
        Assert.Equal(i, new BigInteger((byte[])reader.Value));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }
}

#pragma warning restore 618