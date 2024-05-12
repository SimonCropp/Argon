# Deserialize an immutable collection

This sample deserializes JSON into an immutable collection.

<!-- snippet: DeserializeImmutableCollections -->
<a id='snippet-DeserializeImmutableCollections'></a>
```cs
var json = """
           [
             'One',
             'II',
             '3'
           ]
           """;

var l = JsonConvert.DeserializeObject<ImmutableList<string>>(json);

foreach (var s in l)
{
    Console.WriteLine(s);
}

// One
// II
// 3
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/DeserializeImmutableCollections.cs#L10-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-DeserializeImmutableCollections' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
