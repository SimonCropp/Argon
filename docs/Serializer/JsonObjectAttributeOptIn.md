## JsonObjectAttribute opt-in serialization

This sample uses `Argon.JsonObjectAttribute` and `Argon.MemberSerialization` to specify that only properties that have been explicitly specified with `Argon.JsonPropertyAttribute` should be serialized.

<!-- snippet: JsonObjectAttributeOptInTypes -->
<a id='snippet-jsonobjectattributeoptintypes'></a>
```cs
[JsonObject(MemberSerialization.OptIn)]
public class File
{
    // excluded from serialization
    // does not have JsonPropertyAttribute
    public Guid Id { get; set; }

    [JsonProperty] public string Name { get; set; }

    [JsonProperty] public int Size { get; set; }
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/JsonObjectAttributeOptIn.cs#L7-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonobjectattributeoptintypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonObjectAttributeOptInUsage -->
<a id='snippet-jsonobjectattributeoptinusage'></a>
```cs
var file = new File
{
    Id = Guid.NewGuid(),
    Name = "ImportantLegalDocuments.docx",
    Size = 50 * 1024
};

var json = JsonConvert.SerializeObject(file, Formatting.Indented);

Console.WriteLine(json);
// {
//   "Name": "ImportantLegalDocuments.docx",
//   "Size": 51200
// }
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/JsonObjectAttributeOptIn.cs#L26-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonobjectattributeoptinusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
