# Parsing JSON

LINQ to JSON has methods available for parsing JSON from a string or loading JSON directly from a file.


## Parsing JSON text

JSON values can be read from a string using `Argon.JToken.Parse(System.String)`.

<!-- snippet: LinqToJsonCreateParse -->
<a id='snippet-LinqToJsonCreateParse'></a>
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
<sup><a href='/src/ArgonTests/Documentation/LinqToJsonTests.cs#L166-L180' title='Snippet source file'>snippet source</a> | <a href='#snippet-LinqToJsonCreateParse' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: LinqToJsonCreateParseArray -->
<a id='snippet-LinqToJsonCreateParseArray'></a>
```cs
var json = """
           [
             'Small',
             'Medium',
             'Large'
           ]
           """;

var a = JArray.Parse(json);
```
<sup><a href='/src/ArgonTests/Documentation/LinqToJsonTests.cs#L186-L198' title='Snippet source file'>snippet source</a> | <a href='#snippet-LinqToJsonCreateParseArray' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Loading JSON from a file

JSON can also be loaded directly from a file using `Argon.JToken.ReadFrom(Argon.JsonReader)`.

<!-- snippet: LinqToJsonReadObject -->
<a id='snippet-LinqToJsonReadObject'></a>
```cs
using var reader = File.OpenText(@"c:\person.json");
var o = (JObject) JToken.ReadFrom(new JsonTextReader(reader));
// do stuff
```
<sup><a href='/src/ArgonTests/Documentation/LinqToJsonTests.cs#L210-L216' title='Snippet source file'>snippet source</a> | <a href='#snippet-LinqToJsonReadObject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * LINQtoJSON
 * `Argon.JToken.Parse(System.String)`
 * `Argon.JToken.ReadFrom(Argon.JsonReader)`
