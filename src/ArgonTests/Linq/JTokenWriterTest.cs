// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


public class JTokenWriterTest : TestFixtureBase
{
    [Fact]
    public void ValueFormatting()
    {
        var data = "Hello world."u8.ToArray();

        JToken root;
        using (var jsonWriter = new JTokenWriter())
        {
            jsonWriter.WriteStartArray();
            jsonWriter.WriteValue('@');
            jsonWriter.WriteValue("\r\n\t\f\b?{\\r\\n\"\'");
            jsonWriter.WriteValue(true);
            jsonWriter.WriteValue(10);
            jsonWriter.WriteValue(10.99);
            jsonWriter.WriteValue(0.99);
            jsonWriter.WriteValue(0.000000000000000001d);
            jsonWriter.WriteValue(0.000000000000000001m);
            jsonWriter.WriteValue((string) null);
            jsonWriter.WriteValue("This is a string.");
            jsonWriter.WriteNull();
            jsonWriter.WriteUndefined();
            jsonWriter.WriteValue(data);
            jsonWriter.WriteEndArray();

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
    public void State()
    {
        using JsonWriter jsonWriter = new JTokenWriter();
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);

        jsonWriter.WriteStartObject();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);

        jsonWriter.WritePropertyName("CPU");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);

        jsonWriter.WriteValue("Intel");
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);

        jsonWriter.WritePropertyName("Drives");
        Assert.Equal(WriteState.Property, jsonWriter.WriteState);

        jsonWriter.WriteStartArray();
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        jsonWriter.WriteValue("DVD read/writer");
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        jsonWriter.WriteValue(new BigInteger(123));
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        jsonWriter.WriteValue(Array.Empty<byte>());
        Assert.Equal(WriteState.Array, jsonWriter.WriteState);

        jsonWriter.WriteEnd();
        Assert.Equal(WriteState.Object, jsonWriter.WriteState);

        jsonWriter.WriteEndObject();
        Assert.Equal(WriteState.Start, jsonWriter.WriteState);
    }

    [Fact]
    public void CurrentToken()
    {
        using var writer = new JTokenWriter();
        Assert.Equal(WriteState.Start, writer.WriteState);
        Assert.Equal(null, writer.CurrentToken);

        writer.WriteStartObject();
        Assert.Equal(WriteState.Object, writer.WriteState);
        Assert.Equal(writer.Token, writer.CurrentToken);

        var o = (JObject) writer.Token;

        writer.WritePropertyName("CPU");
        Assert.Equal(WriteState.Property, writer.WriteState);
        Assert.Equal(o.Property("CPU"), writer.CurrentToken);

        writer.WriteValue("Intel");
        Assert.Equal(WriteState.Object, writer.WriteState);
        Assert.Equal(o["CPU"], writer.CurrentToken);

        writer.WritePropertyName("Drives");
        Assert.Equal(WriteState.Property, writer.WriteState);
        Assert.Equal(o.Property("Drives"), writer.CurrentToken);

        writer.WriteStartArray();
        Assert.Equal(WriteState.Array, writer.WriteState);
        Assert.Equal(o["Drives"], writer.CurrentToken);

        var a = (JArray) writer.CurrentToken;

        writer.WriteValue("DVD read/writer");
        Assert.Equal(WriteState.Array, writer.WriteState);
        Assert.Equal(a[^1], writer.CurrentToken);

        writer.WriteValue(new BigInteger(123));
        Assert.Equal(WriteState.Array, writer.WriteState);
        Assert.Equal(a[^1], writer.CurrentToken);

        writer.WriteValue(Array.Empty<byte>());
        Assert.Equal(WriteState.Array, writer.WriteState);
        Assert.Equal(a[^1], writer.CurrentToken);

        writer.WriteEnd();
        Assert.Equal(WriteState.Object, writer.WriteState);
        Assert.Equal(a, writer.CurrentToken);

        writer.WriteEndObject();
        Assert.Equal(WriteState.Start, writer.WriteState);
        Assert.Equal(o, writer.CurrentToken);
    }

    [Fact]
    public void WriteComment()
    {
        var writer = new JTokenWriter();

        writer.WriteStartArray();
        writer.WriteComment("fail");
        writer.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"[
  /*fail*/]", writer.Token.ToString());
    }

    [Fact]
    public void WriteBigInteger()
    {
        var writer = new JTokenWriter();

        writer.WriteStartArray();
        writer.WriteValue(new BigInteger(123));
        writer.WriteEndArray();

        var i = (JValue) writer.Token[0];

        Assert.Equal(new BigInteger(123), i.Value);
        Assert.Equal(JTokenType.Integer, i.Type);

        XUnitAssert.AreEqualNormalized(@"[
  123
]", writer.Token.ToString());
    }

    [Fact]
    public void WriteRaw()
    {
        var writer = new JTokenWriter();

        writer.WriteStartArray();
        writer.WriteRaw("fail");
        writer.WriteRaw("fail");
        writer.WriteEndArray();

        // this is a bug. write raw shouldn't be autocompleting like this
        // hard to fix without introducing Raw and RawValue token types
        // meh
        XUnitAssert.AreEqualNormalized(@"[
  fail,
  fail
]", writer.Token.ToString());
    }

    [Fact]
    public void WriteTokenWithParent()
    {
        var o = new JObject
        {
            ["prop1"] = new JArray(1),
            ["prop2"] = 1
        };

        var writer = new JTokenWriter();

        writer.WriteStartArray();

        writer.WriteToken(o.CreateReader());

        Assert.Equal(WriteState.Array, writer.WriteState);

        writer.WriteEndArray();

        Console.WriteLine(writer.Token.ToString());

        XUnitAssert.AreEqualNormalized(
            """
            [
              {
                "prop1": [
                  1
                ],
                "prop2": 1
              }
            ]
            """,
            writer.Token.ToString());
    }

    [Fact]
    public void WriteTokenWithPropertyParent()
    {
        var v = new JValue(1);

        var writer = new JTokenWriter();

        writer.WriteStartObject();
        writer.WritePropertyName("Prop1");

        writer.WriteToken(v.CreateReader());

        Assert.Equal(WriteState.Object, writer.WriteState);

        writer.WriteEndObject();

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Prop1": 1
            }
            """,
            writer.Token.ToString());
    }

    [Fact]
    public void WriteValueTokenWithParent()
    {
        var v = new JValue(1);

        var writer = new JTokenWriter();

        writer.WriteStartArray();

        writer.WriteToken(v.CreateReader());

        Assert.Equal(WriteState.Array, writer.WriteState);

        writer.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"[
  1
]", writer.Token.ToString());
    }

    [Fact]
    public void WriteEmptyToken()
    {
        var o = new JObject();
        var reader = o.CreateReader();
        while (reader.Read())
        {
        }

        var writer = new JTokenWriter();

        writer.WriteStartArray();

        writer.WriteToken(reader);

        Assert.Equal(WriteState.Array, writer.WriteState);

        writer.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"[]", writer.Token.ToString());
    }

    [Fact]
    public void WriteRawValue()
    {
        var writer = new JTokenWriter();

        writer.WriteStartArray();
        writer.WriteRawValue("fail");
        writer.WriteRawValue("fail");
        writer.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"[
  fail,
  fail
]", writer.Token.ToString());
    }

    [Fact]
    public void WriteDuplicatePropertyName()
    {
        var writer = new JTokenWriter();

        writer.WriteStartObject();

        writer.WritePropertyName("prop1");
        writer.WriteStartObject();
        writer.WriteEndObject();

        writer.WritePropertyName("prop1");
        writer.WriteStartArray();
        writer.WriteEndArray();

        writer.WriteEndObject();

        XUnitAssert.AreEqualNormalized(
            """
            {
              "prop1": []
            }
            """,
            writer.Token.ToString());
    }

    [Fact]
    public void WriteTokenDirect()
    {
        JToken token;

        using (var writer = new JTokenWriter())
        {
            writer.WriteToken(JsonToken.StartArray);
            writer.WriteToken(JsonToken.Integer, 1);
            writer.WriteToken(JsonToken.StartObject);
            writer.WriteToken(JsonToken.PropertyName, "integer");
            writer.WriteToken(JsonToken.Integer, int.MaxValue);
            writer.WriteToken(JsonToken.PropertyName, "null-string");
            writer.WriteToken(JsonToken.String, null);
            writer.WriteToken(JsonToken.EndObject);
            writer.WriteToken(JsonToken.EndArray);

            token = writer.Token;
        }

        Assert.Equal(@"[1,{""integer"":2147483647,""null-string"":null}]", token.ToString(Formatting.None));
    }
}