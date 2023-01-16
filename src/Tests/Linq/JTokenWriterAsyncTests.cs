// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class JTokenWriterAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ValueFormattingAsync()
    {
        var data = "Hello world."u8.ToArray();

        JToken root;
        using (var jsonWriter = new JTokenWriter())
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
            await jsonWriter.WriteValueAsync("This is a string.");
            await jsonWriter.WriteNullAsync();
            await jsonWriter.WriteUndefinedAsync();
            await jsonWriter.WriteValueAsync(data);
            await jsonWriter.WriteEndArrayAsync();

            root = jsonWriter.Token;
        }

        Assert.IsType(typeof(JArray), root);
        Assert.Equal(13, root.Children().Count());
        Assert.Equal("@", (string) root[0]);
        Assert.Equal("\r\n\t\f\b?{\\r\\n\"\'", (string) root[1]);
        XUnitAssert.True((bool) root[2]);
        Assert.Equal(10, (int) root[3]);
        Assert.Equal(10.99, (double) root[4]);
        Assert.Equal(0.99, (double) root[5]);
        Assert.Equal(0.000000000000000001d, (double) root[6]);
        Assert.Equal(0.000000000000000001m, (decimal) root[7]);
        Assert.Equal(null, (string) root[8]);
        Assert.Equal("This is a string.", (string) root[9]);
        Assert.Equal(null, ((JValue) root[10]).Value);
        Assert.Equal(null, ((JValue) root[11]).Value);
        Assert.Equal(data, (byte[]) root[12]);
    }

    [Fact]
    public async Task StateAsync()
    {
        using JsonWriter jsonWriter = new JTokenWriter();
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);

        await jsonWriter.WriteStartObjectAsync();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);

        await jsonWriter.WritePropertyNameAsync("CPU");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);

        await jsonWriter.WriteValueAsync("Intel");
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);

        await jsonWriter.WritePropertyNameAsync("Drives");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);

        await jsonWriter.WriteStartArrayAsync();
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        await jsonWriter.WriteValueAsync("DVD read/writer");
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        await jsonWriter.WriteValueAsync(new BigInteger(123));
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        await jsonWriter.WriteValueAsync(Array.Empty<byte>());
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        await jsonWriter.WriteEndAsync();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);

        await jsonWriter.WriteEndObjectAsync();
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);
    }

    [Fact]
    public async Task CurrentTokenAsync()
    {
        using var jsonWriter = new JTokenWriter();
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);
        Assert.Equal(null, jsonWriter.CurrentToken);

        await jsonWriter.WriteStartObjectAsync();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);
        Assert.Equal(jsonWriter.Token, jsonWriter.CurrentToken);

        var o = (JObject) jsonWriter.Token;

        await jsonWriter.WritePropertyNameAsync("CPU");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);
        Assert.Equal(o.Property("CPU"), jsonWriter.CurrentToken);

        await jsonWriter.WriteValueAsync("Intel");
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);
        Assert.Equal(o["CPU"], jsonWriter.CurrentToken);

        await jsonWriter.WritePropertyNameAsync("Drives");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);
        Assert.Equal(o.Property("Drives"), jsonWriter.CurrentToken);

        await jsonWriter.WriteStartArrayAsync();
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);
        Assert.Equal(o["Drives"], jsonWriter.CurrentToken);

        var a = (JArray) jsonWriter.CurrentToken;

        await jsonWriter.WriteValueAsync("DVD read/writer");
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);
        Assert.Equal(a[a.Count - 1], jsonWriter.CurrentToken);

        await jsonWriter.WriteValueAsync(new BigInteger(123));
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);
        Assert.Equal(a[a.Count - 1], jsonWriter.CurrentToken);

        await jsonWriter.WriteValueAsync(Array.Empty<byte>());
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);
        Assert.Equal(a[a.Count - 1], jsonWriter.CurrentToken);

        await jsonWriter.WriteEndAsync();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);
        Assert.Equal(a, jsonWriter.CurrentToken);

        await jsonWriter.WriteEndObjectAsync();
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);
        Assert.Equal(o, jsonWriter.CurrentToken);
    }

    [Fact]
    public async Task WriteCommentAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();
        await writer.WriteCommentAsync("fail");
        await writer.WriteEndArrayAsync();

        XUnitAssert.AreEqualNormalized(@"[
  /*fail*/]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteBigIntegerAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(new BigInteger(123));
        await writer.WriteEndArrayAsync();

        var i = (JValue) writer.Token[0];

        Assert.Equal(new BigInteger(123), i.Value);
        Assert.Equal(JTokenType.Integer, i.Type);

        XUnitAssert.AreEqualNormalized(@"[
  123
]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteRawAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();
        await writer.WriteRawAsync("fail");
        await writer.WriteRawAsync("fail");
        await writer.WriteEndArrayAsync();

        // this is a bug. See non-async equivalent test.
        XUnitAssert.AreEqualNormalized(@"[
  fail,
  fail
]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteTokenWithParentAsync()
    {
        var o = new JObject
        {
            ["prop1"] = new JArray(1),
            ["prop2"] = 1
        };

        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();

        await writer.WriteTokenAsync(o.CreateReader());

        Assert.Equal(WriteState.Array, writer.WriteState);

        await writer.WriteEndArrayAsync();

        Console.WriteLine(writer.Token.ToString());

        XUnitAssert.AreEqualNormalized("""
            [
              {
                "prop1": [
                  1
                ],
                "prop2": 1
              }
            ]
            """, writer.Token.ToString());
    }

    [Fact]
    public async Task WriteTokenWithPropertyParentAsync()
    {
        var v = new JValue(1);

        var writer = new JTokenWriter();

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("Prop1");

        await writer.WriteTokenAsync(v.CreateReader());

        Assert.Equal(WriteState.Object, writer.WriteState);

        await writer.WriteEndObjectAsync();

        XUnitAssert.AreEqualNormalized(@"{
  ""Prop1"": 1
}", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteValueTokenWithParentAsync()
    {
        var v = new JValue(1);

        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();

        await writer.WriteTokenAsync(v.CreateReader());

        Assert.Equal(WriteState.Array, writer.WriteState);

        await writer.WriteEndArrayAsync();

        XUnitAssert.AreEqualNormalized(@"[
  1
]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteEmptyTokenAsync()
    {
        var o = new JObject();
        var reader = o.CreateReader();
        while (reader.Read())
        {
        }

        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();

        await writer.WriteTokenAsync(reader);

        Assert.Equal(WriteState.Array, writer.WriteState);

        await writer.WriteEndArrayAsync();

        XUnitAssert.AreEqualNormalized(@"[]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteRawValueAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();
        await writer.WriteRawValueAsync("fail");
        await writer.WriteRawValueAsync("fail");
        await writer.WriteEndArrayAsync();

        XUnitAssert.AreEqualNormalized(@"[
  fail,
  fail
]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteDuplicatePropertyNameAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartObjectAsync();

        await writer.WritePropertyNameAsync("prop1");
        await writer.WriteStartObjectAsync();
        await writer.WriteEndObjectAsync();

        await writer.WritePropertyNameAsync("prop1");
        await writer.WriteStartArrayAsync();
        await writer.WriteEndArrayAsync();

        await writer.WriteEndObjectAsync();

        XUnitAssert.AreEqualNormalized(@"{
  ""prop1"": []
}", writer.Token.ToString());
    }
}