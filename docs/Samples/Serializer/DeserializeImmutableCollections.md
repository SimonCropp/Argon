# Deserialize an immutable collection

This sample deserializes JSON into an immutable collection.

<!-- snippet: DeserializeImmutableCollections -->
<a id='snippet-deserializeimmutablecollections'></a>
```cs
var json = @"[
      'One',
      'II',
      '3'
    ]";

var l = JsonConvert.DeserializeObject<ImmutableList<string>>(json);

foreach (var s in l)
{
    Console.WriteLine(s);
}
// One
// II
// 3
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DeserializeImmutableCollections.cs#L36-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeimmutablecollections' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
