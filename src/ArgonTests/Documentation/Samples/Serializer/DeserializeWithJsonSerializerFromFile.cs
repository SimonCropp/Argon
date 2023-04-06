// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeWithJsonSerializerFromFile : TestFixtureBase
{
    #region DeserializeWithJsonSerializerFromFileTypes

    public class Movie
    {
        public string Name { get; set; }
        public int Year { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeWithJsonSerializerFromFileUsage

        // read file into a string and deserialize JSON to a type
        var movie1 = JsonConvert.DeserializeObject<Movie>(File.ReadAllText(@"c:\movie.json"));

        // deserialize JSON directly from a file
        using var file = File.OpenText(@"c:\movie.json");
        var serializer = new JsonSerializer();
        var movie2 = (Movie) serializer.Deserialize(file, typeof(Movie));

        #endregion
    }

    public static class File
    {
        public static string ReadAllText(string s) =>
            "{}";

        public static StreamReader OpenText(string s) =>
            new(new MemoryStream("{}"u8.ToArray()));
    }
}