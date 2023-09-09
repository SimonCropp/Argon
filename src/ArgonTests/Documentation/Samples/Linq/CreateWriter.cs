// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class CreateWriter : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region CreateWriter

        var o = new JObject
        {
            {"name1", "value1"},
            {"name2", "value2"}
        };

        var writer = o.CreateWriter();
        writer.WritePropertyName("name3");
        writer.WriteStartArray();
        writer.WriteValue(1);
        writer.WriteValue(2);
        writer.WriteEndArray();

        Console.WriteLine(o.ToString());
        // {
        //   "name1": "value1",
        //   "name2": "value2",
        //   "name3": [
        //     1,
        //     2
        //   ]
        // }

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            {
              "name1": "value1",
              "name2": "value2",
              "name3": [
                1,
                2
              ]
            }
            """,
            o.ToString());
    }
}