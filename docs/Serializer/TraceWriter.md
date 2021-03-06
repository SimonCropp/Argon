# TraceWriter setting

This sample uses an `Argon.ITraceWriter` to log debug information from serialization.

<!-- snippet: TraceWriterTypes -->
<a id='snippet-tracewritertypes'></a>
```cs
public class Account
{
    public string FullName { get; set; }
    public bool Deleted { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/TraceWriter.cs#L7-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-tracewritertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: TraceWriterUsage -->
<a id='snippet-tracewriterusage'></a>
```cs
var json = @"{
      'FullName': 'Dan Deleted',
      'Deleted': true,
      'DeletedDate': '2013-01-20T00:00:00'
    }";

var traceWriter = new MemoryTraceWriter();

var account = JsonConvert.DeserializeObject<Account>(json, new JsonSerializerSettings
{
    TraceWriter = traceWriter
});

Console.WriteLine(traceWriter.ToString());
// Info Started deserializing Argon.Tests.Documentation.Examples.TraceWriter+Account. Path 'FullName', line 2, position 20.
// Verbose Could not find member 'DeletedDate' on Tests.Documentation.Examples.TraceWriter+Account. Path 'DeletedDate', line 4, position 23.
// Info Finished deserializing Argon.Tests.Documentation.Examples.TraceWriter+Account. Path '', line 5, position 8.
// Verbose Deserialized JSON:
// {
//   "FullName": "Dan Deleted",
//   "Deleted": true,
//   "DeletedDate": "2013-01-20T00:00:00"
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/TraceWriter.cs#L20-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-tracewriterusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
