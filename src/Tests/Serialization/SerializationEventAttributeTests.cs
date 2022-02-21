#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using TestObjects;

public class SerializationEventAttributeTests : TestFixtureBase
{
    [Fact]
    public void ObjectEvents()
    {
        var objs = new[] {new SerializationEventTestObject(), new DerivedSerializationEventTestObject()};

        foreach (var current in objs)
        {
            var obj = current;

            Assert.Equal(11, obj.Member1);
            Assert.Equal("Hello World!", obj.Member2);
            Assert.Equal("This is a nonserialized value", obj.Member3);
            Assert.Equal(null, obj.Member4);
            Assert.Equal(null, obj.Member5);

            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            XUnitAssert.AreEqualNormalized(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}", json);

            Assert.Equal(11, obj.Member1);
            Assert.Equal("This value was reset after serialization.", obj.Member2);
            Assert.Equal("This is a nonserialized value", obj.Member3);
            Assert.Equal(null, obj.Member4);

            var expectedError = $"Error message for member Member6 = Error getting value from 'Member6' on '{obj.GetType().FullName}'.";
            Assert.Equal(expectedError, obj.Member5);

            var o = JObject.Parse(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}");
            o["Member6"] = "Dummy text for error";

            obj = (SerializationEventTestObject) JsonConvert.DeserializeObject(o.ToString(), obj.GetType());

            Assert.Equal(11, obj.Member1);
            Assert.Equal("This value went into the data file during serialization.", obj.Member2);
            Assert.Equal("This value was set during deserialization", obj.Member3);
            Assert.Equal("This value was set after deserialization.", obj.Member4);

            expectedError = $"Error message for member Member6 = Error setting value to 'Member6' on '{obj.GetType()}'.";
            Assert.Equal(expectedError, obj.Member5);

            var derivedObj = obj as DerivedSerializationEventTestObject;
            if (derivedObj != null)
            {
                Assert.Equal("This value was set after deserialization.", derivedObj.Member7);
            }
        }
    }

