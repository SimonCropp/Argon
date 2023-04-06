// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class JValueAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task FloatParseHandlingAsync()
    {
        var v = (JValue) await JToken.ReadFromAsync(
            new JsonTextReader(new StringReader("9.9"))
            {
                FloatParseHandling = FloatParseHandling.Decimal
            });

        Assert.Equal(9.9m, v.Value);
        Assert.Equal(typeof(decimal), v.Value.GetType());
    }

    public class Rate
    {
        public decimal Compoundings { get; set; }
    }
}