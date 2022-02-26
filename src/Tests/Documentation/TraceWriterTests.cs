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
    #endregion


    public class Staff
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public IList<string> Roles { get; set; }
    }

    [Fact]
    public void MemoryTraceWriterTest()
    {
        #region MemoryTraceWriterExample
        var staff = new Staff
        {
            Name = "Arnie Admin",
            Roles = new List<string> { "Administrator" },
            StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)
        };

        ITraceWriter traceWriter = new MemoryTraceWriter();

        JsonConvert.SerializeObject(
            staff,
            new JsonSerializerSettings { TraceWriter = traceWriter, Converters = { new JavaScriptDateTimeConverter() } });

        Console.WriteLine(traceWriter);
        // 2012-11-11T12:08:42.761 Info Started serializing Argon.Tests.Serialization.Staff. Path ''.
        // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path 'StartDate'.
        // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path 'StartDate'.
        // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
        // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
        // 2012-11-11T12:08:42.799 Info Finished serializing Argon.Tests.Serialization.Staff. Path ''.
        // 2013-05-18T21:38:11.255 Verbose Serialized JSON:
        // {
        //   "Name": "Arnie Admin",
        //   "StartDate": new Date(
        //     976623132000
        //   ),
        //   "Roles": [
        //     "Administrator"
        //   ]
        // }
        #endregion

        var memoryTraceWriter = (MemoryTraceWriter)traceWriter;

        Assert.Equal(7, memoryTraceWriter.GetTraceMessages().Count());
    }
}