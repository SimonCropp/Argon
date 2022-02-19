# Serialize JSON to a file

This sample serializes JSON to a file.

<!-- snippet: SerializeWithJsonSerializerToFileTypes -->
<a id='snippet-serializewithjsonserializertofiletypes'></a>
```cs
public class Movie
{
    public string Name { get; set; }
    public int Year { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeWithJsonSerializerToFile.cs#L32-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializewithjsonserializertofiletypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeWithJsonSerializerToFileUsage -->
<a id='snippet-serializewithjsonserializertofileusage'></a>
```cs
var movie = new Movie
{
    Name = "Bad Boys",
    Year = 1995
};

// serialize JSON to a string and then write string to a file
File.WriteAllText(@"c:\movie.json", JsonConvert.SerializeObject(movie));

// serialize JSON directly to a file
using (var file = File.CreateText(@"c:\movie.json"))
{
    var serializer = new JsonSerializer();
    serializer.Serialize(file, movie);
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeWithJsonSerializerToFile.cs#L43-L59' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializewithjsonserializertofileusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
