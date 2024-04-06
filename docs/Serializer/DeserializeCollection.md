# Deserialize a Collection

This sample deserializes JSON into a collection.

<!-- snippet: DeserializeCollection -->
<a id='snippet-DeserializeCollection'></a>
```cs
var json = "['Starcraft','Halo','Legend of Zelda']";

var videogames = JsonConvert.DeserializeObject<List<string>>(json);

Console.WriteLine(string.Join(", ", videogames.ToArray()));
// Starcraft, Halo, Legend of Zelda
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/DeserializeCollection.cs#L10-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-DeserializeCollection' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
