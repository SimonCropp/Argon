# Write JSON to a file

This sample writes LINQ to JSON objects to a file.

<!-- snippet: WriteToJsonFile -->
<a id='snippet-WriteToJsonFile'></a>
```cs
var videogameRatings = new JObject(
    new JProperty("Halo", 9),
    new JProperty("Starcraft", 9),
    new JProperty("Call of Duty", 7.5));

File.WriteAllText(@"c:\videogames.json", videogameRatings.ToString());

// write JSON directly to a file
using var file = File.CreateText(@"c:\videogames.json");
using var writer = new JsonTextWriter(file);
videogameRatings.WriteTo(writer);
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/WriteToJsonFile.cs#L10-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-WriteToJsonFile' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
