﻿using DiagnosticsTrace = System.Diagnostics.Trace;

namespace Argon;

/// <summary>
/// Represents a trace writer that writes to the application's <see cref="TraceListener" /> instances.
/// </summary>
public class DiagnosticsTraceWriter : ITraceWriter
{
    /// <summary>
    /// Gets the <see cref="TraceLevel" /> that will be used to filter the trace messages passed to the writer.
    /// For example a filter level of <see cref="TraceLevel.Info" /> will exclude <see cref="TraceLevel.Verbose" /> messages and include <see cref="TraceLevel.Info" />,
    /// <see cref="TraceLevel.Warning" /> and <see cref="TraceLevel.Error" /> messages.
    /// </summary>
    public TraceLevel LevelFilter { get; set; }

    static TraceEventType GetTraceEventType(TraceLevel level)
    {
        switch (level)
        {
            case TraceLevel.Error:
                return TraceEventType.Error;
            case TraceLevel.Warning:
                return TraceEventType.Warning;
            case TraceLevel.Info:
                return TraceEventType.Information;
            case TraceLevel.Verbose:
                return TraceEventType.Verbose;
            default:
                throw new ArgumentOutOfRangeException(nameof(level));
        }
    }

    /// <summary>
    /// Writes the specified trace level, message and optional exception.
    /// </summary>
    /// <param name="level">The <see cref="TraceLevel" /> at which to write this trace.</param>
    /// <param name="message">The trace message.</param>
    /// <param name="exception">The trace exception. This parameter is optional.</param>
    public void Trace(TraceLevel level, string message, Exception? exception)
    {
        if (level == TraceLevel.Off)
        {
            return;
        }

        var eventCache = new TraceEventCache();
        var traceEventType = GetTraceEventType(level);

        foreach (TraceListener listener in DiagnosticsTrace.Listeners)
        {
            if (listener.IsThreadSafe)
            {
                listener.TraceEvent(eventCache, "Argon", traceEventType, 0, message);
            }
            else
            {
                lock (listener)
                {
                    listener.TraceEvent(eventCache, "Argon", traceEventType, 0, message);
                }
            }

            if (DiagnosticsTrace.AutoFlush)
            {
                listener.Flush();
            }
        }
    }
}