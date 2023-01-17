# JsonPropertyAttribute required

This sample uses `Argon.JsonPropertyAttribute` to set `Argon.Required` which is used during deserialization to validate the presence of required JSON properties.

<!-- snippet: JsonPropertyRequiredTypes -->
<a id='snippet-jsonpropertyrequiredtypes'></a>
```cs
public class Videogame
{
    [JsonProperty(Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty(Required = Required.AllowNull)]
    public DateTime? ReleaseDate { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonPropertyRequired.cs#L7-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertyrequiredtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonPropertyRequiredUsage -->
<a id='snippet-jsonpropertyrequiredusage'></a>
```cs
var json = """
    {
      'Name': 'Starcraft III',
      'ReleaseDate': null
    }
    """;

var starcraft = JsonConvert.DeserializeObject<Videogame>(json);

Console.WriteLine(starcraft.Name);
// Starcraft III

Console.WriteLine(starcraft.ReleaseDate);
// null
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonPropertyRequired.cs#L23-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertyrequiredusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
