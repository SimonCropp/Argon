// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation;

public class LogEventInfo
{
    public LogLevel Level;
    public string Message;
    public Exception Exception;
}

public class LogLevel
{
    public static LogLevel Info;
    public static LogLevel Trace;
    public static LogLevel Error;
    public static LogLevel Warn;
    public static LogLevel Off;
}

public class Logger
{
    public void Log(LogEventInfo logEvent)
    {
    }
}

public static class LogManager
{
    public static Logger GetLogger(string className)
    {
        return new Logger();
    }
}

public class TraceWriterTests : TestFixtureBase
{
    #region CustomTraceWriterExample

    public class NLogTraceWriter : ITraceWriter
    {
        static readonly Logger Logger = LogManager.GetLogger("NLogTraceWriter");

        public TraceLevel LevelFilter =>
            // trace all messages. nlog can handle filtering
            TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception exception)
        {
            var logEvent = new LogEventInfo
            {
                Message = message,
                Level = GetLogLevel(level),
                Exception = exception
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

    #endregion

    public class Staff
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public IList<string> Roles { get; set; }
    }

    [Fact]
    public Task MemoryTraceWriterTest()
    {
        #region MemoryTraceWriterExample

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

        #endregion

        var memoryTraceWriter = (MemoryTraceWriter) traceWriter;

        var lines = memoryTraceWriter.GetTraceMessages().Select(x => x[24..]);
        return Verify(string.Join(Environment.NewLine, lines));
    }
}