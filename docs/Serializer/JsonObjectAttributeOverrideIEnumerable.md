# JsonObjectAttribute force object serialization

This sample uses `Argon.JsonObjectAttribute` to serialize a class that implements `System.Collections.Generic.IEnumerable<T>` as a JSON object instead of a JSON array.

<!-- snippet: JsonObjectAttributeOverrideIEnumerableTypes -->
<a id='snippet-jsonobjectattributeoverrideienumerabletypes'></a>
```cs
[JsonObject]
public class Directory : IEnumerable<string>
{
    public string Name { get; set; }
    public IList<string> Files { get; set; }

    public Directory() =>
        Files = new List<string>();

    public IEnumerator<string> GetEnumerator() =>
        Files.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/JsonObjectAttributeOverrideIEnumerable.cs#L7-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonobjectattributeoverrideienumerabletypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonObjectAttributeOverrideIEnumerableUsage -->
<a id='snippet-jsonobjectattributeoverrideienumerableusage'></a>
```cs
var directory = new Directory
{
    Name = "My Documents",
    Files =
    {
        "ImportantLegalDocuments.docx",
        "WiseFinancalAdvice.xlsx"
    }
};

var json = JsonConvert.SerializeObject(directory, Formatting.Indented);

Console.WriteLine(json);
// {
//   "Name": "My Documents",
//   "Files": [
//     "ImportantLegalDocuments.docx",
//     "WiseFinancalAdvice.xlsx"
//   ]
// }
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/JsonObjectAttributeOverrideIEnumerable.cs#L30-L53' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonobjectattributeoverrideienumerableusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
