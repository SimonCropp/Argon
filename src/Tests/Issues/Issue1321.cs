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

namespace Argon.Tests.Issues;

public class Issue1321 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        XUnitAssert.Throws<JsonWriterException>(
            () =>
            {
                JsonConvert.DeserializeObject(
                    @"[""1"",",
                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024});
            },
            "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public void Test2()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));

        XUnitAssert.Throws<JsonWriterException>(
            () => { writer.WriteToken(reader); },
            "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public async Task Test2_Async()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));

        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () => { await writer.WriteTokenAsync(reader); }, "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public void Test3()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));
        reader.Read();

        XUnitAssert.Throws<JsonWriterException>(
            () => { writer.WriteToken(reader); },
            "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public async Task Test3_Async()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () => { await writer.WriteTokenAsync(reader); }, "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public void Test4()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[[""1"","));
        reader.Read();
        reader.Read();

        XUnitAssert.Throws<JsonWriterException>(
            () => { writer.WriteToken(reader); },
            "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public async Task Test4_Async()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[[""1"","));
        await reader.ReadAsync();
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonWriterException>(async () => { await writer.WriteTokenAsync(reader); }, "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public void Test5()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.WriteStartArray();

        var reader = new JsonTextReader(new StringReader(@"[[""1"","));
        reader.Read();
        reader.Read();

        XUnitAssert.Throws<JsonWriterException>(
            () => { jsonWriter.WriteToken(reader); },
            "Unexpected end when reading token. Path '[0]'.");
    }

    [Fact]
    public async Task Test5_Async()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.WriteStartArray();

        var reader = new JsonTextReader(new StringReader(@"[[""1"","));
        await reader.ReadAsync();
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonWriterException>(
            async () => { await jsonWriter.WriteTokenAsync(reader); },
            "Unexpected end when reading token. Path '[0]'.");
    }
}