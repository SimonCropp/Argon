// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1708 : TestFixtureBase
{
    [Fact]
    public void Test_DateTime()
    {
        var jsonTextReader = new JsonTextReader(new StringReader("'2018-05-27T23:25:08Z'"));
        jsonTextReader.DateParseHandling = DateParseHandling.None;
        jsonTextReader.Read();

        var serializer = new JsonSerializer();
        var dt = serializer.Deserialize<DateTime>(jsonTextReader);

        Assert.Equal(DateTimeKind.Utc, dt.Kind);
    }
}