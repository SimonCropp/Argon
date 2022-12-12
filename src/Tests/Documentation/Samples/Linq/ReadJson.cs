// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class ReadJson : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ReadJson

        var o1 = JObject.Parse(File.ReadAllText(@"c:\videogames.json"));

        // read JSON directly from a file
        using var file = File.OpenText(@"c:\videogames.json");
        using var reader = new JsonTextReader(file);
        var o2 = (JObject) JToken.ReadFrom(reader);

        #endregion
    }

    public static class File
    {
        public static StreamReader OpenText(string path) =>
            new(new MemoryStream("{}"u8.ToArray()));

        public static string ReadAllText(string path) =>
            "{}";
    }
}