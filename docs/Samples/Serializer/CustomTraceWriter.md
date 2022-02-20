# Custom ITraceWriter

This sample creates a custom `Argon.Serialization.ITraceWriter` that writes to [NLog](http://nlog-project.org/)

<!-- snippet: CustomTraceWriterTypes -->
<a id='snippet-customtracewritertypes'></a>
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
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/CustomTraceWriter.cs#L32-L71' title='Snippet source file'>snippet source</a> | <a href='#snippet-customtracewritertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: CustomTraceWriterUsage -->
<a id='snippet-customtracewriterusage'></a>
```cs
IList<string> countries = new List<string>
{
    "New Zealand",
    "Australia",
    "Denmark",
    "China"
};

var json = JsonConvert.SerializeObject(countries, Formatting.Indented, new JsonSerializerSettings
{
    TraceWriter = new NLogTraceWriter()
});

Console.WriteLine(json);
// [
//   "New Zealand",
//   "Australia",
//   "Denmark",
//   "China"
// ]
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/CustomTraceWriter.cs#L76-L97' title='Snippet source file'>snippet source</a> | <a href='#snippet-customtracewriterusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
