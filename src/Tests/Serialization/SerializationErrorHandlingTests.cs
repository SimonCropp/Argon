// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;
using ErrorEventArgs = Argon.ErrorEventArgs;

public class SerializationErrorHandlingTests : TestFixtureBase
{
    [Fact]
    public void ErrorHandlingMetadata()
    {
        var errors = new List<Exception>();

        var a2 = JsonConvert.DeserializeObject<AAA>(@"{""MyTest"":{""$type"":""<Namespace>.JsonTest+MyTest2, <Assembly>""}}", new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Error = (_, e) =>
            {
                errors.Add(e.ErrorContext.Error);
                e.ErrorContext.Handled = true;
            }
        });

        Assert.NotNull(a2);
        Assert.Equal(1, errors.Count);
        Assert.Equal("Error resolving type specified in JSON '<Namespace>.JsonTest+MyTest2, <Assembly>'. Path 'MyTest.$type', line 1, position 61.", errors[0].Message);
    }

    [Fact]
    public void ErrorHandlingMetadata_TopLevel()
    {
        var errors = new List<Exception>();

        var a2 = (JObject) JsonConvert.TryDeserializeObject(@"{""$type"":""<Namespace>.JsonTest+MyTest2, <Assembly>""}", new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Error = (_, e) =>
            {
                errors.Add(e.ErrorContext.Error);
                e.ErrorContext.Handled = true;
            }
        });

        Assert.Null(a2);
        Assert.Equal(1, errors.Count);
        Assert.Equal("Error resolving type specified in JSON '<Namespace>.JsonTest+MyTest2, <Assembly>'. Path '$type', line 1, position 51.", errors[0].Message);
    }

    public class AAA
    {
        public ITest MyTest { get; set; }
    }

    public interface ITest
    {
    }

    public class MyTest : ITest
    {
    }

    public class MyClass1
    {
        [JsonProperty("myint")] public int MyInt { get; set; }
        [JsonProperty("Mybool")] public bool Mybool { get; set; }
    }

    [Fact]
    public void ErrorDeserializingIntegerInObject()
    {
        var errors = new List<string>();
        var json = "{\"myint\":3554860000,\"Mybool\":false}";
        var i = JsonConvert.DeserializeObject<MyClass1>(json, new JsonSerializerSettings
        {
            Error = delegate(object _, ErrorEventArgs args)
            {
                errors.Add(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            }
        });

        Assert.Equal(1, errors.Count);
        Assert.Equal("JSON integer 3554860000 is too large or small for an Int32. Path 'myint', line 1, position 19.", errors[0]);
    }

    [Fact]
    public void ErrorDeserializingListHandled()
    {
        var json = """
            [
              {
                "Name": "Jim",
                "BirthDate": "2013-08-14T00:00:00.000",
                "LastModified": "2013-08-14T00:00:00.000"
              },
              {
                "Name": "Jim",
                "BirthDate": "2013-08-14T00:00:00.000",
                "LastModified": "2013-08-14T00:00:00.000"
              }
            ]
            """;

        var possibleMsgs = new[]
        {
            "[1] - Error message for member 1 = An item with the same key has already been added.",
            "[1] - Error message for member 1 = An element with the same key already exists in the dictionary.", // mono
            "[1] - Error message for member 1 = An item with the same key has already been added. Key: Jim" // netcore
        };
        var c = JsonConvert.DeserializeObject<VersionKeyedCollection>(json);
        Assert.Equal(1, c.Count);
        Assert.Equal(1, c.Messages.Count);

        Console.WriteLine(c.Messages[0]);
        Assert.True(possibleMsgs.Any(m => m == c.Messages[0]), $"Expected One of: {Environment.NewLine}{string.Join(Environment.NewLine, possibleMsgs)}{Environment.NewLine}Was: {Environment.NewLine}{c.Messages[0]}");
    }

    [Fact]
    public void DeserializingErrorInChildObject()
    {
        var c = JsonConvert.DeserializeObject<ListErrorObjectCollection>("""
            [
              {
                "Member": "Value1",
                "Member2": null
              },
              {
                "Member": "Value2"
              },
              {
                "ThrowError": "Value",
                "Object": {
                  "Array": [
                    1,
                    2
                  ]
                }
              },
              {
                "ThrowError": "Handle this!",
                "Member": "Value3"
              }
            ]
            """);

        Assert.Equal(3, c.Count);
        Assert.Equal("Value1", c[0].Member);
        Assert.Equal("Value2", c[1].Member);
        Assert.Equal("Value3", c[2].Member);
        Assert.Equal("Handle this!", c[2].ThrowError);
    }

    [Fact]
    public void SerializingErrorIn3DArray()
    {
        var c = new ListErrorObject[,,]
        {
            {
                {
                    new()
                    {
                        Member = "Value1",
                        ThrowError = "Handle this!",
                        Member2 = "Member1"
                    },
                    new()
                    {
                        Member = "Value2",
                        Member2 = "Member2"
                    },
                    new()
                    {
                        Member = "Value3",
                        ThrowError = "Handle that!",
                        Member2 = "Member3"
                    }
                },
                {
                    new()
                    {
                        Member = "Value1",
                        ThrowError = "Handle this!",
                        Member2 = "Member1"
                    },
                    new()
                    {
                        Member = "Value2",
                        Member2 = "Member2"
                    },
                    new()
                    {
                        Member = "Value3",
                        ThrowError = "Handle that!",
                        Member2 = "Member3"
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Error = (_, e) =>
            {
                if (e.CurrentObject.GetType().IsArray)
                {
                    e.ErrorContext.Handled = true;
                }
            }
        });

        XUnitAssert.AreEqualNormalized(@"[
  [
    [
      {
        ""Member"": ""Value1"",
        ""ThrowError"": ""Handle this!"",
        ""Member2"": ""Member1""
      },
      {
        ""Member"": ""Value2""
      },
      {
        ""Member"": ""Value3"",
        ""ThrowError"": ""Handle that!"",
        ""Member2"": ""Member3""
      }
    ],
    [
      {
        ""Member"": ""Value1"",
        ""ThrowError"": ""Handle this!"",
        ""Member2"": ""Member1""
      },
      {
        ""Member"": ""Value2""
      },
      {
        ""Member"": ""Value3"",
        ""ThrowError"": ""Handle that!"",
        ""Member2"": ""Member3""
      }
    ]
  ]
]", json);
    }

    [Fact]
    public void SerializingErrorInChildObject()
    {
        var c = new ListErrorObjectCollection
        {
            new()
            {
                Member = "Value1",
                ThrowError = "Handle this!",
                Member2 = "Member1"
            },
            new()
            {
                Member = "Value2",
                Member2 = "Member2"
            },
            new()
            {
                Member = "Value3",
                ThrowError = "Handle that!",
                Member2 = "Member3"
            }
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            [
              {
                "Member": "Value1",
                "ThrowError": "Handle this!",
                "Member2": "Member1"
              },
              {
                "Member": "Value2"
              },
              {
                "Member": "Value3",
                "ThrowError": "Handle that!",
                "Member2": "Member3"
              }
            ]
            """, json);
    }

    [Fact]
    public void DeserializingErrorInDateTimeCollection()
    {
        var c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(@"[
  ""2009-09-09T00:00:00Z"",
  ""kjhkjhkjhkjh"",
  [
    1
  ],
  ""1977-02-20T00:00:00Z"",
  null,
  ""2000-12-01T00:00:00Z""
]", new IsoDateTimeConverter());

        Assert.Equal(3, c.Count);
        Assert.Equal(new(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
        Assert.Equal(new(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
        Assert.Equal(new(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);
    }

    [Fact]
    public void DeserializingErrorHandlingUsingEvent()
    {
        var errors = new List<string>();

        var serializer = JsonSerializer.Create(new()
        {
            Error = delegate(object _, ErrorEventArgs args)
            {
                errors.Add($"{args.ErrorContext.Path} - {args.ErrorContext.Member} - {args.ErrorContext.Error.Message}");
                args.ErrorContext.Handled = true;
            },
            Converters = {new IsoDateTimeConverter()}
        });
        var c = serializer.Deserialize<List<DateTime>>(new JsonTextReader(new StringReader(@"[
        ""2009-09-09T00:00:00Z"",
        ""I am not a date and will error!"",
        [
          1
        ],
        ""1977-02-20T00:00:00Z"",
        null,
        ""2000-12-01T00:00:00Z""
      ]")));

        // 2009-09-09T00:00:00Z
        // 1977-02-20T00:00:00Z
        // 2000-12-01T00:00:00Z

        // The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
        // Unexpected token parsing date. Expected String, got StartArray.
        // Cannot convert null value to System.DateTime.

        Assert.Equal(3, c.Count);
        Assert.Equal(new(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
        Assert.Equal(new(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
        Assert.Equal(new(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);

        Assert.Equal(3, errors.Count);
        var possibleErrs = new[]
        {
            "[1] - 1 - The string was not recognized as a valid DateTime. There is an unknown word starting at index 0.",
            "[1] - 1 - String was not recognized as a valid DateTime.",
            "[1] - 1 - The string 'I am not a date and will error!' was not recognized as a valid DateTime. There is an unknown word starting at index '0'."
        };

        Assert.True(possibleErrs.Any(m => m == errors[0]),
            $"Expected One of: {string.Join(Environment.NewLine, possibleErrs)}{Environment.NewLine}But was: {errors[0]}");

        Assert.Equal("[2] - 2 - Unexpected token parsing date. Expected String, got StartArray. Path '[2]', line 4, position 9.", errors[1]);
        Assert.Equal("[4] - 4 - Cannot convert null value to System.DateTime. Path '[4]', line 8, position 12.", errors[2]);
    }

    [Fact]
    public void DeserializingErrorInDateTimeCollectionWithAttributeWithEventNotCalled()
    {
        var eventErrorHandlerCalled = false;

        var c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(
            @"[
  ""2009-09-09T00:00:00Z"",
  ""kjhkjhkjhkjh"",
  [
    1
  ],
  ""1977-02-20T00:00:00Z"",
  null,
  ""2000-12-01T00:00:00Z""
]",
            new JsonSerializerSettings
            {
                Error = (_, _) => eventErrorHandlerCalled = true,
                Converters =
                {
                    new IsoDateTimeConverter()
                }
            });

        Assert.Equal(3, c.Count);
        Assert.Equal(new(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
        Assert.Equal(new(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
        Assert.Equal(new(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);

        XUnitAssert.False(eventErrorHandlerCalled);
    }

    [Fact]
    public void SerializePerson()
    {
        var person = new PersonError
        {
            Name = "George Michael Bluth",
            Age = 16,
            Roles = null,
            Title = "Mister Manager"
        };

        var json = JsonConvert.SerializeObject(person, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "Name": "George Michael Bluth",
              "Age": 16,
              "Title": "Mister Manager"
            }
            """, json);
    }

    [Fact]
    public void DeserializeNestedUnhandled()
    {
        var errors = new List<string>();

        var json = @"[[""kjhkjhkjhkjh""]]";

        Exception e = null;
        try
        {
            var serializer = new JsonSerializer();
            serializer.Error += delegate(object _, ErrorEventArgs args)
            {
                // only log an error once
                if (args.CurrentObject == args.ErrorContext.OriginalObject)
                {
                    errors.Add($"{args.ErrorContext.Path} - {args.ErrorContext.Member} - {args.ErrorContext.Error.Message}");
                }
            };

            serializer.Deserialize(new StringReader(json), typeof(List<List<DateTime>>));
        }
        catch (Exception exception)
        {
            e = exception;
        }

        Assert.Equal(@"Could not convert string to DateTime: kjhkjhkjhkjh. Path '[0][0]', line 1, position 16.", e.Message);

        Assert.Equal(1, errors.Count);
        Assert.Equal(@"[0][0] - 0 - Could not convert string to DateTime: kjhkjhkjhkjh. Path '[0][0]', line 1, position 16.", errors[0]);
    }

    [Fact]
    public void MultipleRequiredPropertyErrors()
    {
        var json = "{}";
        var errors = new List<string>();
        var serializer = new JsonSerializer
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Default
        };
        serializer.Error += delegate(object _, ErrorEventArgs args)
        {
            errors.Add($"{args.ErrorContext.Path} - {args.ErrorContext.Member} - {args.ErrorContext.Error.Message}");
            args.ErrorContext.Handled = true;
        };
        serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(MyTypeWithRequiredMembers));

        Assert.Equal(2, errors.Count);
        Assert.True(errors[0].StartsWith(" - Required1 - Required property 'Required1' not found in JSON. Path '', line 1, position 2."));
        Assert.True(errors[1].StartsWith(" - Required2 - Required property 'Required2' not found in JSON. Path '', line 1, position 2."));
    }

    [Fact]
    public void HandlingArrayErrors()
    {
        var json = "[\"a\",\"b\",\"45\",34]";

        var errors = new List<string>();

        var serializer = new JsonSerializer();
        serializer.Error += delegate(object _, ErrorEventArgs args)
        {
            errors.Add($"{args.ErrorContext.Path} - {args.ErrorContext.Member} - {args.ErrorContext.Error.Message}");
            args.ErrorContext.Handled = true;
        };

        serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(int[]));

        Assert.Equal(2, errors.Count);
        Assert.Equal("[0] - 0 - Could not convert string to integer: a. Path '[0]', line 1, position 4.", errors[0]);
        Assert.Equal("[1] - 1 - Could not convert string to integer: b. Path '[1]', line 1, position 8.", errors[1]);
    }

    [Fact]
    public void HandlingMultidimensionalArrayErrors()
    {
        var json = "[[\"a\",\"45\"],[\"b\",34]]";

        var errors = new List<string>();

        var serializer = new JsonSerializer();
        serializer.Error += delegate(object _, ErrorEventArgs args)
        {
            errors.Add($"{args.ErrorContext.Path} - {args.ErrorContext.Member} - {args.ErrorContext.Error.Message}");
            args.ErrorContext.Handled = true;
        };

        serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(int[,]));

        Assert.Equal(2, errors.Count);
        Assert.Equal("[0][0] - 0 - Could not convert string to integer: a. Path '[0][0]', line 1, position 5.", errors[0]);
        Assert.Equal("[1][0] - 0 - Could not convert string to integer: b. Path '[1][0]', line 1, position 16.", errors[1]);
    }

    [Fact]
    public void ErrorHandlingAndAvoidingRecursiveDepthError()
    {
        var json = "{'A':{'A':{'A':{'A':{'A':{}}}}}}";
        var serializer = new JsonSerializer();
        var errors = new List<string>();
        serializer.Error += (_, e) =>
        {
            e.ErrorContext.Handled = true;
            errors.Add(e.ErrorContext.Path);
        };

        serializer.Deserialize<Nest>(new JsonTextReader(new StringReader(json)) {MaxDepth = 3});

        Assert.Equal(1, errors.Count);
        Assert.Equal("A.A.A", errors[0]);
    }

    public class Nest
    {
        public Nest A { get; set; }
    }

    [Fact]
    public void InfiniteErrorHandlingLoopFromInputError()
    {
        var errors = new List<string>();

        var serializer = new JsonSerializer();
        serializer.Error += (_, e) =>
        {
            errors.Add(e.ErrorContext.Error.Message);
            e.ErrorContext.Handled = true;
        };

        var result = serializer.TryDeserialize<ErrorPerson[]>(new JsonTextReader(new ThrowingReader()));

        Assert.Null(result);
        Assert.Equal(3, errors.Count);
        Assert.Equal("too far", errors[0]);
        Assert.Equal("too far", errors[1]);
        Assert.Equal("Infinite loop detected from error handling. Path '[1023]', line 1, position 65536.", errors[2]);
    }

    [Fact]
    public void ArrayHandling()
    {
        var errors = new List<string>();

        var o = JsonConvert.DeserializeObject(
            "[0,x]",
            typeof(int[]),
            new JsonSerializerSettings
            {
                Error = (_, arg) =>
                {
                    errors.Add(arg.ErrorContext.Error.Message);
                    arg.ErrorContext.Handled = true;
                }
            });

        Assert.NotNull(o);

        Assert.Equal(1, errors.Count);
        Assert.Equal("Unexpected character encountered while parsing value: x. Path '[0]', line 1, position 4.", errors[0]);

        Assert.Equal(1, ((int[]) o).Length);
        Assert.Equal(0, ((int[]) o)[0]);
    }

    [Fact]
    public void ArrayHandling_JTokenReader()
    {
        var errors = new List<string>();

        var reader = new JTokenReader(new JArray(0, true));

        var serializer = JsonSerializer.Create(new()
        {
            Error = (_, arg) =>
            {
                errors.Add(arg.ErrorContext.Error.Message);
                arg.ErrorContext.Handled = true;
            }
        });
        var o = serializer.Deserialize(reader, typeof(int[]));

        Assert.NotNull(o);

        Assert.Equal(1, errors.Count);
        Assert.Equal("Error reading integer. Unexpected token: Boolean. Path '[1]'.", errors[0]);

        Assert.Equal(1, ((int[]) o).Length);
        Assert.Equal(0, ((int[]) o)[0]);
    }

    [Fact]
    public void ArrayHandlingInObject()
    {
        var errors = new List<string>();

        var o = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(
            "{'badarray':[0,x,2],'goodarray':[0,1,2]}",
            new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                Error = (_, arg) =>
                {
                    errors.Add(arg.ErrorContext.Error.Message);
                    arg.ErrorContext.Handled = true;
                }
            });

        Assert.NotNull(o);

        Assert.Equal(2, errors.Count);
        Assert.Equal("Unexpected character encountered while parsing value: x. Path 'badarray[0]', line 1, position 16.", errors[0]);
        Assert.Equal("Unexpected character encountered while parsing value: ,. Path 'badarray[1]', line 1, position 17.", errors[1]);

        Assert.Equal(2, o.Count);
        Assert.Equal(2, o["badarray"].Length);
        Assert.Equal(0, o["badarray"][0]);
        Assert.Equal(2, o["badarray"][1]);
    }

    [Fact]
    public void ErrorHandlingEndOfContent()
    {
        var errors = new List<string>();

        const string input = "{\"events\":[{\"code\":64411},{\"code\":64411,\"prio";

        const int maxDepth = 256;
        using (var jsonTextReader = new JsonTextReader(new StringReader(input)) {MaxDepth = maxDepth})
        {
            var jsonSerializer = JsonSerializer.Create(new()
            {
                MaxDepth = maxDepth,
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            });
            jsonSerializer.Error += (_, e) =>
            {
                errors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            };

            var logMessage = jsonSerializer.Deserialize<LogMessage>(jsonTextReader);

            Assert.NotNull(logMessage.Events);
            Assert.Equal(1, logMessage.Events.Count);
            Assert.Equal("64411", logMessage.Events[0].Code);
        }

        Assert.Equal(3, errors.Count);
        Assert.Equal(@"Unterminated string. Expected delimiter: "". Path 'events[1].code', line 1, position 45.", errors[0]);
        Assert.Equal(@"Unexpected end when deserializing array. Path 'events[1].code', line 1, position 45.", errors[1]);
        Assert.Equal(@"Unexpected end when deserializing object. Path 'events[1].code', line 1, position 45.", errors[2]);
    }

    [Fact]
    public void ErrorHandlingEndOfContentDictionary()
    {
        var errors = new List<string>();

        const string input = "{\"events\":{\"code\":64411},\"events2\":{\"code\":64412,";

        const int maxDepth = 256;
        using (var jsonTextReader = new JsonTextReader(new StringReader(input)) {MaxDepth = maxDepth})
        {
            var jsonSerializer = JsonSerializer.Create(new() {MaxDepth = maxDepth, MetadataPropertyHandling = MetadataPropertyHandling.Default});
            jsonSerializer.Error += (_, e) =>
            {
                errors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            };

            var logEvents = jsonSerializer.Deserialize<IDictionary<string, LogEvent>>(jsonTextReader);

            Assert.NotNull(logEvents);
            Assert.Equal(2, logEvents.Count);
            Assert.Equal("64411", logEvents["events"].Code);
            Assert.Equal("64412", logEvents["events2"].Code);
        }

        Assert.Equal(2, errors.Count);
        Assert.Equal(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[0]);
        Assert.Equal(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[1]);
    }

    [Fact]
    public void ErrorHandlingEndOfContentDynamic()
    {
        var errors = new List<string>();

        var json = """
            {
              "Explicit": true,
              "Decimal": 99.9,
              "Int": 1,
              "ChildObject": {
                "Integer": 123
            """;

        var newDynamicObject = JsonConvert.DeserializeObject<TestDynamicObject>(
            json,
            new JsonSerializerSettings
            {
                Error = (_, e) =>
                {
                    errors.Add(e.ErrorContext.Error.Message);
                    e.ErrorContext.Handled = true;
                },
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            });
        XUnitAssert.True(newDynamicObject.Explicit);

        dynamic d = newDynamicObject;

        Assert.Equal(99.9, d.Decimal);
        Assert.Equal(1, d.Int);
        Assert.Equal(123, d.ChildObject.Integer);

        Assert.Equal(2, errors.Count);
        Assert.Equal(@"Unexpected end when deserializing object. Path 'ChildObject.Integer', line 6, position 18.", errors[0]);
        Assert.Equal(@"Unexpected end when deserializing object. Path 'ChildObject.Integer', line 6, position 18.", errors[1]);
    }

    [Fact]
    public void WriteEndOnPropertyState()
    {
        var settings = new JsonSerializerSettings();
        settings.Error += (_, args) =>
        {
            args.ErrorContext.Handled = true;
        };

        var data = new List<ErrorPerson2>
        {
            new() {FirstName = "Scott", LastName = "Hanselman"},
            new() {FirstName = "Scott", LastName = "Hunter"},
            new() {FirstName = "Scott", LastName = "Guthrie"}
        };

        var dictionary = data.GroupBy(person => person.FirstName)
            .ToDictionary(group => group.Key, group => group.Cast<IErrorPerson2>());
        var output = JsonConvert.SerializeObject(dictionary, Formatting.None, settings);
        Assert.Equal(@"{""Scott"":[]}", output);
    }

    [Fact]
    public void WriteEndOnPropertyState2()
    {
        var settings = new JsonSerializerSettings();
        settings.Error += (_, args) =>
        {
            args.ErrorContext.Handled = true;
        };

        var data = new List<ErrorPerson2>
        {
            new() {FirstName = "Scott", LastName = "Hanselman"},
            new() {FirstName = "Scott", LastName = "Hunter"},
            new() {FirstName = "Scott", LastName = "Guthrie"},
            new() {FirstName = "James", LastName = "Newton-King"}
        };

        var dictionary = data
            .GroupBy(person => person.FirstName)
            .ToDictionary(group => group.Key, group => group.Cast<IErrorPerson2>());
        var output = JsonConvert.SerializeObject(dictionary, Formatting.None, settings);

        Assert.Equal(@"{""Scott"":[],""James"":[]}", output);
    }

    [Fact]
    public void NoObjectWithEvent()
    {
        var json = "{\"}";
        var byteArray = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(byteArray);
        var jReader = new JsonTextReader(new StreamReader(stream));
        var s = new JsonSerializer();
        s.Error += (_, args) =>
        {
            args.ErrorContext.Handled = true;
        };
        var obj = s.TryDeserialize<ErrorPerson2>(jReader);

        Assert.Null(obj);
    }

    [Fact]
    public void NoObjectWithAttribute()
    {
        var json = "{\"}";
        var byteArray = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(byteArray);
        var jReader = new JsonTextReader(new StreamReader(stream));
        var s = new JsonSerializer();

        XUnitAssert.Throws<JsonReaderException>(
            () => s.Deserialize<ErrorTestObject>(jReader),
            @"Unterminated string. Expected delimiter: "". Path '', line 1, position 3.");
    }

    public class RootThing
    {
        public Something Something { get; set; }
    }

    public class RootSomethingElse
    {
        public SomethingElse SomethingElse { get; set; }
    }

    /// <summary>
    /// This could be an object we are passing up in an interface.
    /// </summary>
    [JsonConverter(typeof(SomethingConverter))]
    public class Something
    {
        public class SomethingConverter : JsonConverter
        {
            public override bool CanConvert(Type type) =>
                true;

            public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
            {
                try
                {
                    // Do own stuff.
                    // Then call deserialise for inner object.
                    var innerObject = serializer.Deserialize(reader, typeof(SomethingElse));

                    return null;
                }
                catch (Exception exception)
                {
                    // If we get an error wrap it in something less scary.
                    throw new("An error occurred.", exception);
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                try
                {
                    var s = (Something) value;

                    // Do own stuff.
                    // Then call serialise for inner object.
                    serializer.Serialize(writer, s.RootSomethingElse);
                }
                catch (Exception exception)
                {
                    // If we get an error wrap it in something less scary.
                    throw new("An error occurred.", exception);
                }
            }
        }

        public RootSomethingElse RootSomethingElse { get; set; }

        public Something() =>
            RootSomethingElse = new();
    }

    /// <summary>
    /// This is an object that is contained in the interface object.
    /// </summary>
    [JsonConverter(typeof(SomethingElseConverter))]
    public class SomethingElse
    {
        public class SomethingElseConverter : JsonConverter
        {
            public override bool CanConvert(Type type) =>
                true;

            public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
                throw new NotImplementedException();

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
                throw new NotImplementedException();
        }
    }

    [Fact]
    public void DeserializeWrappingErrorsAndErrorHandling()
    {
        var serializer = JsonSerializer.Create(new());

        var foo = "{ something: { rootSomethingElse { somethingElse: 0 } } }";
        var reader = new StringReader(foo);

        XUnitAssert.Throws<Exception>(
            () => serializer.Deserialize(reader, typeof(Something)),
            "An error occurred.");
    }

    [Fact]
    public void SerializeWrappingErrorsAndErrorHandling()
    {
        var serializer = JsonSerializer.Create(new());

        var s = new Something
        {
            RootSomethingElse = new()
            {
                SomethingElse = new()
            }
        };
        var r = new RootThing
        {
            Something = s
        };

        var writer = new StringWriter();

        XUnitAssert.Throws<Exception>(
            () => serializer.Serialize(writer, r),
            "An error occurred.");
    }

    [Fact]
    public void DeserializeRootConverter()
    {
        var result = JsonConvert.TryDeserializeObject<SomethingElse>("{}", new JsonSerializerSettings
        {
            Error = (_, e) =>
            {
                e.ErrorContext.Handled = true;
            }
        });

        Assert.Null(result);
    }

    [Fact]
    public void SerializeRootConverter()
    {
        var result = JsonConvert.SerializeObject(new SomethingElse(), new JsonSerializerSettings
        {
            Error = (_, e) =>
            {
                e.ErrorContext.Handled = true;
            }
        });

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void IntegerToLarge_ReadNextValue()
    {
        var errorMessages = new List<string>();

        var reader = new JsonTextReader(new StringReader("""
            {
              "string1": "blah",
              "int1": 2147483648,
              "string2": "also blah",
              "int2": 2147483648,
              "string3": "more blah",
              "dateTime1": "200NOTDATE",
              "string4": "even more blah"
            }
            """));
        var settings = new JsonSerializerSettings
        {
            Error = (_, args) =>
            {
                errorMessages.Add(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            }
        };
        var serializer = JsonSerializer.Create(settings);

        var data = new DataModel();
        serializer.Populate(reader, data);

        Assert.Equal("blah", data.String1);
        Assert.Equal(0, data.Int1);
        Assert.Equal("also blah", data.String2);
        Assert.Equal(0, data.Int2);
        Assert.Equal("more blah", data.String3);
        Assert.Equal(default, data.DateTime1);
        Assert.Equal("even more blah", data.String4);

        //Assert.AreEqual(2, errorMessages.Count);
        Assert.Equal("JSON integer 2147483648 is too large or small for an Int32. Path 'int1', line 3, position 20.", errorMessages[0]);
        Assert.Equal("JSON integer 2147483648 is too large or small for an Int32. Path 'int2', line 5, position 20.", errorMessages[1]);
        Assert.Equal("Could not convert string to DateTime: 200NOTDATE. Path 'dateTime1', line 7, position 27.", errorMessages[2]);
    }

    [Fact]
    public void HandleErrorInDictionaryObject()
    {
        var json = """
            {
                model1: { String1: 's1', Int1: 'x' },
                model2: { String1: 's2', Int1: 2 }
            }
            """;
        var dictionary = JsonConvert.DeserializeObject<TolerantDictionary<string, DataModel>>(json);

        Assert.Equal(1, dictionary.Count);
        Assert.True(dictionary.ContainsKey("model2"));
        Assert.Equal("s2", dictionary["model2"].String1);
        Assert.Equal(2, dictionary["model2"].Int1);
    }

    class DataModel
    {
        public string String1 { get; set; }
        public int Int1 { get; set; }
        public string String2 { get; set; }
        public int Int2 { get; set; }
        public string String3 { get; set; }
        public DateTime DateTime1 { get; set; }
        public string String4 { get; set; }
    }

    interface IErrorPerson2
    {
    }

    class ErrorPerson2 //:IPerson - oops! Forgot to implement the person interface
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }

    public class ThrowingReader : TextReader
    {
        int _position;
        static string element = "{\"FirstName\":\"Din\",\"LastName\":\"Rav\",\"Item\":{\"ItemName\":\"temp\"}}";
        bool _firstRead = true;
        bool _readComma;

        public override int Read(char[] buffer, int index, int count)
        {
            var temp = new char[buffer.Length];
            var charsRead = 0;
            if (_firstRead)
            {
                charsRead = new StringReader("[").Read(temp, index, count);
                _firstRead = false;
            }
            else
            {
                if (_readComma)
                {
                    charsRead = new StringReader(",").Read(temp, index, count);
                    _readComma = false;
                }
                else
                {
                    charsRead = new StringReader(element).Read(temp, index, count);
                    _readComma = true;
                }
            }

            _position += charsRead;
            if (_position > 65536)
            {
                throw new("too far");
            }

            Array.Copy(temp, index, buffer, index, charsRead);
            return charsRead;
        }
    }

    public class ErrorPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ErrorItem Item { get; set; }
    }

    public class ErrorItem
    {
        public string ItemName { get; set; }
    }

    [JsonObject]
    public class MyTypeWithRequiredMembers
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string Required1;

        [JsonProperty(Required = Required.AllowNull)]
        public string Required2;
    }

    public class LogMessage
    {
        public string DeviceId { get; set; }
        public IList<LogEvent> Events { get; set; }
    }

    public class LogEvent
    {
        public string Code { get; set; }
        public int Priority { get; set; }
    }


    public class ErrorTestObject
    {
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
        }
    }

    /// <summary>
    /// A dictionary that ignores deserialization errors and excludes bad items
    /// </summary>
    public class TolerantDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        [OnError]
        public void OnDeserializationError(StreamingContext streamingContext, ErrorContext errorContext) =>
            errorContext.Handled = true;
    }
}