// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class WriteToJsonFile : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region WriteToJsonFile

        var videogameRatings = new JObject(
            new JProperty("Halo", 9),
            new JProperty("Starcraft", 9),
            new JProperty("Call of Duty", 7.5));

        File.WriteAllText(@"c:\videogames.json", videogameRatings.ToString());

        // write JSON directly to a file
        using var file = File.CreateText(@"c:\videogames.json");
        using var writer = new JsonTextWriter(file);
        videogameRatings.WriteTo(writer);

        #endregion
    }

    public static class File
    {
        public static StreamWriter CreateText(string path)
        {
            return new(new MemoryStream());
        }

        public static void WriteAllText(string s1, string s2)
        {
        }
    }
}