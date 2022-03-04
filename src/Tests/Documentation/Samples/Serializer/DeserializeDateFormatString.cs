// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeDateFormatString : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region DeserializeDateFormatString
        var json = @"[
              '7 December, 2009',
              '1 January, 2010',
              '10 February, 2010'
            ]";

        var dateList = JsonConvert.DeserializeObject<IList<DateTime>>(json, new JsonSerializerSettings
        {
            DateFormatString = "d MMMM, yyyy"
        });

        foreach (var dateTime in dateList)
        {
            Console.WriteLine(dateTime.ToLongDateString());
        }
        // Monday, 07 December 2009
        // Friday, 01 January 2010
        // Wednesday, 10 February 2010
        #endregion

        Assert.Equal(new(2009, 12, 7, 0, 0, 0, DateTimeKind.Utc), dateList[0]);
        Assert.Equal(new(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc), dateList[1]);
        Assert.Equal(new(2010, 2, 10, 0, 0, 0, DateTimeKind.Utc), dateList[2]);
    }
}