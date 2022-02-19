# Deserialize a Dictionary

This sample deserializes JSON into a dictionary.

<!-- snippet: DeserializeDictionary -->
<a id='snippet-deserializedictionary'></a>
```cs
var json = @"{
      'href': '/account/login.aspx',
      'target': '_blank'
    }";

var htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

Console.WriteLine(htmlAttributes["href"]);
// /account/login.aspx

Console.WriteLine(htmlAttributes["target"]);
// _blank
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DeserializeDictionary.cs#L35-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializedictionary' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
