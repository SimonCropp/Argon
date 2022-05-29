// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1321 : TestFixtureBase
{
    [Fact]
    public void Test() =>
        XUnitAssert.Throws<JsonWriterException>(
            () =>
            {
                JsonConvert.DeserializeObject(
                    @"[""1"",",
                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024});
            },
            "Unexpected end when reading token. Path ''.");

    [Fact]
    public void Test2()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));

        XUnitAssert.Throws<JsonWriterException>(
            () => writer.WriteToken(reader),
            "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public async Task Test2_Async()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));

        await XUnitAssert.ThrowsAsync<JsonWriterException>(
            () => writer.WriteTokenAsync(reader),
            "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public void Test3()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));
        reader.Read();

        XUnitAssert.Throws<JsonWriterException>(
            () => writer.WriteToken(reader),
            "Unexpected end when reading token. Path ''.");
    }

    [Fact]
    public async Task Test3_Async()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader(@"[""1"","));
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonWriterException>(
            () => writer.WriteTokenAsync(reader),
            "Unexpected end when reading token. Path ''.");
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
            () => writer.WriteToken(reader),
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

        await XUnitAssert.ThrowsAsync<JsonWriterException>(
            () => writer.WriteTokenAsync(reader),
            "Unexpected end when reading token. Path ''.");
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
            () => jsonWriter.WriteToken(reader),
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
            () => jsonWriter.WriteTokenAsync(reader),
            "Unexpected end when reading token. Path '[0]'.");
    }
}