    [Fact]
    public void ObjectWithConstructorEvents()
    {
        var obj = new SerializationEventTestObjectWithConstructor(11, "Hello World!", null);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}", json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        obj = JsonConvert.DeserializeObject<SerializationEventTestObjectWithConstructor>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value went into the data file during serialization.", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
    }

    [Fact]
    public void ListEvents()
    {
        var obj = new SerializationEventTestList
        {
            1.1m,
            2.222222222m,
            int.MaxValue,
            Convert.ToDecimal(Math.PI)
        };

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  -1.0,
  1.1,
  2.222222222,
  2147483647.0,
  3.14159265358979
]", json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        obj = JsonConvert.DeserializeObject<SerializationEventTestList>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
    }

    [Fact]
    public void DictionaryEvents()
    {
        var obj = new SerializationEventTestDictionary
        {
            {1.1m, "first"},
            {2.222222222m, "second"},
            {int.MaxValue, "third"},
            {Convert.ToDecimal(Math.PI), "fourth"}
        };

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""1.1"": ""first"",
  ""2.222222222"": ""second"",
  ""2147483647"": ""third"",
  ""3.14159265358979"": ""fourth"",
  ""79228162514264337593543950335"": ""Inserted on serializing""
}", json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        obj = JsonConvert.DeserializeObject<SerializationEventTestDictionary>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
    }

    [Fact]
    public void ObjectEventsDocumentationExample()
    {
        var obj = new SerializationEventTestObject();

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);
        Assert.Equal(null, obj.Member5);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}", json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);
        Assert.Equal("Error message for member Member6 = Error getting value from 'Member6' on 'TestObjects.SerializationEventTestObject'.", obj.Member5);

        obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value went into the data file during serialization.", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
        Assert.Equal(null, obj.Member5);
    }

    public class SerializationEventBaseTestObject
    {
        public string TestMember { get; set; }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            TestMember = "Set!";
        }
    }

    public class SerializationEventContextSubClassTestObject : SerializationEventBaseTestObject
    {
    }

    [Fact]
    public void SerializationEventContextTestObjectSubClassTest()
    {
        var obj = new SerializationEventContextSubClassTestObject();

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""TestMember"": ""Set!""
}", json);
    }

    public class SerializationEventContextTestObject
    {
        public string TestMember { get; set; }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            TestMember = $"{context.State} {context.Context}";
        }
    }

    [Fact]
    public void SerializationEventContextTest()
    {
        var value = new SerializationEventContextTestObject();

        var json = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
        {
            Context =
                new StreamingContext(
                    StreamingContextStates.Remoting,
                    "ContextValue")
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""TestMember"": ""Remoting ContextValue""
}", json);
    }

    [Fact]
    public void WhenSerializationErrorDetectedBySerializer_ThenCallbackIsCalled()
    {
        // Verify contract is properly finding our callback
        var resolver = new DefaultContractResolver().ResolveContract(typeof(FooEvent));

        Assert.Equal(resolver.OnErrorCallbacks.Count, 1);

        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            // If I don't specify Error here, the callback isn't called
            // either, but no exception is thrown.
            MissingMemberHandling = MissingMemberHandling.Error,
        });

        // This throws with missing member exception, rather than calling my callback.
        var foo = serializer.Deserialize<FooEvent>(new JsonTextReader(new StringReader("{ Id: 25 }")));

        // When fixed, this would pass.
        Assert.Equal(25, foo.Identifier);
    }

    public class FooEvent
    {
        public int Identifier { get; set; }

        [OnError]
        void OnError(StreamingContext context, ErrorContext error)
        {
            Identifier = 25;

            // Here we could for example manually copy the
            // persisted "Id" value into the renamed "Identifier"
            // property, etc.
            error.Handled = true;
        }
    }

    [Fact]
    public void DerivedSerializationEvents()
    {
        var c = JsonConvert.DeserializeObject<DerivedSerializationEventOrderTestObject>("{}");

        JsonConvert.SerializeObject(c, Formatting.Indented);

        var e = c.GetEvents();

        XUnitAssert.AreEqualNormalized(@"OnDeserializing
OnDeserializing_Derived
OnDeserialized
OnDeserialized_Derived
OnSerializing
OnSerializing_Derived
OnSerialized
OnSerialized_Derived", string.Join(Environment.NewLine, e.ToArray()));
    }

    [Fact]
    public void DerivedDerivedSerializationEvents()
    {
        var c = JsonConvert.DeserializeObject<DerivedDerivedSerializationEventOrderTestObject>("{}");

        JsonConvert.SerializeObject(c, Formatting.Indented);

        var e = c.GetEvents();

        XUnitAssert.AreEqualNormalized(@"OnDeserializing
OnDeserializing_Derived
OnDeserializing_Derived_Derived
OnDeserialized
OnDeserialized_Derived
OnDeserialized_Derived_Derived
OnSerializing
OnSerializing_Derived
OnSerializing_Derived_Derived
OnSerialized
OnSerialized_Derived
OnSerialized_Derived_Derived", string.Join(Environment.NewLine, e.ToArray()));
    }

    [Fact]
    public void DerivedDerivedSerializationEvents_DataContractSerializer()
    {
        var xml = @"<SerializationEventAttributeTests.DerivedDerivedSerializationEventOrderTestObject xmlns=""http://schemas.datacontract.org/2004/07/"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""/>";

        var ss = new DataContractSerializer(typeof(DerivedDerivedSerializationEventOrderTestObject));

        var c = (DerivedDerivedSerializationEventOrderTestObject) ss.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

        var ms = new MemoryStream();
        ss.WriteObject(ms, c);

        var e = c.GetEvents();

        XUnitAssert.AreEqualNormalized(@"OnDeserializing
OnDeserializing_Derived
OnDeserializing_Derived_Derived
OnDeserialized
OnDeserialized_Derived
OnDeserialized_Derived_Derived
OnSerializing
OnSerializing_Derived
OnSerializing_Derived_Derived
OnSerialized
OnSerialized_Derived
OnSerialized_Derived_Derived", string.Join(Environment.NewLine, e.ToArray()));
    }

    [Fact]
    public void NoStreamingContextParameter()
    {
        var d = new ExportPostData
        {
            user = "user!",
            contract = new Contract
            {
                contractName = "name!"
            }
        };

        XUnitAssert.Throws<JsonException>(
            () => JsonConvert.SerializeObject(d, Formatting.Indented),
            "Serialization Callback 'Void Deserialized()' in type 'SerializationEventAttributeTests+Contract' must have a single parameter of type 'System.Runtime.Serialization.StreamingContext'.");
    }

    public class SerializationEventOrderTestObject
    {
        protected IList<string> Events { get; private set; }

        public SerializationEventOrderTestObject()
        {
            Events = new List<string>();
        }

        public IList<string> GetEvents()
        {
            return Events;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            Events.Add("OnSerializing");
        }

        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            Events.Add("OnSerialized");
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            Events.Add("OnDeserializing");
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Events.Add("OnDeserialized");
        }
    }

    public class DerivedSerializationEventOrderTestObject : SerializationEventOrderTestObject
    {
        [OnSerializing]
        internal new void OnSerializingMethod(StreamingContext context)
        {
            Events.Add("OnSerializing_Derived");
        }

        [OnSerialized]
        internal new void OnSerializedMethod(StreamingContext context)
        {
            Events.Add("OnSerialized_Derived");
        }

        [OnDeserializing]
        internal new void OnDeserializingMethod(StreamingContext context)
        {
            Events.Add("OnDeserializing_Derived");
        }

        [OnDeserialized]
        internal new void OnDeserializedMethod(StreamingContext context)
        {
            Events.Add("OnDeserialized_Derived");
        }
    }

    public class DerivedDerivedSerializationEventOrderTestObject : DerivedSerializationEventOrderTestObject
    {
        [OnSerializing]
        internal new void OnSerializingMethod(StreamingContext context)
        {
            Events.Add("OnSerializing_Derived_Derived");
        }

        [OnSerialized]
        internal new void OnSerializedMethod(StreamingContext context)
        {
            Events.Add("OnSerialized_Derived_Derived");
        }

        [OnDeserializing]
        internal new void OnDeserializingMethod(StreamingContext context)
        {
            Events.Add("OnDeserializing_Derived_Derived");
        }

        [OnDeserialized]
        internal new void OnDeserializedMethod(StreamingContext context)
        {
            Events.Add("OnDeserialized_Derived_Derived");
        }
    }

    public class ExportPostData
    {
        public Contract contract { get; set; }
        public bool includeSubItems { get; set; }
        public string user { get; set; }
        public string[] projects { get; set; }
    }

    public class Contract
    {
        public string _id { get; set; }
        public string contractName { get; set; }
        public string contractNumber { get; set; }
        public string updatedBy { get; set; }
        public DateTime updated_at { get; set; }

        bool _onDeserializedCalled;

        public bool GetOnDeserializedCalled()
        {
            return _onDeserializedCalled;
        }

        [OnDeserialized]
        internal void Deserialized()
        {
            _onDeserializedCalled = true;
        }
    }
}