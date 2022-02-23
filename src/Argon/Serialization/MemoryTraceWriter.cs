namespace Argon;

/// <summary>
/// Represents a trace writer that writes to memory. When the trace message limit is
/// reached then old trace messages will be removed as new messages are added.
/// </summary>
public class MemoryTraceWriter : ITraceWriter
{
    readonly Queue<string> traceMessages;
    readonly object @lock;

    /// <summary>
    /// Gets the <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
    /// For example a filter level of <see cref="TraceLevel.Info"/> will exclude <see cref="TraceLevel.Verbose"/> messages and include <see cref="TraceLevel.Info"/>,
    /// <see cref="TraceLevel.Warning"/> and <see cref="TraceLevel.Error"/> messages.
    /// </summary>
    public TraceLevel LevelFilter { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTraceWriter"/> class.
    /// </summary>
    public MemoryTraceWriter()
    {
        LevelFilter = TraceLevel.Verbose;
        traceMessages = new Queue<string>();
        @lock = new object();
    }

    /// <summary>
    /// Writes the specified trace level, message and optional exception.
    /// </summary>
    /// <param name="level">The <see cref="TraceLevel"/> at which to write this trace.</param>
    /// <param name="message">The trace message.</param>
    /// <param name="ex">The trace exception. This parameter is optional.</param>
    public void Trace(TraceLevel level, string message, Exception? ex)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
        stringBuilder.Append(" ");
        stringBuilder.Append(level.ToString("g"));
        stringBuilder.Append(" ");
        stringBuilder.Append(message);

        var s = stringBuilder.ToString();

        lock (@lock)
        {
            if (traceMessages.Count >= 1000)
            {
                traceMessages.Dequeue();
            }

            traceMessages.Enqueue(s);
        }
    }

    /// <summary>
    /// Returns an enumeration of the most recent trace messages.
    /// </summary>
    /// <returns>An enumeration of the most recent trace messages.</returns>
    public IEnumerable<string> GetTraceMessages()
    {
        return traceMessages;
    }

    /// <summary>
    /// Returns a <see cref="String"/> of the most recent trace messages.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> of the most recent trace messages.
    /// </returns>
    public override string ToString()
    {
        lock (@lock)
        {
            var stringBuilder = new StringBuilder();
            foreach (var traceMessage in traceMessages)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }

                stringBuilder.Append(traceMessage);
            }

            return stringBuilder.ToString();
        }
    }
}