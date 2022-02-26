// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class JsonPropertyRequired : TestFixtureBase
{
    #region JsonPropertyRequiredTypes
    public class Videogame
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public DateTime? ReleaseDate { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region JsonPropertyRequiredUsage
        var json = @"{
              'Name': 'Starcraft III',
              'ReleaseDate': null
            }";

        var starcraft = JsonConvert.DeserializeObject<Videogame>(json);

        Console.WriteLine(starcraft.Name);
        // Starcraft III

        Console.WriteLine(starcraft.ReleaseDate);
        // null
        #endregion
    }
}