# Serialize an immutable collection

This sample serializes an immutable collection into JSON.

<!-- snippet: SerializeImmutableCollections -->
<a id='snippet-serializeimmutablecollections'></a>
```cs
var l = ImmutableList.CreateRange(new List<string>
{
    "One",
    "II",
    "3"
});

var json = JsonConvert.SerializeObject(l, Formatting.Indented);
// [
//   "One",
//   "II",
//   "3"
// ]
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/SerializeImmutableCollections.cs#L12-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeimmutablecollections' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
