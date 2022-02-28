// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class JValueAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task FloatParseHandlingAsync()
    {
        var v = (JValue)await JToken.ReadFromAsync(
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

    readonly Rate _rate = new() { Compoundings = 12.166666666666666666666666667m };

    [Fact]
    public async Task ParseAndConvertDateTimeOffsetAsync()
    {
        var json = @"{ d: ""2013-08-14T04:38:31.000+1230"" }";

        using var stringReader = new StringReader(json);
        using var jsonReader = new JsonTextReader(stringReader);
        jsonReader.DateParseHandling = DateParseHandling.DateTimeOffset;

        var obj = await JObject.LoadAsync(jsonReader);
        var d = (JValue)obj["d"];

        Assert.IsType(typeof(DateTimeOffset), d.Value);
        var offset = ((DateTimeOffset)d.Value).Offset;
        Assert.Equal(TimeSpan.FromHours(1), offset);

        var dateTimeOffset = (DateTimeOffset)d;
        Assert.Equal(TimeSpan.FromHours(1), dateTimeOffset.Offset);
    }

    [Fact]
    public async Task ParseIsoTimeZonesAsync()
    {
        var expectedDate = new DateTimeOffset(2013, 08, 14, 4, 38, 31, TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(30)));
        var reader = new JsonTextReader(new StringReader("'2013-08-14T04:38:31.000+1230'"));
        reader.DateParseHandling = DateParseHandling.DateTimeOffset;
        var date = (JValue)await JToken.ReadFromAsync(reader);
        Assert.Equal(expectedDate, date.Value);

        var expectedDate2 = new DateTimeOffset(2013, 08, 14, 4, 38, 31, TimeSpan.FromHours(12));
        var reader2 = new JsonTextReader(new StringReader("'2013-08-14T04:38:31.000+12'"));
        reader2.DateParseHandling = DateParseHandling.DateTimeOffset;
        var date2 = (JValue)await JToken.ReadFromAsync(reader2);
        Assert.Equal(expectedDate2, date2.Value);
    }
}