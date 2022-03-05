// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Argon.Tests.Documentation;

public class CustomTraceWriter : TestFixtureBase
{
    #region CustomTraceWriterTypes

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

    [Fact]
    public void Example()
    {
        #region CustomTraceWriterUsage

        var countries = new List<string>
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

        #endregion

        XUnitAssert.AreEqualNormalized(@"[
  ""New Zealand"",
  ""Australia"",
  ""Denmark"",
  ""China""
]", json);
    }
}