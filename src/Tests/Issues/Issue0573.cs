// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue0573
{
    [Fact]
    public void Test()
    {
        var json = "{'Value':'hi'}";
        var traceWriter = new MemoryTraceWriter { LevelFilter = TraceLevel.Info };
        var o = JsonConvert.DeserializeObject<PrivateSetterTestClass>(json, new JsonSerializerSettings
        {
            TraceWriter = traceWriter
        });
        var messages = traceWriter.GetTraceMessages().ToList();

        var hasMessage = messages.Any(message => message.Contains("Info Unable to deserialize value to non-writable property 'Value' on Issue0573+PrivateSetterTestClass. Path 'Value', line 1, position 13."));
        Assert.True(hasMessage);
    }

    public class PrivateSetterTestClass
    {
        public string Value { get; private set; }
    }
}