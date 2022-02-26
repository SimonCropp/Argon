// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializeDateFormatHandling : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region SerializeDateFormatHandling
        var mayanEndOfTheWorld = new DateTime(2012, 12, 21);

        var jsonIsoDate = JsonConvert.SerializeObject(mayanEndOfTheWorld);

        Console.WriteLine(jsonIsoDate);
        // "2012-12-21T00:00:00"

        var jsonMsDate = JsonConvert.SerializeObject(mayanEndOfTheWorld, new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        });

        Console.WriteLine(jsonMsDate);
        // "\/Date(1356044400000+0100)\/"
        #endregion

        Assert.NotNull(jsonMsDate);
    }
}