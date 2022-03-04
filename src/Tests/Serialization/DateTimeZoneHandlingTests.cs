// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class DateTimeZoneHandlingTests : TestFixtureBase
{
    [Fact]
    public void DeserializeObject()
    {
        var json = @"
  {
    ""Value"": ""2017-12-05T21:59:00""
  }";

        var c1 = JsonConvert.DeserializeObject<DateTimeWrapper>(json, new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        });

        var c2 = JsonConvert.DeserializeObject<DateTimeWrapper>(json, new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        });

        var c3 = JsonConvert.DeserializeObject<DateTimeWrapper>(json, new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        });

        var c4 = JsonConvert.DeserializeObject<DateTimeWrapper>(json);

        Assert.Equal(DateTimeKind.Utc, c1.Value.Kind);
        Assert.Equal(DateTimeKind.Local, c2.Value.Kind);
        Assert.Equal(DateTimeKind.Unspecified, c3.Value.Kind);
        Assert.Equal(DateTimeKind.Unspecified, c4.Value.Kind);
    }

    [Fact]
    public void DeserializeFromJObject()
    {
        var json = @"
  {
    ""Value"": ""2017-12-05T21:59:00""
  }";

        var jo = JObject.Parse(json);

        var c1 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        }));

        var c2 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        }));

        var c3 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        }));

        var c4 = jo.ToObject<DateTimeWrapper>();

        Assert.Equal(DateTimeKind.Utc, c1.Value.Kind);
        Assert.Equal(DateTimeKind.Local, c2.Value.Kind);
        Assert.Equal(DateTimeKind.Unspecified, c3.Value.Kind);
        Assert.Equal(DateTimeKind.Unspecified, c4.Value.Kind);
    }
}