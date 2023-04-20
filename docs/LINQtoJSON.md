# LINQ to JSON

LINQ to JSON is an API for working with JSON objects. It has been designed with LINQ in mind to enable quick querying and creation of JSON objects.

<!-- snippet: LinqToJsonBasic -->
<a id='snippet-linqtojsonbasic'></a>
```cs
var o = JObject.Parse(
    """
    {
      'CPU': 'Intel',
      'Drives': [
        'DVD read/writer',
        '500 gigabyte hard drive'
      ]
    }
    """);

var cpu = (string) o["CPU"];
// Intel

var firstDrive = (string) o["Drives"][0];
// DVD read/writer

var allDrives = o["Drives"].Select(t => (string) t).ToList();
// DVD read/writer
// 500 gigabyte hard drive
```
<sup><a href='/src/ArgonTests/Documentation/LinqToJsonTests.cs#L12-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsonbasic' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Topics

 * [ParsingLINQtoJSON]
 * [CreatingLINQtoJSON]
 * [QueryingLINQtoJSON]
 * [SelectToken]


## Related Topics

 * `Argon.JObject`
 * `Argon.JArray`
 * `Argon.JValue`
