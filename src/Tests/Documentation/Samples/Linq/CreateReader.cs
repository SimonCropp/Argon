// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class CreateReader : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region CreateReader
        var o = new JObject
        {
            { "Cpu", "Intel" },
            { "Memory", 32 },
            {
                "Drives", new JArray
                {
                    "DVD",
                    "SSD"
                }
            }
        };

        var reader = o.CreateReader();
        while (reader.Read())
        {
            Console.Write(reader.TokenType);
            if (reader.Value != null)
            {
                Console.Write($" - {reader.Value}");
            }

            Console.WriteLine();
        }

        // StartObject
        // PropertyName - Cpu
        // String - Intel
        // PropertyName - Memory
        // Integer - 32
        // PropertyName - Drives
        // StartArray
        // String - DVD
        // String - SSD
        // EndArray
        // EndObject
        #endregion

        Assert.False(reader.Read());
    }
}