# Serializing Collections

This sample serializes a collection to JSON.

<!-- snippet: SerializeCollection -->
<a id='snippet-serializecollection'></a>
```cs
var videogames = new List<string>
{
    "Starcraft",
    "Halo",
    "Legend of Zelda"
};

var json = JsonConvert.SerializeObject(videogames);

Console.WriteLine(json);
// ["Starcraft","Halo","Legend of Zelda"]
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeCollection.cs#L31-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializecollection' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
