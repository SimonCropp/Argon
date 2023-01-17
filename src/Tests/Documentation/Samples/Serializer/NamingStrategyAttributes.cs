// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class NamingStrategyAttributes : TestFixtureBase
{
    #region NamingStrategyAttributesTypes

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int SnakeRating { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region NamingStrategyAttributesUsage

        var user = new User
        {
            FirstName = "Tom",
            LastName = "Riddle",
            SnakeRating = 10
        };

        var json = JsonConvert.SerializeObject(user, Formatting.Indented);

        Console.WriteLine(json);
        // {
        //   "firstName": "Tom",
        //   "lastName": "Riddle",
        //   "snake_rating": 10
        // }

        #endregion

        XUnitAssert.AreEqualNormalized("""
            {
              "firstName": "Tom",
              "lastName": "Riddle",
              "snake_rating": 10
            }
            """, json);
    }
}