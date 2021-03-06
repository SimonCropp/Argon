// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class TraceWriter : TestFixtureBase
{
    #region TraceWriterTypes

    public class Account
    {
        public string FullName { get; set; }
        public bool Deleted { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region TraceWriterUsage

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

        #endregion

        Assert.Equal(4, traceWriter.GetTraceMessages().Count());
    }
}