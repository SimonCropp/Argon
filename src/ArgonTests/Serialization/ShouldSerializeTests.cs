// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Xml.Serialization;

public class ShouldSerializeTests : TestFixtureBase
{
    public class A
    {
    }

    public class B
    {
        public A A { get; set; }

        public virtual bool ShouldSerializeA() =>
            false;
    }

    [Fact]
    public void VirtualShouldSerializeSimple()
    {
        var json = JsonConvert.SerializeObject(new B());

        Assert.Equal("{}", json);
    }

    [Fact]
    public void VirtualShouldSerialize()
    {
        var setFoo = new Foo2
        {
            name = Guid.NewGuid().ToString(),
            myBar = new()
            {
                name = Guid.NewGuid().ToString(),
                myBaz = new Baz1[]
                {
                    new()
                    {
                        name = Guid.NewGuid().ToString(),
                        myFrob = new Frob1[]
                        {
                            new() {name = Guid.NewGuid().ToString()}
                        }
                    },
                    new()
                    {
                        name = Guid.NewGuid().ToString(),
                        myFrob = new Frob1[]
                        {
                            new() {name = Guid.NewGuid().ToString()}
                        }
                    },
                    new()
                    {
                        name = Guid.NewGuid().ToString(),
                        myFrob = new Frob1[]
                        {
                            new() {name = Guid.NewGuid().ToString()}
                        }
                    }
                }
            }
        };

        var setFooJson = Serialize(setFoo);
        var deserializedSetFoo = JsonConvert.DeserializeObject<Foo2>(setFooJson);

        Assert.Equal(setFoo.name, deserializedSetFoo.name);
        Assert.NotNull(deserializedSetFoo.myBar);
        Assert.Equal(setFoo.myBar.name, deserializedSetFoo.myBar.name);
        Assert.NotNull(deserializedSetFoo.myBar.myBaz);
        Assert.Equal(setFoo.myBar.myBaz.Length, deserializedSetFoo.myBar.myBaz.Length);
        Assert.Equal(setFoo.myBar.myBaz[0].name, deserializedSetFoo.myBar.myBaz[0].name);
        Assert.NotNull(deserializedSetFoo.myBar.myBaz[0].myFrob[0]);
        Assert.Equal(setFoo.myBar.myBaz[0].myFrob[0].name, deserializedSetFoo.myBar.myBaz[0].myFrob[0].name);
        Assert.Equal(setFoo.myBar.myBaz[1].name, deserializedSetFoo.myBar.myBaz[1].name);
        Assert.NotNull(deserializedSetFoo.myBar.myBaz[2].myFrob[0]);
        Assert.Equal(setFoo.myBar.myBaz[1].myFrob[0].name, deserializedSetFoo.myBar.myBaz[1].myFrob[0].name);
        Assert.Equal(setFoo.myBar.myBaz[2].name, deserializedSetFoo.myBar.myBaz[2].name);
        Assert.NotNull(deserializedSetFoo.myBar.myBaz[2].myFrob[0]);
        Assert.Equal(setFoo.myBar.myBaz[2].myFrob[0].name, deserializedSetFoo.myBar.myBaz[2].myFrob[0].name);

        XUnitAssert.True(setFoo.myBar.ShouldSerializemyBazCalled);
    }

