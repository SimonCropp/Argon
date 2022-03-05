# Configure CamelCaseNamingStrategy

This sample configures a `Argon.Serialization.CamelCaseNamingStrategy` to not camel case dictionary keys.

<!-- snippet: NamingStrategySkipDictionaryKeysTypes -->
<a id='snippet-namingstrategyskipdictionarykeystypes'></a>
```cs
public class DailyHighScores
{
    public DateTime Date { get; set; }
    public string Game { get; set; }
    public Dictionary<string, int> UserPoints { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategySkipDictionaryKeys.cs#L7-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategyskipdictionarykeystypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: NamingStrategySkipDictionaryKeysUsage -->
<a id='snippet-namingstrategyskipdictionarykeysusage'></a>
```cs
var dailyHighScores = new DailyHighScores
{
    Date = new(2016, 6, 27, 0, 0, 0, DateTimeKind.Utc),
    Game = "Donkey Kong",
    UserPoints = new()
    {
        ["JamesNK"] = 9001,
        ["JoC"] = 1337,
        ["JessicaN"] = 1000
    }
};

var contractResolver = new DefaultContractResolver
{
    NamingStrategy = new CamelCaseNamingStrategy
    {
        ProcessDictionaryKeys = false
    }
};

var json = JsonConvert.SerializeObject(dailyHighScores, new JsonSerializerSettings
{
    ContractResolver = contractResolver,
    Formatting = Formatting.Indented
});

Console.WriteLine(json);
// {
//   "date": "2016-06-27T00:00:00Z",
//   "game": "Donkey Kong",
//   "userPoints": {
//     "JamesNK": 9001,
//     "JoC": 1337,
//     "JessicaN": 1000
//   }
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategySkipDictionaryKeys.cs#L21-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategyskipdictionarykeysusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
