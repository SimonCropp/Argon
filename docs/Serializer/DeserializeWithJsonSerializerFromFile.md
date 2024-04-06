# Deserialize JSON from a file

This sample deserializes JSON retrieved from a file.

<!-- snippet: DeserializeWithJsonSerializerFromFileTypes -->
<a id='snippet-DeserializeWithJsonSerializerFromFileTypes'></a>
```cs
public class Movie
{
    public string Name { get; set; }
    public int Year { get; set; }
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/DeserializeWithJsonSerializerFromFile.cs#L8-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-DeserializeWithJsonSerializerFromFileTypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeWithJsonSerializerFromFileUsage -->
<a id='snippet-DeserializeWithJsonSerializerFromFileUsage'></a>
```cs
// read file into a string and deserialize JSON to a type
var movie1 = JsonConvert.DeserializeObject<Movie>(File.ReadAllText(@"c:\movie.json"));

// deserialize JSON directly from a file
using var file = File.OpenText(@"c:\movie.json");
var serializer = new JsonSerializer();
var movie2 = (Movie) serializer.Deserialize(file, typeof(Movie));
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/DeserializeWithJsonSerializerFromFile.cs#L21-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-DeserializeWithJsonSerializerFromFileUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
