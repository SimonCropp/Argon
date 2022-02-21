﻿#region License
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

        var c1 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        }));

        var c2 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        }));

        var c3 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new JsonSerializerSettings
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