    static string Serialize(Foo2 f)
    {
        //Code copied from JsonConvert.SerializeObject(), with addition of trace writing
        var serializer = JsonSerializer.CreateDefault();

        var stringBuilder = new StringBuilder(256);
        var stringWriter = new StringWriter(stringBuilder, InvariantCulture);
        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.None
               })
        {
            serializer.Serialize(jsonWriter, f, typeof(Foo2));
        }

        return stringWriter.ToString();
    }

    [Fact]
    public void ShouldSerializeTest()
    {
        var c = new ShouldSerializeTestClass
        {
            Name = "James",
            Age = 27
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Age": 27
            }
            """,
            json);

        c.shouldSerializeName = true;
        json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Name": "James",
              "Age": 27
            }
            """,
            json);

        var deserialized = JsonConvert.DeserializeObject<ShouldSerializeTestClass>(json);
        Assert.Equal("James", deserialized.Name);
        Assert.Equal(27, deserialized.Age);
    }

    [Fact]
    public void ShouldSerializeExample()
    {
        var joe = new Employee
        {
            Name = "Joe Employee"
        };
        var mike = new Employee
        {
            Name = "Mike Manager"
        };

        joe.Manager = mike;
        mike.Manager = mike;

        var json = JsonConvert.SerializeObject(new[] {joe, mike}, Formatting.Indented);
        // [
        //   {
        //     "Name": "Joe Employee",
        //     "Manager": {
        //       "Name": "Mike Manager"
        //     }
        //   },
        //   {
        //     "Name": "Mike Manager"
        //   }
        // ]

        XUnitAssert.AreEqualNormalized(
            """
            [
              {
                "Name": "Joe Employee",
                "Manager": {
                  "Name": "Mike Manager"
                }
              },
              {
                "Name": "Mike Manager"
              }
            ]
            """,
            json);
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

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Age": 27
            }
            """,
            json);

        var deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json);
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

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Name": "James",
              "Age": 27,
              "Weight": 0,
              "Height": 0,
              "FavoriteNumber": 23
            }
            """,
            json);

        deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json);
        Assert.Equal("James", deserialized.Name);
        Assert.True(deserialized.NameSpecified);
        Assert.True(deserialized.WeightSpecified);
        Assert.True(deserialized.HeightSpecified);
        Assert.True(deserialized.FavoriteNumberSpecified);
        Assert.Equal(27, deserialized.Age);
        Assert.Equal(23, deserialized.FavoriteNumber);
    }

    //    [Fact]
    //    public void XmlSerializerSpecifiedTrueTest()
    //    {
    //      XmlSerializer s = new XmlSerializer(typeof(OptionalOrder));

    //      StringWriter sw = new StringWriter();
    //      s.Serialize(sw, new OptionalOrder() { FirstOrder = "First", FirstOrderSpecified = true });

    //      Console.WriteLine(sw.ToString());

    //      string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
    //<OptionalOrder xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    //  <FirstOrder>First</FirstOrder>
    //</OptionalOrder>";

    //      OptionalOrder o = (OptionalOrder)s.Deserialize(new StringReader(xml));
    //      Console.WriteLine(o.FirstOrder);
    //      Console.WriteLine(o.FirstOrderSpecified);
    //    }

    //    [Fact]
    //    public void XmlSerializerSpecifiedFalseTest()
    //    {
    //      XmlSerializer s = new XmlSerializer(typeof(OptionalOrder));

    //      StringWriter sw = new StringWriter();
    //      s.Serialize(sw, new OptionalOrder() { FirstOrder = "First", FirstOrderSpecified = false });

    //      Console.WriteLine(sw.ToString());

    //      //      string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
    //      //<OptionalOrder xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    //      //  <FirstOrder>First</FirstOrder>
    //      //</OptionalOrder>";

    //      //      OptionalOrder o = (OptionalOrder)s.Deserialize(new StringReader(xml));
    //      //      Console.WriteLine(o.FirstOrder);
    //      //      Console.WriteLine(o.FirstOrderSpecified);
    //    }

    public class OptionalOrder
    {
        // This field shouldn't be serialized
        // if it is uninitialized.
        public string FirstOrder;

        // Use the XmlIgnoreAttribute to ignore the
        // special field named "FirstOrderSpecified".
        [XmlIgnore] public bool FirstOrderSpecified;
    }

    public class FamilyDetails
    {
        public string Name { get; set; }
        public int NumberOfChildren { get; set; }

        [JsonIgnore] public bool NumberOfChildrenSpecified { get; set; }
    }

    [Fact]
    public void SpecifiedExample()
    {
        var joe = new FamilyDetails
        {
            Name = "Joe Family Details",
            NumberOfChildren = 4,
            NumberOfChildrenSpecified = true
        };

        var martha = new FamilyDetails
        {
            Name = "Martha Family Details",
            NumberOfChildren = 3,
            NumberOfChildrenSpecified = false
        };

        var json = JsonConvert.SerializeObject(new[] {joe, martha}, Formatting.Indented);
        //[
        //  {
        //    "Name": "Joe Family Details",
        //    "NumberOfChildren": 4
        //  },
        //  {
        //    "Name": "Martha Family Details"
        //  }
        //]

        XUnitAssert.AreEqualNormalized(
            """
            [
              {
                "Name": "Joe Family Details",
                "NumberOfChildren": 4
              },
              {
                "Name": "Martha Family Details"
              }
            ]
            """,
            json);

        var mikeString = "{\"Name\": \"Mike Person\"}";
        var mike = JsonConvert.DeserializeObject<FamilyDetails>(mikeString);

        XUnitAssert.False(mike.NumberOfChildrenSpecified);

        var mikeFullDisclosureString = "{\"Name\": \"Mike Person\", \"NumberOfChildren\": \"0\"}";
        mike = JsonConvert.DeserializeObject<FamilyDetails>(mikeFullDisclosureString);

        XUnitAssert.True(mike.NumberOfChildrenSpecified);
    }

    [Fact]
    public void ShouldSerializeInheritedClassTest()
    {
        var joe = new NewEmployee
        {
            Name = "Joe Employee",
            Age = 100
        };

        var mike = new Employee
        {
            Name = "Mike Manager"
        };
        mike.Manager = mike;

        joe.Manager = mike;

        //StringWriter sw = new StringWriter();

        //XmlSerializer x = new XmlSerializer(typeof(NewEmployee));
        //x.Serialize(sw, joe);

        //Console.WriteLine(sw);

        //JavaScriptSerializer s = new JavaScriptSerializer();
        //Console.WriteLine(s.Serialize(new {html = @"<script>hi</script>; & ! ^ * ( ) ! @ # $ % ^ ' "" - , . / ; : [ { } ] ; ' - _ = + ? ` ~ \ |"}));

        var json = JsonConvert.SerializeObject(joe, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Age": 100,
              "Name": "Joe Employee",
              "Manager": {
                "Name": "Mike Manager"
              }
            }
            """,
            json);
    }

    [Fact]
    public void ShouldDeserialize_True()
    {
        var json = @"{'HasName':true,'Name':'Name!'}";

        var c = JsonConvert.DeserializeObject<ShouldDeserializeTestClass>(json, new JsonSerializerSettings
        {
            ContractResolver = ShouldDeserializeContractResolver.Instance,
        });

        Assert.Equal(null, c.ExtensionData);
        XUnitAssert.True(c.HasName);
        Assert.Equal("Name!", c.Name);
    }

    [Fact]
    public void ShouldDeserialize_False()
    {
        var json = @"{'HasName':false,'Name':'Name!'}";

        var c = JsonConvert.DeserializeObject<ShouldDeserializeTestClass>(
            json,
            new JsonSerializerSettings
            {
                ContractResolver = ShouldDeserializeContractResolver.Instance,
            });

        Assert.Equal(1, c.ExtensionData.Count);
        Assert.Equal("Name!", (string) c.ExtensionData["Name"]);
        XUnitAssert.False(c.HasName);
        Assert.Equal(null, c.Name);
    }

    public class Employee
    {
        public string Name { get; set; }
        public Employee Manager { get; set; }

        public bool ShouldSerializeManager() =>
            Manager != this;
    }

    public class NewEmployee : Employee
    {
        public int Age { get; set; }

        public bool ShouldSerializeName() =>
            false;
    }

    public class ShouldSerializeTestClass
    {
        internal bool shouldSerializeName;

        public string Name { get; set; }
        public int Age { get; set; }

        public void ShouldSerializeAge()
        {
            // dummy. should never be used because it doesn't return bool
        }

        public bool ShouldSerializeName() =>
            shouldSerializeName;
    }

    public class SpecifiedTestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }
        public int FavoriteNumber { get; set; }

        // dummy. should never be used because it isn't of type bool
        [JsonIgnore] public long AgeSpecified { get; set; }

        [JsonIgnore] public bool NameSpecified { get; set; }

        [JsonIgnore] public bool WeightSpecified;

        [JsonIgnore] [XmlIgnore] public bool HeightSpecified;

        [JsonIgnore]
        public bool FavoriteNumberSpecified =>
            // get only example
            FavoriteNumber != 0;
    }

    public class Foo2
    {
        public Bar2 myBar { get; set; }

        public string name { get; set; }

        public virtual bool ShouldSerializemyBar() =>
            myBar != null;

        public virtual bool ShouldSerializename() =>
            name != null;
    }

    public class Bar2
    {
        [JsonIgnore] public bool ShouldSerializemyBazCalled { get; set; }

        public Baz1[] myBaz { get; set; }

        public string name { get; set; }

        public virtual bool ShouldSerializemyBaz()
        {
            ShouldSerializemyBazCalled = true;
            return myBaz != null;
        }

        public virtual bool ShouldSerializename() =>
            name != null;
    }

    public class Baz1
    {
        public Frob1[] myFrob { get; set; }

        public string name { get; set; }

        public virtual bool ShouldSerializename() =>
            name != null;

        public virtual bool ShouldSerializemyFrob() =>
            myFrob != null;
    }

    public class Frob1
    {
        public string name { get; set; }

        public virtual bool ShouldSerializename() =>
            name != null;
    }

    public class ShouldDeserializeContractResolver : DefaultContractResolver
    {
        public new static readonly ShouldDeserializeContractResolver Instance = new();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            var shouldDeserializeMethodInfo = member.DeclaringType.GetMethod($"ShouldDeserialize{member.Name}");

            if (shouldDeserializeMethodInfo != null)
            {
                property.ShouldDeserialize = o => (bool) shouldDeserializeMethodInfo.Invoke(o, null);
            }

            return property;
        }
    }

    public class ShouldDeserializeTestClass
    {
        [JsonExtensionData] public IDictionary<string, JToken> ExtensionData { get; set; }

        public bool HasName { get; set; }
        public string Name { get; set; }

        public bool ShouldDeserializeName() =>
            HasName;
    }
}