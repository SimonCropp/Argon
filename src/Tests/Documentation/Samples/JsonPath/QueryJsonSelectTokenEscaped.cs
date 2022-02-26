// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class QueryJsonSelectTokenEscaped : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region QueryJsonSelectTokenEscaped
        var o = JObject.Parse(@"{
              'Space Invaders': 'Taito',
              'Doom ]|[': 'id',
              ""Yar's Revenge"": 'Atari',
              'Government ""Intelligence""': 'Make-Believe'
            }");

        var spaceInvaders = (string)o.SelectToken("['Space Invaders']");
        // Taito

        var doom3 = (string)o.SelectToken("['Doom ]|[']");
        // id

        var yarsRevenge = (string)o.SelectToken("['Yar\\'s Revenge']");
        // Atari

        var governmentIntelligence = (string)o.SelectToken("['Government \"Intelligence\"']");
        // Make-Believe
        #endregion

        Assert.Equal("Taito", spaceInvaders);
        Assert.Equal("id", doom3);
        Assert.Equal("Atari", yarsRevenge);
        Assert.Equal("Make-Believe", governmentIntelligence);
    }
}