// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializeCollection : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region SerializeCollection

        var videogames = new List<string>
        {
            "Starcraft",
            "Halo",
            "Legend of Zelda"
        };

        var json = JsonConvert.SerializeObject(videogames);

        Console.WriteLine(json);
        // ["Starcraft","Halo","Legend of Zelda"]

        #endregion

        Assert.Equal(@"[""Starcraft"",""Halo"",""Legend of Zelda""]", json);
    }
}