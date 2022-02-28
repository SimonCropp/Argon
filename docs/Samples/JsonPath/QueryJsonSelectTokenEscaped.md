# Querying JSON with JSON Path and escaped properties

This sample loads JSON with properties that need to be escaped when queried with `Argon.Linq.JToken.SelectToken(System.String)`.

<!-- snippet: QueryJsonSelectTokenEscaped -->
<a id='snippet-queryjsonselecttokenescaped'></a>
```cs
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
```
<sup><a href='/src/Tests/Documentation/Samples/JsonPath/QueryJsonSelectTokenEscaped.cs#L10-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryjsonselecttokenescaped' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
