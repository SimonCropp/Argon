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

using Xunit;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Linq;

public class JTokenWriterAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ValueFormattingAsync()
    {
        var data = Encoding.UTF8.GetBytes("Hello world.");

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
            await jsonWriter.WriteValueAsync((string)null);
            await jsonWriter.WriteValueAsync("This is a string.");
            await jsonWriter.WriteNullAsync();
            await jsonWriter.WriteUndefinedAsync();
            await jsonWriter.WriteValueAsync(data);
            await jsonWriter.WriteEndArrayAsync();

            root = jsonWriter.Token;
        }

        Xunit.Assert.IsType(typeof(JArray), root);
        Xunit.Assert.Equal(13, root.Children().Count());
        Xunit.Assert.Equal("@", (string)root[0]);
        Xunit.Assert.Equal("\r\n\t\f\b?{\\r\\n\"\'", (string)root[1]);
        XUnitAssert.True((bool)root[2]);
        Xunit.Assert.Equal(10, (int)root[3]);
        Xunit.Assert.Equal(10.99, (double)root[4]);
        Xunit.Assert.Equal(0.99, (double)root[5]);
        Xunit.Assert.Equal(0.000000000000000001d, (double)root[6]);
        Xunit.Assert.Equal(0.000000000000000001m, (decimal)root[7]);
        Xunit.Assert.Equal(null, (string)root[8]);
        Xunit.Assert.Equal("This is a string.", (string)root[9]);
        Xunit.Assert.Equal(null, ((JValue)root[10]).Value);
        Xunit.Assert.Equal(null, ((JValue)root[11]).Value);
        Xunit.Assert.Equal(data, (byte[])root[12]);
    }

    [Fact]
    public async Task StateAsync()
    {
        using (JsonWriter jsonWriter = new JTokenWriter())
        {
            Xunit.Assert.Equal(WriteState.Start, jsonWriter.WriteState);

            await jsonWriter.WriteStartObjectAsync();
            Xunit.Assert.Equal(WriteState.Object, jsonWriter.WriteState);

            await jsonWriter.WritePropertyNameAsync("CPU");
            Xunit.Assert.Equal(WriteState.Property, jsonWriter.WriteState);

            await jsonWriter.WriteValueAsync("Intel");
            Xunit.Assert.Equal(WriteState.Object, jsonWriter.WriteState);

            await jsonWriter.WritePropertyNameAsync("Drives");
            Xunit.Assert.Equal(WriteState.Property, jsonWriter.WriteState);

            await jsonWriter.WriteStartArrayAsync();
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);

            await jsonWriter.WriteValueAsync("DVD read/writer");
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);

            await jsonWriter.WriteValueAsync(new BigInteger(123));
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);

            await jsonWriter.WriteValueAsync(new byte[0]);
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);

            await jsonWriter.WriteEndAsync();
            Xunit.Assert.Equal(WriteState.Object, jsonWriter.WriteState);

            await jsonWriter.WriteEndObjectAsync();
            Xunit.Assert.Equal(WriteState.Start, jsonWriter.WriteState);
        }
    }

    [Fact]
    public async Task CurrentTokenAsync()
    {
        using (var jsonWriter = new JTokenWriter())
        {
            Xunit.Assert.Equal(WriteState.Start, jsonWriter.WriteState);
            Xunit.Assert.Equal(null, jsonWriter.CurrentToken);

            await jsonWriter.WriteStartObjectAsync();
            Xunit.Assert.Equal(WriteState.Object, jsonWriter.WriteState);
            Xunit.Assert.Equal(jsonWriter.Token, jsonWriter.CurrentToken);

            var o = (JObject)jsonWriter.Token;

            await jsonWriter.WritePropertyNameAsync("CPU");
            Xunit.Assert.Equal(WriteState.Property, jsonWriter.WriteState);
            Xunit.Assert.Equal(o.Property("CPU"), jsonWriter.CurrentToken);

            await jsonWriter.WriteValueAsync("Intel");
            Xunit.Assert.Equal(WriteState.Object, jsonWriter.WriteState);
            Xunit.Assert.Equal(o["CPU"], jsonWriter.CurrentToken);

            await jsonWriter.WritePropertyNameAsync("Drives");
            Xunit.Assert.Equal(WriteState.Property, jsonWriter.WriteState);
            Xunit.Assert.Equal(o.Property("Drives"), jsonWriter.CurrentToken);

            await jsonWriter.WriteStartArrayAsync();
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);
            Xunit.Assert.Equal(o["Drives"], jsonWriter.CurrentToken);

            var a = (JArray)jsonWriter.CurrentToken;

            await jsonWriter.WriteValueAsync("DVD read/writer");
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);
            Xunit.Assert.Equal(a[a.Count - 1], jsonWriter.CurrentToken);

            await jsonWriter.WriteValueAsync(new BigInteger(123));
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);
            Xunit.Assert.Equal(a[a.Count - 1], jsonWriter.CurrentToken);

            await jsonWriter.WriteValueAsync(new byte[0]);
            Xunit.Assert.Equal(WriteState.Array, jsonWriter.WriteState);
            Xunit.Assert.Equal(a[a.Count - 1], jsonWriter.CurrentToken);

            await jsonWriter.WriteEndAsync();
            Xunit.Assert.Equal(WriteState.Object, jsonWriter.WriteState);
            Xunit.Assert.Equal(a, jsonWriter.CurrentToken);

            await jsonWriter.WriteEndObjectAsync();
            Xunit.Assert.Equal(WriteState.Start, jsonWriter.WriteState);
            Xunit.Assert.Equal(o, jsonWriter.CurrentToken);
        }
    }

    [Fact]
    public async Task WriteCommentAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();
        await writer.WriteCommentAsync("fail");
        await writer.WriteEndArrayAsync();

        StringAssert.AreEqual(@"[
  /*fail*/]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteBigIntegerAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(new BigInteger(123));
        await writer.WriteEndArrayAsync();

        var i = (JValue)writer.Token[0];

        Xunit.Assert.Equal(new BigInteger(123), i.Value);
        Xunit.Assert.Equal(JTokenType.Integer, i.Type);

        StringAssert.AreEqual(@"[
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
        StringAssert.AreEqual(@"[
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

        Xunit.Assert.Equal(WriteState.Array, writer.WriteState);

        await writer.WriteEndArrayAsync();

        Console.WriteLine(writer.Token.ToString());

        StringAssert.AreEqual(@"[
  {
    ""prop1"": [
      1
    ],
    ""prop2"": 1
  }
]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteTokenWithPropertyParentAsync()
    {
        var v = new JValue(1);

        var writer = new JTokenWriter();

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("Prop1");

        await writer.WriteTokenAsync(v.CreateReader());

        Xunit.Assert.Equal(WriteState.Object, writer.WriteState);

        await writer.WriteEndObjectAsync();

        StringAssert.AreEqual(@"{
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

        Xunit.Assert.Equal(WriteState.Array, writer.WriteState);

        await writer.WriteEndArrayAsync();

        StringAssert.AreEqual(@"[
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

        Xunit.Assert.Equal(WriteState.Array, writer.WriteState);

        await writer.WriteEndArrayAsync();

        StringAssert.AreEqual(@"[]", writer.Token.ToString());
    }

    [Fact]
    public async Task WriteRawValueAsync()
    {
        var writer = new JTokenWriter();

        await writer.WriteStartArrayAsync();
        await writer.WriteRawValueAsync("fail");
        await writer.WriteRawValueAsync("fail");
        await writer.WriteEndArrayAsync();

        StringAssert.AreEqual(@"[
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

        StringAssert.AreEqual(@"{
  ""prop1"": []
}", writer.Token.ToString());
    }

    [Fact]
    public async Task DateTimeZoneHandlingAsync()
    {
        var writer = new JTokenWriter
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        await writer.WriteValueAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

        var value = (JValue)writer.Token;
        var dt = (DateTime)value.Value;

        Xunit.Assert.Equal(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc), dt);
    }
}