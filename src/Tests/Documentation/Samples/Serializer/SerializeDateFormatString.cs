// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializeDateFormatString : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region SerializeDateFormatString
        var dateList = new List<DateTime>
        {
            new(2009, 12, 7, 23, 10, 0, DateTimeKind.Utc),
            new(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            new(2010, 2, 10, 10, 0, 0, DateTimeKind.Utc)
        };

        var json = JsonConvert.SerializeObject(dateList, new JsonSerializerSettings
        {
            DateFormatString = "d MMMM, yyyy",
            Formatting = Formatting.Indented
        });

        Console.WriteLine(json);
        // [
        //   "7 December, 2009",
        //   "1 January, 2010",
        //   "10 February, 2010"
        // ]
        #endregion

        XUnitAssert.AreEqualNormalized(@"[
  ""7 December, 2009"",
  ""1 January, 2010"",
  ""10 February, 2010""
]", json);
    }
}