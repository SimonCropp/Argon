// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1460 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        JsonWriter.WriteValue(jsonWriter, PrimitiveTypeCode.Object, null);

        Assert.Equal("null", stringWriter.ToString());
    }
}