// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1592 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var json = @"{
""test customer's"": ""testing""
}";

        var stringReader = new StringReader(json);
        var reader = new JsonTextReader(stringReader);

        reader.Read();
        reader.Read();
        reader.Read();

        Assert.Equal("testing", reader.Value);
        Assert.Equal("['test customer\\'s']", reader.Path);
    }
}