# Parsing JSON

LINQ to JSON has methods available for parsing JSON from a string or loading JSON directly from a file.


## Parsing JSON text

JSON values can be read from a string using `Argon.JToken.Parse(System.String)`.

<!-- snippet: LinqToJsonCreateParse -->
<a id='snippet-linqtojsoncreateparse'></a>
```cs
var json = """
    {
      CPU: 'Intel',
      Drives: [
        'DVD read/writer',
        '500 gigabyte hard drive'
      ]
    }
    """;

var o = JObject.Parse(json);
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L162-L176' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsoncreateparse' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: LinqToJsonCreateParseArray -->
<a id='snippet-linqtojsoncreateparsearray'></a>
```cs
var json = @"[
      'Small',
      'Medium',
      'Large'
    ]";

var a = JArray.Parse(json);
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L182-L192' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsoncreateparsearray' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Loading JSON from a file

JSON can also be loaded directly from a file using `Argon.JToken.ReadFrom(Argon.JsonReader)`.

<!-- snippet: LinqToJsonReadObject -->
<a id='snippet-linqtojsonreadobject'></a>
```cs
using var reader = File.OpenText(@"c:\person.json");
var o = (JObject) JToken.ReadFrom(new JsonTextReader(reader));
// do stuff
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L204-L210' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsonreadobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * LINQtoJSON
 * `Argon.JToken.Parse(System.String)`
 * `Argon.JToken.ReadFrom(Argon.JsonReader)`
