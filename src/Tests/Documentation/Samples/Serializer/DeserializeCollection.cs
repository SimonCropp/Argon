// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeCollection : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region DeserializeCollection
        var json = @"['Starcraft','Halo','Legend of Zelda']";

        var videogames = JsonConvert.DeserializeObject<List<string>>(json);

        Console.WriteLine(string.Join(", ", videogames.ToArray()));
        // Starcraft, Halo, Legend of Zelda
        #endregion

        Assert.Equal("Starcraft, Halo, Legend of Zelda", string.Join(", ", videogames.ToArray()));
    }
}