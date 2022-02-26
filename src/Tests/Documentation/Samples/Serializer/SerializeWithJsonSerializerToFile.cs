// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializeWithJsonSerializerToFile : TestFixtureBase
{
    #region SerializeWithJsonSerializerToFileTypes
    public class Movie
    {
        public string Name { get; set; }
        public int Year { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region SerializeWithJsonSerializerToFileUsage
        var movie = new Movie
        {
            Name = "Bad Boys",
            Year = 1995
        };

        // serialize JSON to a string and then write string to a file
        File.WriteAllText(@"c:\movie.json", JsonConvert.SerializeObject(movie));

        // serialize JSON directly to a file
        using var file = File.CreateText(@"c:\movie.json");
        var serializer = new JsonSerializer();
        serializer.Serialize(file, movie);

        #endregion
    }

    public static class File
    {
        public static StreamWriter CreateText(string path)
        {
            return new StreamWriter(new MemoryStream());
        }

        public static void WriteAllText(string s1, string s2)
        {
        }
    }
}