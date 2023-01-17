# Read JSON using JsonTextReader

This sample reads JSON using the `Argon.JsonTextReader`.

<!-- snippet: ReadJsonWithJsonTextReader -->
<a id='snippet-readjsonwithjsontextreader'></a>
```cs
var json = """
    {
       'CPU': 'Intel',
       'PSU': '500W',
       'Drives': [
         'DVD read/writer'
         /*(broken)*/,
         '500 gigabyte hard drive',
         '200 gigabyte hard drive'
       ]
    }
    """;

var reader = new JsonTextReader(new StringReader(json));
while (reader.Read())
{
    if (reader.Value != null)
    {
        Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
    }
    else
    {
        Console.WriteLine("Token: {0}", reader.TokenType);
    }
}

// Token: StartObject
// Token: PropertyName, Value: CPU
// Token: String, Value: Intel
// Token: PropertyName, Value: PSU
// Token: String, Value: 500W
// Token: PropertyName, Value: Drives
// Token: StartArray
// Token: String, Value: DVD read/writer
// Token: Comment, Value: (broken)
// Token: String, Value: 500 gigabyte hard drive
// Token: String, Value: 200 gigabyte hard drive
// Token: EndArray
// Token: EndObject
```
<sup><a href='/src/Tests/Documentation/Samples/Json/ReadJsonWithJsonTextReader.cs#L10-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-readjsonwithjsontextreader' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
