// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class CreateJsonJTokenWriter : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region CreateJsonJTokenWriter

        var writer = new JTokenWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("name1");
        writer.WriteValue("value1");
        writer.WritePropertyName("name2");
        writer.WriteStartArray();
        writer.WriteValue(1);
        writer.WriteValue(2);
        writer.WriteEndArray();
        writer.WriteEndObject();

        var o = (JObject) writer.Token;

        Console.WriteLine(o.ToString());
        // {
        //   "name1": "value1",
        //   "name2": [
        //     1,
        //     2
        //   ]
        // }

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            {
              "name1": "value1",
              "name2": [
                1,
                2
              ]
            }
            """,
            o.ToString());
    }
}