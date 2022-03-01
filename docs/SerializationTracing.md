# Debugging with Serialization Tracing

The Json.NET serializer supports logging and debugging using the `Argon.Serialization.ITraceWriter` interface. By assigning a trace writer you can capture serialization messages and errors and debug what happens inside the Json.NET serializer when serializing and deserializing JSON.


## ITraceWriter

A trace writer can be assigned using properties on JsonSerializerSettings or JsonSerializer.

<!-- snippet: MemoryTraceWriterExample -->
<a id='snippet-memorytracewriterexample'></a>
```cs
var staff = new Staff
{
    Name = "Arnie Admin",
    Roles = new List<string> {"Administrator"},
    StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)
};

ITraceWriter traceWriter = new MemoryTraceWriter();

JsonConvert.SerializeObject(
    staff,
    new JsonSerializerSettings {TraceWriter = traceWriter});
```
<sup><a href='/src/Tests/Documentation/TraceWriterTests.cs#L93-L108' title='Snippet source file'>snippet source</a> | <a href='#snippet-memorytracewriterexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Results in:

<!-- snippet: TraceWriterTests.MemoryTraceWriterTest.verified.txt -->
<a id='snippet-TraceWriterTests.MemoryTraceWriterTest.verified.txt'></a>
```txt
Info Started serializing Argon.Tests.Documentation.TraceWriterTests+Staff. Path ''.
Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
Info Finished serializing Argon.Tests.Documentation.TraceWriterTests+Staff. Path ''.
Verbose Serialized JSON: 
{
  "Name": "Arnie Admin",
  "StartDate": "2000-12-12T12:12:12Z",
  "Roles": [
    "Administrator"
  ]
}
```
<sup><a href='/src/Tests/Documentation/TraceWriterTests.MemoryTraceWriterTest.verified.txt#L1-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-TraceWriterTests.MemoryTraceWriterTest.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Json.NET has two implementations of ITraceWriter: `Argon.Serialization.MemoryTraceWriter`, which keeps messages in memory for simple debugging, like the example above, and `Argon.Serialization.DiagnosticsTraceWriter`, which writes messages to any System.Diagnostics.TraceListeners your application is using.


## Custom ITraceWriter

To write messages using your existing logging framework, just implement a custom version of ITraceWriter.

<!-- snippet: CustomTraceWriterExample -->
<a id='snippet-customtracewriterexample'></a>
```cs
public class NLogTraceWriter : ITraceWriter
{
    static readonly Logger Logger = LogManager.GetLogger("NLogTraceWriter");

    public TraceLevel LevelFilter =>
        // trace all messages. nlog can handle filtering
        TraceLevel.Verbose;

    public void Trace(TraceLevel level, string message, Exception ex)
    {
        var logEvent = new LogEventInfo
        {
            Message = message,
            Level = GetLogLevel(level),
            Exception = ex
        };

        // log Json.NET message to NLog
        Logger.Log(logEvent);
    }

    static LogLevel GetLogLevel(TraceLevel level)
    {
        switch (level)
        {
            case TraceLevel.Error:
                return LogLevel.Error;
            case TraceLevel.Warning:
                return LogLevel.Warn;
            case TraceLevel.Info:
                return LogLevel.Info;
            case TraceLevel.Off:
                return LogLevel.Off;
            default:
                return LogLevel.Trace;
        }
    }
}
```
<sup><a href='/src/Tests/Documentation/TraceWriterTests.cs#L40-L81' title='Snippet source file'>snippet source</a> | <a href='#snippet-customtracewriterexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.JsonSerializer`
 * `Argon.Serialization.ITraceWriter`
 * `Argon.Serialization.MemoryTraceWriter`
 * `Argon.Serialization.DiagnosticsTraceWriter`
