// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class Issue0198 : TestFixtureBase
{
    [Fact]
    public void Test_List()
    {
        IEnumerable<TestClass1> objects = new List<TestClass1>
        {
            new()
            {
                Prop1 = new HashSet<TestClass2>
                {
                    new()
                    {
                        MyProperty1 = "Test1",
                        MyProperty2 = "Test2",
                    }
                },
                Prop2 = new List<string>
                {
                    "Test1",
                    "Test1"
                },
                Prop3 = new HashSet<TestClass2>
                {
                    new()
                    {
                        MyProperty1 = "Test1",
                        MyProperty2 = "Test2",
                    }
                },
            }
        };

        var serializedData = JsonConvert.SerializeObject(
            objects,
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            });

        var a = JsonConvert.DeserializeObject<IEnumerable<TestClass1>>(
            serializedData,
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

        var o = a.First();

        Assert.Equal(1, o.Prop1.Count);
        Assert.Equal(1, o.Prop2.Count);
        Assert.Equal(1, o.Prop3.Count);
    }

    [Fact]
    public void Test_Collection()
    {
        var c = new TestClass3
        {
            Prop1 = new Dictionary<string, string>
            {
                ["key"] = "value"
            }
        };

        var serializedData = JsonConvert.SerializeObject(
            c,
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            });

        var a = JsonConvert.DeserializeObject<TestClass3>(serializedData, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        Assert.Equal(1, a.Prop1.Count);
    }

    class TestClass1 : AbstractClass
    {
        public ICollection<TestClass2> Prop1 { get; set; } = new HashSet<TestClass2>();
        public ICollection<string> Prop2 { get; set; } = new HashSet<string>();
    }

    class TestClass2
    {
        public string MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
    }

    abstract class AbstractClass
    {
        public ICollection<TestClass2> Prop3 { get; set; } = new List<TestClass2>();
    }

    class TestClass3
    {
        public IDictionary<string, string> Prop1 { get; set; } = new ModelStateDictionary<string>();
    }
}