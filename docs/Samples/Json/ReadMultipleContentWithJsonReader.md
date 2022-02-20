# Read Multiple Fragments With JsonReader

This sample sets `Argon.JsonReader.SupportMultipleContent` to true so that multiple JSON fragments can be read from a `System.IO.Stream` or `System.IO.TextReader`.

<!-- snippet: ReadMultipleContentWithJsonReaderTypes -->
<a id='snippet-readmultiplecontentwithjsonreadertypes'></a>
```cs
public class Role
{
    public string Name { get; set; }
}
```
<sup><a href='/Src/Tests/Documentation/Samples/Json/ReadMultipleContentWithJsonReader.cs#L32-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-readmultiplecontentwithjsonreadertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: ReadMultipleContentWithJsonReaderUsage -->
<a id='snippet-readmultiplecontentwithjsonreaderusage'></a>
```cs
var json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";

IList<Role> roles = new List<Role>();

var reader = new JsonTextReader(new StringReader(json));
reader.SupportMultipleContent = true;

while (true)
{
    if (!reader.Read())
    {
        break;
    }

    var serializer = new JsonSerializer();
    var role = serializer.Deserialize<Role>(reader);

    roles.Add(role);
}

foreach (var role in roles)
{
    Console.WriteLine(role.Name);
}

// Admin
// Publisher
```
<sup><a href='/Src/Tests/Documentation/Samples/Json/ReadMultipleContentWithJsonReader.cs#L42-L70' title='Snippet source file'>snippet source</a> | <a href='#snippet-readmultiplecontentwithjsonreaderusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
