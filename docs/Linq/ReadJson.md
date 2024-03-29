# Read JSON from a file using JObject

This sample reads JSON from a file into a `Argon.JObject`.

<!-- snippet: ReadJson -->
<a id='snippet-readjson'></a>
```cs
var o1 = JObject.Parse(File.ReadAllText(@"c:\videogames.json"));

// read JSON directly from a file
using var file = File.OpenText(@"c:\videogames.json");
using var reader = new JsonTextReader(file);
var o2 = (JObject) JToken.ReadFrom(reader);
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/ReadJson.cs#L12-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-readjson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
