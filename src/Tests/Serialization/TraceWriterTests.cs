using TestObjects;

// ReSharper disable UseObjectOrCollectionInitializer

namespace Argon.Tests.Serialization;

public class Staff
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public IList<string> Roles { get; set; }
}

public class RoleTrace
{
    public string Name { get; set; }
}

public class TraceWriterTests : TestFixtureBase
{
    [Fact]
    public void DeserializedJsonWithAlreadyReadReader()
    {
        var json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";
        IList<RoleTrace> roles = new List<RoleTrace>();
        var reader = new JsonTextReader(new StringReader(json));
        reader.SupportMultipleContent = true;
        var traceWriter = new InMemoryTraceWriter();
        while (true)
        {
            if (!reader.Read())
            {
                break;
            }
            var serializer = new JsonSerializer
            {
                //the next line raise an exception
                TraceWriter = traceWriter
            };
            var role = serializer.Deserialize<RoleTrace>(reader);
            roles.Add(role);
        }

        Assert.Equal("Admin", roles[0].Name);
        Assert.Equal("Publisher", roles[1].Name);

        XUnitAssert.AreEqualNormalized(@"Deserialized JSON: 
{
  ""name"": ""Admin""
}", traceWriter.TraceRecords[2].Message);

        XUnitAssert.AreEqualNormalized(@"Deserialized JSON: 
{
  ""name"": ""Publisher""
}", traceWriter.TraceRecords[5].Message);
    }

    [Fact]
    public async Task DeserializedJsonWithAlreadyReadReader_Async()
    {
        var json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";
        IList<RoleTrace> roles = new List<RoleTrace>();
        var reader = new JsonTextReader(new StringReader(json));
        reader.SupportMultipleContent = true;
        var traceWriter = new InMemoryTraceWriter();
        while (true)
        {
            if (!await reader.ReadAsync())
            {
                break;
            }
            var serializer = new JsonSerializer
            {
                //the next line raise an exception
                TraceWriter = traceWriter
            };
            var role = serializer.Deserialize<RoleTrace>(reader);
            roles.Add(role);
        }

        Assert.Equal("Admin", roles[0].Name);
        Assert.Equal("Publisher", roles[1].Name);

        XUnitAssert.AreEqualNormalized(@"Deserialized JSON: 
{
  ""name"": ""Admin""
}", traceWriter.TraceRecords[2].Message);

        XUnitAssert.AreEqualNormalized(@"Deserialized JSON: 
{
  ""name"": ""Publisher""
}", traceWriter.TraceRecords[5].Message);
    }

    [Fact]
    public void DiagnosticsTraceWriterTest()
    {
        var stringWriter = new StringWriter();
        var listener = new TextWriterTraceListener(stringWriter);

        try
        {
            Trace.AutoFlush = true;
            Trace.Listeners.Add(listener);

            var traceWriter = new DiagnosticsTraceWriter();
            traceWriter.Trace(TraceLevel.Verbose, "Verbose!", null);
            traceWriter.Trace(TraceLevel.Info, "Info!", null);
            traceWriter.Trace(TraceLevel.Warning, "Warning!", null);
            traceWriter.Trace(TraceLevel.Error, "Error!", null);
            traceWriter.Trace(TraceLevel.Off, "Off!", null);

            XUnitAssert.AreEqualNormalized(@"Argon Verbose: 0 : Verbose!
Argon Information: 0 : Info!
Argon Warning: 0 : Warning!
Argon Error: 0 : Error!
", stringWriter.ToString());
        }
        finally
        {
            Trace.Listeners.Remove(listener);
            Trace.AutoFlush = false;
        }
    }

    [Fact]
    public void WriteNullableByte()
    {
        var stringWriter = new StringWriter();
        var traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(stringWriter));
        traceJsonWriter.WriteStartArray();
        traceJsonWriter.WriteValue((byte?)null);
        traceJsonWriter.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public void WriteNullObject()
    {
        var stringWriter = new StringWriter();
        var traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(stringWriter));
        traceJsonWriter.WriteStartArray();
        traceJsonWriter.WriteValue((object)null);
        traceJsonWriter.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public void WriteNullString()
    {
        var stringWriter = new StringWriter();
        var traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(stringWriter));
        traceJsonWriter.WriteStartArray();
        traceJsonWriter.WriteValue((string)null);
        traceJsonWriter.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public void WriteNullUri()
    {
        var stringWriter = new StringWriter();
        var traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(stringWriter));
        traceJsonWriter.WriteStartArray();
        traceJsonWriter.WriteValue((Uri)null);
        traceJsonWriter.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public void WriteNullByteArray()
    {
        var stringWriter = new StringWriter();
        var traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(stringWriter));
        traceJsonWriter.WriteStartArray();
        traceJsonWriter.WriteValue((byte[])null);
        traceJsonWriter.WriteEndArray();

        XUnitAssert.AreEqualNormalized(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public void WriteJRaw()
    {
        ITraceWriter traceWriter = new MemoryTraceWriter();

        var settings = new JRaw("$('#element')");
        var json = JsonConvert.SerializeObject(settings, new JsonSerializerSettings
        {
            TraceWriter = traceWriter
        });

        Assert.Equal("$('#element')", json);

        Assert.True(traceWriter.ToString().EndsWith($"Verbose Serialized JSON: {Environment.NewLine}$('#element')", StringComparison.Ordinal));
    }

    [Fact]
    public void WriteJRawInArray()
    {
        ITraceWriter traceWriter = new MemoryTraceWriter();

        var raws = new List<JRaw>
        {
            new("$('#element')"),
            new("$('#element')"),
            new("$('#element')")
        };

        var json = JsonConvert.SerializeObject(raws, new JsonSerializerSettings
        {
            TraceWriter = traceWriter,
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized(@"[
  $('#element'),
  $('#element'),
  $('#element')
]", json);

        Assert.True(XUnitAssert.Normalize(traceWriter.ToString()).EndsWith(XUnitAssert.Normalize(@"Verbose Serialized JSON: 
[
  $('#element'),
  $('#element'),
  $('#element')
]"), StringComparison.Ordinal));
    }

    [Fact]
    public void MemoryTraceWriterSerializeTest()
    {
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

        // 2012-11-11T12:08:42.761 Info Started serializing Argon.Tests.Serialization.Staff. Path ''.
        // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path 'StartDate'.
        // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path 'StartDate'.
        // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
        // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
        // 2012-11-11T12:08:42.799 Info Finished serializing Argon.Tests.Serialization.Staff. Path ''.

        var memoryTraceWriter = (MemoryTraceWriter)traceWriter;
        var output = memoryTraceWriter.ToString();

        Assert.Equal(854, output.Length);
        Assert.Equal(7, memoryTraceWriter.GetTraceMessages().Count());

        var json = @"Serialized JSON: 
{
  ""Name"": ""Arnie Admin"",
  ""StartDate"": new Date(
    976623132000
  ),
  ""Roles"": [
    ""Administrator""
  ]
}";

        json = XUnitAssert.Normalize(json);
        output = XUnitAssert.Normalize(output);

        Assert.True(output.Contains(json));
    }

    [Fact]
    public void MemoryTraceWriterDeserializeTest()
    {
        var json = @"{
  ""Name"": ""Arnie Admin"",
  ""StartDate"": new Date(
    976623132000
  ),
  ""Roles"": [
    ""Administrator""
  ]
}";

        var staff = new Staff
        {
            Name = "Arnie Admin",
            Roles = new List<string> { "Administrator" },
            StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)
        };

        ITraceWriter traceWriter = new MemoryTraceWriter();

        JsonConvert.DeserializeObject<Staff>(
            json,
            new JsonSerializerSettings
            {
                TraceWriter = traceWriter,
                Converters = { new JavaScriptDateTimeConverter() },
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            });

        // 2012-11-11T12:08:42.761 Info Started serializing Argon.Tests.Serialization.Staff. Path ''.
        // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path 'StartDate'.
        // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter TestObjects.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
        // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
        // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
        // 2012-11-11T12:08:42.799 Info Finished serializing Argon.Tests.Serialization.Staff. Path ''.
        // 2013-05-19T00:07:24.360 Verbose Deserialized JSON:
        // {
        //   "Name": "Arnie Admin",
        //   "StartDate": new Date(
        //     976623132000
        //   ),
        //   "Roles": [
        //     "Administrator"
        //   ]
        // }

        var memoryTraceWriter = (MemoryTraceWriter)traceWriter;
        var output = memoryTraceWriter.ToString();

        Assert.Equal(996, output.Length);
        Assert.Equal(7, memoryTraceWriter.GetTraceMessages().Count());

        json = XUnitAssert.Normalize(json);
        output = XUnitAssert.Normalize(output);

        Assert.True(output.Contains(json));
    }

    [Fact]
    public void MemoryTraceWriterLimitTest()
    {
        var traceWriter = new MemoryTraceWriter();

        for (var i = 0; i < 1005; i++)
        {
            traceWriter.Trace(TraceLevel.Verbose, (i + 1).ToString(CultureInfo.InvariantCulture), null);
        }

        IList<string> traceMessages = traceWriter.GetTraceMessages().ToList();

        Assert.Equal(1000, traceMessages.Count);

        Assert.True(traceMessages.First().EndsWith(" 6"));
        Assert.True(traceMessages.Last().EndsWith(" 1005"));
    }

    [Fact]
    public async Task MemoryTraceWriterThreadSafety_Trace()
    {
        var tasks = new List<Task>();

        var traceWriter = new MemoryTraceWriter();

        for (var i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < 1005; j++)
                {
                    traceWriter.Trace(TraceLevel.Verbose, (j + 1).ToString(CultureInfo.InvariantCulture), null);
                }
            }));
        }

        await Task.WhenAll(tasks);

        IList<string> traceMessages = traceWriter.GetTraceMessages().ToList();

        Assert.Equal(1000, traceMessages.Count);
    }

    [Fact]
    public async Task MemoryTraceWriterThreadSafety_ToString()
    {
        var tasks = new List<Task>();

        var traceWriter = new MemoryTraceWriter();

        tasks.Add(Task.Run(() =>
        {
            for (var j = 0; j < 10005; j++)
            {
                traceWriter.Trace(TraceLevel.Verbose, (j + 1).ToString(CultureInfo.InvariantCulture), null);
            }
        }));

        string s = null;

        tasks.Add(Task.Run(() =>
        {
            for (var j = 0; j < 10005; j++)
            {
                s = traceWriter.ToString();
            }
        }));

        await Task.WhenAll(tasks);

        Assert.NotNull(s);
    }

    [Fact]
    public void Serialize()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Info
        };

        var json =
            JsonConvert.SerializeObject(
                new TraceTestObject
                {
                    StringArray = new[] { "1", "2" },
                    IntList = new List<int> { 1, 2 },
                    Version = new VersionOld(1, 2, 3, 4),
                    StringDictionary =
                        new Dictionary<string, string>
                        {
                            { "1", "!" },
                            { "Two", "!!" },
                            { "III", "!!!" }
                        },
                    Double = 1.1d
                },
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter,
                    Formatting = Formatting.Indented
                });

        Assert.Equal("Started serializing Argon.Tests.Serialization.TraceTestObject. Path ''.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Started serializing System.Collections.Generic.List`1[System.Int32]. Path 'IntList'.", traceWriter.TraceRecords[1].Message);
        Assert.Equal("Finished serializing System.Collections.Generic.List`1[System.Int32]. Path 'IntList'.", traceWriter.TraceRecords[2].Message);
        Assert.Equal("Started serializing System.String[]. Path 'StringArray'.", traceWriter.TraceRecords[3].Message);
        Assert.Equal("Finished serializing System.String[]. Path 'StringArray'.", traceWriter.TraceRecords[4].Message);
        Assert.Equal("Started serializing TestObjects.VersionOld. Path 'Version'.", traceWriter.TraceRecords[5].Message);
        Assert.Equal("Finished serializing TestObjects.VersionOld. Path 'Version'.", traceWriter.TraceRecords[6].Message);
        Assert.Equal("Started serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path 'StringDictionary'.", traceWriter.TraceRecords[7].Message);
        Assert.Equal("Finished serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path 'StringDictionary'.", traceWriter.TraceRecords[8].Message);
        Assert.Equal("Finished serializing Argon.Tests.Serialization.TraceTestObject. Path ''.", traceWriter.TraceRecords[9].Message);

        Assert.False(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
    }

    [Fact]
    public void Deserialize()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Info
        };

        var o2 = JsonConvert.DeserializeObject<TraceTestObject>(
            @"{
  ""IntList"": [
    1,
    2
  ],
  ""StringArray"": [
    ""1"",
    ""2""
  ],
  ""Version"": {
    ""Major"": 1,
    ""Minor"": 2,
    ""Build"": 3,
    ""Revision"": 4,
    ""MajorRevision"": 0,
    ""MinorRevision"": 4
  },
  ""StringDictionary"": {
    ""1"": ""!"",
    ""Two"": ""!!"",
    ""III"": ""!!!""
  },
  ""Double"": 1.1
}",
            new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

        Assert.Equal(2, o2.IntList.Count);
        Assert.Equal(2, o2.StringArray.Length);
        Assert.Equal(1, o2.Version.Major);
        Assert.Equal(2, o2.Version.Minor);
        Assert.Equal(3, o2.StringDictionary.Count);
        Assert.Equal(1.1d, o2.Double);

        Assert.Equal("Started deserializing Argon.Tests.Serialization.TraceTestObject. Path 'IntList', line 2, position 12.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 2, position 14.", traceWriter.TraceRecords[1].Message);
        Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList'"));
        Assert.Equal("Started deserializing System.String[]. Path 'StringArray', line 6, position 18.", traceWriter.TraceRecords[3].Message);
        Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.String[]. Path 'StringArray'"));
        Assert.Equal("Deserializing TestObjects.VersionOld using creator with parameters: Major, Minor, Build, Revision. Path 'Version.Major', line 11, position 12.", traceWriter.TraceRecords[5].Message);
        Assert.True(traceWriter.TraceRecords[6].Message.StartsWith("Started deserializing TestObjects.VersionOld. Path 'Version'"));
        Assert.True(traceWriter.TraceRecords[7].Message.StartsWith("Finished deserializing TestObjects.VersionOld. Path 'Version'"));
        Assert.Equal("Started deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary.1', line 19, position 8.", traceWriter.TraceRecords[8].Message);
        Assert.True(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary'"));
        Assert.True(traceWriter.TraceRecords[10].Message.StartsWith("Finished deserializing Argon.Tests.Serialization.TraceTestObject. Path ''"));

        Assert.False(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
    }

    [Fact]
    public void Populate()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Info
        };

        var o2 = new TraceTestObject();

        JsonConvert.PopulateObject(@"{
  ""IntList"": [
    1,
    2
  ],
  ""StringArray"": [
    ""1"",
    ""2""
  ],
  ""Version"": {
    ""Major"": 1,
    ""Minor"": 2,
    ""Build"": 3,
    ""Revision"": 4,
    ""MajorRevision"": 0,
    ""MinorRevision"": 4
  },
  ""StringDictionary"": {
    ""1"": ""!"",
    ""Two"": ""!!"",
    ""III"": ""!!!""
  },
  ""Double"": 1.1
}",
            o2,
            new JsonSerializerSettings
            {
                TraceWriter = traceWriter,
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            });

        Assert.Equal(2, o2.IntList.Count);
        Assert.Equal(2, o2.StringArray.Length);
        Assert.Equal(1, o2.Version.Major);
        Assert.Equal(2, o2.Version.Minor);
        Assert.Equal(3, o2.StringDictionary.Count);
        Assert.Equal(1.1d, o2.Double);

        Assert.Equal("Started deserializing Argon.Tests.Serialization.TraceTestObject. Path 'IntList', line 2, position 12.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 2, position 14.", traceWriter.TraceRecords[1].Message);
        Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList'"));
        Assert.Equal("Started deserializing System.String[]. Path 'StringArray', line 6, position 18.", traceWriter.TraceRecords[3].Message);
        Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.String[]. Path 'StringArray'"));
        Assert.Equal("Deserializing TestObjects.VersionOld using creator with parameters: Major, Minor, Build, Revision. Path 'Version.Major', line 11, position 12.", traceWriter.TraceRecords[5].Message);
        Assert.True(traceWriter.TraceRecords[6].Message.StartsWith("Started deserializing TestObjects.VersionOld. Path 'Version'"));
        Assert.True(traceWriter.TraceRecords[7].Message.StartsWith("Finished deserializing TestObjects.VersionOld. Path 'Version'"));
        Assert.Equal("Started deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary.1', line 19, position 8.", traceWriter.TraceRecords[8].Message);
        Assert.True(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary'"));
        Assert.True(traceWriter.TraceRecords[10].Message.StartsWith("Finished deserializing Argon.Tests.Serialization.TraceTestObject. Path ''"));

        Assert.False(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
    }

    [Fact]
    public void ErrorDeserializing()
    {
        var json = @"{""Integer"":""hi""}";

        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Info
        };

        XUnitAssert.Throws<Exception>(() =>
        {
            JsonConvert.DeserializeObject<IntegerTestClass>(
                json,
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter
                });
        }, "Could not convert string to integer: hi. Path 'Integer', line 1, position 15.");

        Assert.Equal(2, traceWriter.TraceRecords.Count);

        Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
        Assert.Equal("Started deserializing Argon.Tests.Serialization.IntegerTestClass. Path 'Integer', line 1, position 11.", traceWriter.TraceRecords[0].Message);

        Assert.Equal(TraceLevel.Error, traceWriter.TraceRecords[1].Level);
        Assert.Equal("Error deserializing Argon.Tests.Serialization.IntegerTestClass. Could not convert string to integer: hi. Path 'Integer', line 1, position 15.", traceWriter.TraceRecords[1].Message);
    }

