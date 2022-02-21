# MaxDepth setting

This sample uses the `Argon.JsonSerializerSettings.MaxDepth` setting to constrain JSON to a maximum depth when deserializing.

<!-- snippet: MaxDepth -->
<a id='snippet-maxdepth'></a>
```cs
var json = @"[
      [
        [
          '1',
          'Two',
          'III'
        ]
      ]
    ]";

try
{
    JsonConvert.DeserializeObject<List<IList<IList<string>>>>(json, new JsonSerializerSettings
    {
        MaxDepth = 2
    });
}
catch (JsonReaderException ex)
{
    Console.WriteLine(ex.Message);
    // The reader's MaxDepth of 2 has been exceeded. Path '[0][0]', line 3, position 12.
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/MaxDepth.cs#L31-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-maxdepth' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