    [Fact]
    public void ErrorDeserializingNested()
    {
        var json = @"{""IntList"":[1, ""two""]}";

        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Info
        };

        XUnitAssert.Throws<Exception>(() =>
        {
            JsonConvert.DeserializeObject<TraceTestObject>(
                json,
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter
                });
        }, "Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.");

        Assert.Equal(3, traceWriter.TraceRecords.Count);

        Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
        Assert.Equal("Started deserializing Argon.Tests.Serialization.TraceTestObject. Path 'IntList', line 1, position 11.", traceWriter.TraceRecords[0].Message);

        Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[1].Level);
        Assert.Equal("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 1, position 12.", traceWriter.TraceRecords[1].Message);

        Assert.Equal(TraceLevel.Error, traceWriter.TraceRecords[2].Level);
        Assert.Equal("Error deserializing System.Collections.Generic.IList`1[System.Int32]. Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.", traceWriter.TraceRecords[2].Message);
    }

    [Fact]
    public void SerializeDictionarysWithPreserveObjectReferences()
    {
        var circularDictionary = new PreserveReferencesHandlingTests.CircularDictionary();
        circularDictionary.Add("other", new PreserveReferencesHandlingTests.CircularDictionary { { "blah", null } });
        circularDictionary.Add("self", circularDictionary);

        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        JsonConvert.SerializeObject(
            circularDictionary,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TraceWriter = traceWriter
            });

        Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference Id '1' for Argon.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path ''."));
        Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference Id '2' for Argon.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other'."));
        Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference to Id '1' for Argon.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'self'."));
    }

    [Fact]
    public void DeserializeDictionarysWithPreserveObjectReferences()
    {
        var json = @"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}";

        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        JsonConvert.DeserializeObject<PreserveReferencesHandlingTests.CircularDictionary>(json,
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                TraceWriter = traceWriter
            });

        Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '1' for Argon.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other', line 3, position 10."));
        Assert.True(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '2' for Argon.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other.blah', line 5, position 11."));
        Assert.True(traceWriter.TraceRecords.Any(r => r.Message.StartsWith("Resolved object reference '1' to Argon.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'self'")));
    }

    [Fact]
    public void WriteTypeNameForObjects()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        IList<object> l = new List<object>
        {
            new Dictionary<string, string> { { "key!", "value!" } },
            new VersionOld(1, 2, 3, 4)
        };

        JsonConvert.SerializeObject(l, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            TraceWriter = traceWriter
        });

        Assert.Equal("Started serializing System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[0].Message);
        Assert.Equal($"Writing type name '{ReflectionUtils.GetTypeName(typeof(List<object>), 0, DefaultSerializationBinder.Instance)}' for System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[1].Message);
        Assert.Equal("Started serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values'.", traceWriter.TraceRecords[2].Message);
        Assert.Equal($"Writing type name '{ReflectionUtils.GetTypeName(typeof(Dictionary<string, string>), 0, DefaultSerializationBinder.Instance)}' for System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[3].Message);
        Assert.Equal("Finished serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[4].Message);
        Assert.Equal("Started serializing TestObjects.VersionOld. Path '$values[0]'.", traceWriter.TraceRecords[5].Message);
        Assert.Equal($"Writing type name '{ReflectionUtils.GetTypeName(typeof(VersionOld), 0, DefaultSerializationBinder.Instance)}' for TestObjects.VersionOld. Path '$values[1]'.", traceWriter.TraceRecords[6].Message);
        Assert.Equal("Finished serializing TestObjects.VersionOld. Path '$values[1]'.", traceWriter.TraceRecords[7].Message);
        Assert.Equal("Finished serializing System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[8].Message);
    }

    [Fact]
    public void SerializeConverter()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        IList<DateTime> d = new List<DateTime>
        {
            new(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)
        };

        var json = JsonConvert.SerializeObject(d, Formatting.Indented, new JsonSerializerSettings
        {
            Converters = { new JavaScriptDateTimeConverter() },
            TraceWriter = traceWriter
        });

        Assert.Equal("Started serializing System.Collections.Generic.List`1[System.DateTime]. Path ''.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Started serializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path ''.", traceWriter.TraceRecords[1].Message);
        Assert.Equal("Finished serializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path '[0]'.", traceWriter.TraceRecords[2].Message);
        Assert.Equal("Finished serializing System.Collections.Generic.List`1[System.DateTime]. Path ''.", traceWriter.TraceRecords[3].Message);
    }

    [Fact]
    public void DeserializeConverter()
    {
        var json = @"[new Date(976623132000)]";

        var traceWriter =
            new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

        JsonConvert.DeserializeObject<List<DateTime>>(
            json,
            new JsonSerializerSettings
            {
                Converters = { new JavaScriptDateTimeConverter() },
                TraceWriter = traceWriter
            });

        Assert.Equal("Started deserializing System.Collections.Generic.List`1[System.DateTime]. Path '', line 1, position 1.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Started deserializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path '[0]', line 1, position 10.", traceWriter.TraceRecords[1].Message);
        Assert.Equal("Finished deserializing System.DateTime with converter Argon.JavaScriptDateTimeConverter. Path '[0]', line 1, position 23.", traceWriter.TraceRecords[2].Message);
        Assert.Equal("Finished deserializing System.Collections.Generic.List`1[System.DateTime]. Path '', line 1, position 24.", traceWriter.TraceRecords[3].Message);
    }

    [Fact]
    public void DeserializeTypeName()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        var json = @"{
  ""$type"": ""System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib"",
  ""$values"": [
    {
      ""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib"",
      ""key!"": ""value!""
    },
    {
      ""$type"": ""TestObjects.VersionOld, Tests"",
      ""Major"": 1,
      ""Minor"": 2,
      ""Build"": 3,
      ""Revision"": 4,
      ""MajorRevision"": 0,
      ""MinorRevision"": 4
    }
  ]
}";

        JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            MetadataPropertyHandling = MetadataPropertyHandling.Default,
            TraceWriter = traceWriter
        });

        Assert.Equal("Resolved type 'System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib' to System.Collections.Generic.List`1[System.Object]. Path '$type', line 2, position 83.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Started deserializing System.Collections.Generic.List`1[System.Object]. Path '$values', line 3, position 14.", traceWriter.TraceRecords[1].Message);
        Assert.Equal("Resolved type 'System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib' to System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].$type', line 5, position 119.", traceWriter.TraceRecords[2].Message);
        Assert.Equal("Started deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].key!', line 6, position 13.", traceWriter.TraceRecords[3].Message);
        Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'"));
        Assert.Equal("Resolved type 'TestObjects.VersionOld, Tests' to TestObjects.VersionOld. Path '$values[1].$type', line 9, position 46.", traceWriter.TraceRecords[5].Message);
        Assert.Equal("Deserializing TestObjects.VersionOld using creator with parameters: Major, Minor, Build, Revision. Path '$values[1].Major', line 10, position 14.", traceWriter.TraceRecords[6].Message);
        Assert.True(traceWriter.TraceRecords[7].Message.StartsWith("Started deserializing TestObjects.VersionOld. Path '$values[1]'"));
        Assert.True(traceWriter.TraceRecords[8].Message.StartsWith("Finished deserializing TestObjects.VersionOld. Path '$values[1]'"));
        Assert.True(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.List`1[System.Object]. Path '$values'"));
    }

    [Fact]
    public void DeserializeISerializable()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        XUnitAssert.Throws<SerializationException>(() =>
            {
                JsonConvert.DeserializeObject<Exception>(
                    "{}",
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter
                    });
            },
            "Member 'ClassName' was not found.",
            "Member 'Message' was not found.");

        Assert.True(traceWriter.TraceRecords[0].Message.StartsWith("Deserializing System.Exception using ISerializable constructor. Path ''"));
        Assert.Equal(TraceLevel.Info, traceWriter.TraceRecords[0].Level);

#if !NET5_0_OR_GREATER
            Assert.Equal("Error deserializing System.Exception. Member 'ClassName' was not found. Path '', line 1, position 2.", traceWriter.TraceRecords[1].Message);
#else
        Assert.Equal("Error deserializing System.Exception. Member 'Message' was not found. Path '', line 1, position 2.", traceWriter.TraceRecords[1].Message);
#endif

        Assert.Equal(TraceLevel.Error, traceWriter.TraceRecords[1].Level);
    }

    [Fact]
    public void DeserializeMissingMember()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        JsonConvert.DeserializeObject<Person>(
            "{'MissingMemberProperty':'!!'}",
            new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

        Assert.Equal("Started deserializing TestObjects.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Could not find member 'MissingMemberProperty' on TestObjects.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[1].Message);
        Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing TestObjects.Person. Path ''"));
    }

    [Fact]
    public void DeserializeMissingMemberConstructor()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        var json = @"{
  ""Major"": 1,
  ""Minor"": 2,
  ""Build"": 3,
  ""Revision"": 4,
  ""MajorRevision"": 0,
  ""MinorRevision"": 4,
  ""MissingMemberProperty"": null
}";

        JsonConvert.DeserializeObject<VersionOld>(json, new JsonSerializerSettings
        {
            TraceWriter = traceWriter
        });

        Assert.Equal("Deserializing TestObjects.VersionOld using creator with parameters: Major, Minor, Build, Revision. Path 'Major', line 2, position 10.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("Could not find member 'MissingMemberProperty' on TestObjects.VersionOld. Path 'MissingMemberProperty', line 8, position 31.", traceWriter.TraceRecords[1].Message);
        Assert.True(traceWriter.TraceRecords[2].Message.StartsWith("Started deserializing TestObjects.VersionOld. Path ''"));
        Assert.True(traceWriter.TraceRecords[3].Message.StartsWith("Finished deserializing TestObjects.VersionOld. Path ''"));
    }

    [Fact]
    public void PublicParameterizedConstructorWithPropertyNameConflictWithAttribute()
    {
        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        var json = @"{name:""1""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorWithPropertyNameConflictWithAttribute>(json, new JsonSerializerSettings
        {
            TraceWriter = traceWriter
        });

        Assert.NotNull(c);
        Assert.Equal(1, c.Name);

        Assert.Equal("Deserializing TestObjects.PublicParameterizedConstructorWithPropertyNameConflictWithAttribute using creator with parameters: name. Path 'name', line 1, position 6.", traceWriter.TraceRecords[0].Message);
    }

    [Fact]
    public void ShouldSerializeTestClass()
    {
        var c = new ShouldSerializeTestClass
        {
            Age = 29,
            Name = "Jim",
            _shouldSerializeName = true
        };

        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        JsonConvert.SerializeObject(c, new JsonSerializerSettings { TraceWriter = traceWriter });

        Assert.Equal("ShouldSerialize result for property 'Name' on Argon.Tests.Serialization.ShouldSerializeTestClass: True. Path ''.", traceWriter.TraceRecords[1].Message);
        Assert.Equal(TraceLevel.Verbose, traceWriter.TraceRecords[1].Level);

        traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        c._shouldSerializeName = false;

        JsonConvert.SerializeObject(c, new JsonSerializerSettings { TraceWriter = traceWriter });

        Assert.Equal("ShouldSerialize result for property 'Name' on Argon.Tests.Serialization.ShouldSerializeTestClass: False. Path ''.", traceWriter.TraceRecords[1].Message);
        Assert.Equal(TraceLevel.Verbose, traceWriter.TraceRecords[1].Level);
    }

    [Fact]
    public void SpecifiedTest()
    {
        var c = new SpecifiedTestClass
        {
            Name = "James",
            Age = 27,
            NameSpecified = false
        };

        var traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings { TraceWriter = traceWriter });

        Assert.Equal("Started serializing Argon.Tests.Serialization.SpecifiedTestClass. Path ''.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("IsSpecified result for property 'Name' on Argon.Tests.Serialization.SpecifiedTestClass: False. Path ''.", traceWriter.TraceRecords[1].Message);
        Assert.Equal("IsSpecified result for property 'Weight' on Argon.Tests.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[2].Message);
        Assert.Equal("IsSpecified result for property 'Height' on Argon.Tests.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[3].Message);
        Assert.Equal("IsSpecified result for property 'FavoriteNumber' on Argon.Tests.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[4].Message);
        Assert.Equal("Finished serializing Argon.Tests.Serialization.SpecifiedTestClass. Path ''.", traceWriter.TraceRecords[5].Message);

        XUnitAssert.AreEqualNormalized(@"{
  ""Age"": 27
}", json);

        traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        var deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json, new JsonSerializerSettings { TraceWriter = traceWriter });

        Assert.Equal("Started deserializing Argon.Tests.Serialization.SpecifiedTestClass. Path 'Age', line 2, position 8.", traceWriter.TraceRecords[0].Message);
        Assert.True(traceWriter.TraceRecords[1].Message.StartsWith("Finished deserializing Argon.Tests.Serialization.SpecifiedTestClass. Path ''"));

        Assert.Null(deserialized.Name);
        Assert.False(deserialized.NameSpecified);
        Assert.False(deserialized.WeightSpecified);
        Assert.False(deserialized.HeightSpecified);
        Assert.False(deserialized.FavoriteNumberSpecified);
        Assert.Equal(27, deserialized.Age);

        c.NameSpecified = true;
        c.WeightSpecified = true;
        c.HeightSpecified = true;
        c.FavoriteNumber = 23;
        json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""James"",
  ""Age"": 27,
  ""Weight"": 0,
  ""Height"": 0,
  ""FavoriteNumber"": 23
}", json);

        traceWriter = new InMemoryTraceWriter
        {
            LevelFilter = TraceLevel.Verbose
        };

        deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json, new JsonSerializerSettings { TraceWriter = traceWriter });

        Assert.Equal("Started deserializing Argon.Tests.Serialization.SpecifiedTestClass. Path 'Name', line 2, position 9.", traceWriter.TraceRecords[0].Message);
        Assert.Equal("IsSpecified for property 'Name' on Argon.Tests.Serialization.SpecifiedTestClass set to true. Path 'Name', line 2, position 17.", traceWriter.TraceRecords[1].Message);
        Assert.Equal("IsSpecified for property 'Weight' on Argon.Tests.Serialization.SpecifiedTestClass set to true. Path 'Weight', line 4, position 13.", traceWriter.TraceRecords[2].Message);
        Assert.Equal("IsSpecified for property 'Height' on Argon.Tests.Serialization.SpecifiedTestClass set to true. Path 'Height', line 5, position 13.", traceWriter.TraceRecords[3].Message);
        Assert.True(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing Argon.Tests.Serialization.SpecifiedTestClass. Path ''"));

        Assert.Equal("James", deserialized.Name);
        Assert.True(deserialized.NameSpecified);
        Assert.True(deserialized.WeightSpecified);
        Assert.True(deserialized.HeightSpecified);
        Assert.True(deserialized.FavoriteNumberSpecified);
        Assert.Equal(27, deserialized.Age);
        Assert.Equal(23, deserialized.FavoriteNumber);
    }

    [Fact]
    public void TraceJsonWriterTest_WriteObjectInObject()
    {
        var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        var w = new JsonTextWriter(stringWriter);
        var traceWriter = new TraceJsonWriter(w);

        traceWriter.WriteStartObject();
        traceWriter.WritePropertyName("Prop1");
        traceWriter.WriteValue((object)1);
        traceWriter.WriteEndObject();
        traceWriter.Flush();
        traceWriter.Close();

        var json = @"{
  ""Prop1"": 1
}";

        XUnitAssert.AreEqualNormalized($"Serialized JSON: {Environment.NewLine}{json}", traceWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public async Task TraceJsonWriterTest_WriteObjectInObjectAsync()
    {
        var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        var w = new JsonTextWriter(stringWriter);
        var traceWriter = new TraceJsonWriter(w);

        await traceWriter.WriteStartObjectAsync();
        await traceWriter.WritePropertyNameAsync("Prop1");
        await traceWriter.WriteValueAsync((object)1);
        await traceWriter.WriteEndObjectAsync();
        await traceWriter.FlushAsync();
        traceWriter.Close();

        var json = @"{
  ""Prop1"": 1
}";

        XUnitAssert.AreEqualNormalized($"Serialized JSON: {Environment.NewLine}{json}", traceWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public void TraceJsonWriterTest()
    {
        var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        var w = new JsonTextWriter(stringWriter);
        var traceWriter = new TraceJsonWriter(w);

        traceWriter.WriteStartObject();
        traceWriter.WritePropertyName("Array");
        traceWriter.WriteStartArray();
        traceWriter.WriteValue("String!");
        traceWriter.WriteValue(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
        traceWriter.WriteValue(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
        traceWriter.WriteValue(1.1f);
        traceWriter.WriteValue(1.1d);
        traceWriter.WriteValue(1.1m);
        traceWriter.WriteValue(1);
        traceWriter.WriteValue('!');
        traceWriter.WriteValue((short)1);
        traceWriter.WriteValue((ushort)1);
        traceWriter.WriteValue(1);
        traceWriter.WriteValue((uint)1);
        traceWriter.WriteValue((sbyte)1);
        traceWriter.WriteValue((byte)1);
        traceWriter.WriteValue((long)1);
        traceWriter.WriteValue((ulong)1);
        traceWriter.WriteValue(true);

        traceWriter.WriteValue((DateTime?)new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
        traceWriter.WriteValue((DateTimeOffset?)new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
        traceWriter.WriteValue((float?)1.1f);
        traceWriter.WriteValue((double?)1.1d);
        traceWriter.WriteValue((decimal?)1.1m);
        traceWriter.WriteValue((int?)1);
        traceWriter.WriteValue((char?)'!');
        traceWriter.WriteValue((short?)1);
        traceWriter.WriteValue((ushort?)1);
        traceWriter.WriteValue((int?)1);
        traceWriter.WriteValue((uint?)1);
        traceWriter.WriteValue((sbyte?)1);
        traceWriter.WriteValue((byte?)1);
        traceWriter.WriteValue((long?)1);
        traceWriter.WriteValue((ulong?)1);
        traceWriter.WriteValue((bool?)true);
        traceWriter.WriteValue(BigInteger.Parse("9999999990000000000000000000000000000000000"));

        traceWriter.WriteValue((object)true);
        traceWriter.WriteValue(TimeSpan.FromMinutes(1));
        traceWriter.WriteValue(Guid.Empty);
        traceWriter.WriteValue(new Uri("http://www.google.com/"));
        traceWriter.WriteValue(Encoding.UTF8.GetBytes("String!"));
        traceWriter.WriteRawValue("[1],");
        traceWriter.WriteRaw("[2]");
        traceWriter.WriteNull();
        traceWriter.WriteUndefined();
        traceWriter.WriteStartConstructor("ctor");
        traceWriter.WriteValue(1);
        traceWriter.WriteEndConstructor();
        traceWriter.WriteComment("A comment");
        traceWriter.WriteWhitespace("       ");
        traceWriter.WriteEnd();
        traceWriter.WriteEndObject();
        traceWriter.Flush();
        traceWriter.Close();

        var json = @"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    9999999990000000000000000000000000000000000,
    true,
    ""00:01:00"",
    ""00000000-0000-0000-0000-000000000000"",
    ""http://www.google.com/"",
    ""U3RyaW5nIQ=="",
    [1],[2],
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/       
  ]
}";

        XUnitAssert.AreEqualNormalized($"Serialized JSON: {Environment.NewLine}{json}", traceWriter.GetSerializedJsonMessage());
    }

    [Fact]
    public void TraceJsonReaderTest()
    {
        var json = @"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+00:00"",
    ""U3RyaW5nIQ=="",
    1,
    1.1,
    1.2,
    9999999990000000000000000000000000000000000,
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/
  ]
}";

        var stringReader = new StringReader(json);
        var w = new JsonTextReader(stringReader);
        var traceReader = new TraceJsonReader(w);

        traceReader.Read();
        Assert.Equal(JsonToken.StartObject, traceReader.TokenType);

        traceReader.Read();
        Assert.Equal(JsonToken.PropertyName, traceReader.TokenType);
        Assert.Equal("Array", traceReader.Value);

        traceReader.Read();
        Assert.Equal(JsonToken.StartArray, traceReader.TokenType);
        Assert.Equal(null, traceReader.Value);

        traceReader.ReadAsString();
        Assert.Equal(JsonToken.String, traceReader.TokenType);
        Assert.Equal('"', traceReader.QuoteChar);
        Assert.Equal("String!", traceReader.Value);

        // for great code coverage justice!
        traceReader.QuoteChar = '\'';
        Assert.Equal('\'', traceReader.QuoteChar);

        traceReader.ReadAsString();
        Assert.Equal(JsonToken.String, traceReader.TokenType);
        Assert.Equal("2000-12-12T12:12:12Z", traceReader.Value);

        traceReader.ReadAsDateTime();
        Assert.Equal(JsonToken.Date, traceReader.TokenType);
        Assert.Equal(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc), traceReader.Value);

        traceReader.ReadAsDateTimeOffset();
        Assert.Equal(JsonToken.Date, traceReader.TokenType);
        Assert.Equal(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero), traceReader.Value);

        traceReader.ReadAsBytes();
        Assert.Equal(JsonToken.Bytes, traceReader.TokenType);
        Assert.Equal(Encoding.UTF8.GetBytes("String!"), (byte[])traceReader.Value);

        traceReader.ReadAsInt32();
        Assert.Equal(JsonToken.Integer, traceReader.TokenType);
        Assert.Equal(1, traceReader.Value);

        traceReader.ReadAsDecimal();
        Assert.Equal(JsonToken.Float, traceReader.TokenType);
        Assert.Equal(1.1m, traceReader.Value);

        traceReader.ReadAsDouble();
        Assert.Equal(JsonToken.Float, traceReader.TokenType);
        Assert.Equal(1.2d, traceReader.Value);

        traceReader.Read();
        Assert.Equal(JsonToken.Integer, traceReader.TokenType);
        Assert.Equal(typeof(BigInteger), traceReader.ValueType);
        Assert.Equal(BigInteger.Parse("9999999990000000000000000000000000000000000"), traceReader.Value);

        traceReader.Read();
        Assert.Equal(JsonToken.Null, traceReader.TokenType);

        traceReader.Read();
        Assert.Equal(JsonToken.Undefined, traceReader.TokenType);

        traceReader.Read();
        Assert.Equal(JsonToken.StartConstructor, traceReader.TokenType);

        traceReader.Read();
        Assert.Equal(JsonToken.Integer, traceReader.TokenType);
        Assert.Equal(1L, traceReader.Value);

        traceReader.Read();
        Assert.Equal(JsonToken.EndConstructor, traceReader.TokenType);

        traceReader.Read();
        Assert.Equal(JsonToken.Comment, traceReader.TokenType);
        Assert.Equal("A comment", traceReader.Value);

        traceReader.Read();
        Assert.Equal(JsonToken.EndArray, traceReader.TokenType);

        traceReader.Read();
        Assert.Equal(JsonToken.EndObject, traceReader.TokenType);

        Assert.False(traceReader.Read());

        traceReader.Close();

        XUnitAssert.AreEqualNormalized($"Deserialized JSON: {Environment.NewLine}{json}", traceReader.GetDeserializedJsonMessage());
    }
}

public class TraceRecord
{
    public string Message { get; set; }
    public TraceLevel Level { get; set; }
    public Exception Exception { get; set; }

    public override string ToString()
    {
        return $"{Level} - {Message}";
    }
}

public class InMemoryTraceWriter : ITraceWriter
{
    public TraceLevel LevelFilter { get; set; }
    public IList<TraceRecord> TraceRecords { get; set; }

    public InMemoryTraceWriter()
    {
        LevelFilter = TraceLevel.Verbose;
        TraceRecords = new List<TraceRecord>();
    }

    public void Trace(TraceLevel level, string message, Exception ex)
    {
        TraceRecords.Add(
            new TraceRecord
            {
                Level = level,
                Message = message,
                Exception = ex
            });
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        foreach (var traceRecord in TraceRecords)
        {
            stringBuilder.AppendLine(traceRecord.Message);
        }

        return stringBuilder.ToString();
    }
}

public class TraceTestObject
{
    public IList<int> IntList { get; set; }
    public string[] StringArray { get; set; }
    public VersionOld Version { get; set; }
    public IDictionary<string, string> StringDictionary { get; set; }
    public double Double { get; set; }
}

public class IntegerTestClass
{
    public int Integer { get; set; }
}