﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;
using TestObjects;

public class JsonSerializerCollectionsTests : TestFixtureBase
{
    [Fact]
    public void DeserializeNonGenericListTypeAndReadOnlyListViaConstructor()
    {
        var a = JsonConvert.DeserializeObject<ConstructorCollectionContainer>("{'a':1,'b':['aaa'],'c':['aaa']}");

        Assert.Equal(1, a.A);
        Assert.Equal(1, a.B.Count());
        Assert.Equal("aaa", a.B.ElementAt(0));
        Assert.Equal(0, a.C.Count());
    }

    public class ConstructorCollectionContainer
    {
        public int A { get; }
        public IEnumerable<string> B { get; } = new SortedSet<string>();
        public IEnumerable<string> C { get; } = new List<string>().AsReadOnly();

        public ConstructorCollectionContainer(int a) =>
            A = a;
    }

    [Fact]
    public void DeserializeConcurrentDictionaryWithNullValue()
    {
        const string key = "id";

        var jsonValue = $"{{\"{key}\":null}}";

        var deserializedObject = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(jsonValue);

        Assert.Null(deserializedObject[key]);
    }

    [Fact]
    public void SerializeConcurrentQueue()
    {
        var queue1 = new ConcurrentQueue<int>();
        queue1.Enqueue(1);

        var output = JsonConvert.SerializeObject(queue1);
        Assert.Equal(@"[1]", output);

        var queue2 = JsonConvert.DeserializeObject<ConcurrentQueue<int>>(output);
        Assert.True(queue2.TryDequeue(out var i));
        Assert.Equal(1, i);
    }

    [Fact]
    public void SerializeConcurrentBag()
    {
        var bag1 = new ConcurrentBag<int>
        {
            1
        };

        var output = JsonConvert.SerializeObject(bag1);
        Assert.Equal(@"[1]", output);

        var bag2 = JsonConvert.DeserializeObject<ConcurrentBag<int>>(output);
        Assert.True(bag2.TryTake(out var i));
        Assert.Equal(1, i);
    }

    [Fact]
    public void SerializeConcurrentStack()
    {
        var stack1 = new ConcurrentStack<int>();
        stack1.Push(1);

        var output = JsonConvert.SerializeObject(stack1);
        Assert.Equal(@"[1]", output);

        var stack2 = JsonConvert.DeserializeObject<ConcurrentStack<int>>(output);
        Assert.True(stack2.TryPop(out var i));
        Assert.Equal(1, i);
    }

    [Fact]
    public void SerializeConcurrentDictionary()
    {
        var dic1 = new ConcurrentDictionary<int, int>
        {
            [1] = int.MaxValue
        };

        var output = JsonConvert.SerializeObject(dic1);
        Assert.Equal(@"{""1"":2147483647}", output);

        var dic2 = JsonConvert.DeserializeObject<ConcurrentDictionary<int, int>>(output);
        Assert.True(dic2.TryGetValue(1, out var i));
        Assert.Equal(int.MaxValue, i);
    }

    [Fact]
    public void DoubleKey_WholeValue()
    {
        var dictionary = new Dictionary<double, int>
        {
            {
                1d, 1
            }
        };
        var output = JsonConvert.SerializeObject(dictionary);
        Assert.Equal(@"{""1"":1}", output);

        var deserializedValue = JsonConvert.DeserializeObject<Dictionary<double, int>>(output);
        Assert.Equal(1d, deserializedValue.First().Key);
    }

    [Fact]
    public void DoubleKey_MaxValue()
    {
        var dictionary = new Dictionary<double, int>
        {
            {
                double.MaxValue, 1
            }
        };
        var output = JsonConvert.SerializeObject(dictionary);
        Assert.Equal(@"{""1.7976931348623157E+308"":1}", output);

        var deserializedValue = JsonConvert.DeserializeObject<Dictionary<double, int>>(output);
        Assert.Equal(double.MaxValue, deserializedValue.First().Key);
    }

    [Fact]
    public void FloatKey_MaxValue()
    {
        var dictionary = new Dictionary<float, int>
        {
            {
                float.MaxValue, 1
            }
        };
        var output = JsonConvert.SerializeObject(dictionary);
#if !(NET5_0_OR_GREATER)
        Assert.Equal(@"{""3.40282347E+38"":1}", output);
#else
        Assert.Equal(@"{""3.4028235E+38"":1}", output);
#endif

        var deserializedValue = JsonConvert.DeserializeObject<Dictionary<float, int>>(output);
        Assert.Equal(float.MaxValue, deserializedValue.First().Key);
    }

    public class TestCollectionPrivateParameterized : IEnumerable<int>
    {
        readonly List<int> _bars;

        public TestCollectionPrivateParameterized() =>
            _bars = new();

        [Argon.JsonConstructor]
        TestCollectionPrivateParameterized(IEnumerable<int> bars) =>
            _bars = new(bars);

        public void Add(int bar) =>
            _bars.Add(bar);

        public IEnumerator<int> GetEnumerator() =>
            _bars.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    [Fact]
    public void CollectionJsonConstructorPrivateParameterized()
    {
        var c1 = new TestCollectionPrivateParameterized
        {
            0,
            1,
            2
        };
        var json = JsonConvert.SerializeObject(c1);
        var c2 = JsonConvert.DeserializeObject<TestCollectionPrivateParameterized>(json);

        var values = c2.ToList();

        Assert.Equal(3, values.Count);
        Assert.Equal(0, values[0]);
        Assert.Equal(1, values[1]);
        Assert.Equal(2, values[2]);
    }

    public class TestCollectionPrivate : List<int>
    {
        [Argon.JsonConstructor]
        TestCollectionPrivate()
        {
        }

        public static TestCollectionPrivate Create() =>
            new();
    }

    [Fact]
    public void CollectionJsonConstructorPrivate()
    {
        var c1 = TestCollectionPrivate.Create();
        c1.Add(0);
        c1.Add(1);
        c1.Add(2);
        var json = JsonConvert.SerializeObject(c1);
        var c2 = JsonConvert.DeserializeObject<TestCollectionPrivate>(json);

        var values = c2.ToList();

        Assert.Equal(3, values.Count);
        Assert.Equal(0, values[0]);
        Assert.Equal(1, values[1]);
        Assert.Equal(2, values[2]);
    }

    public class TestCollectionMultipleParameters : List<int>
    {
        [Argon.JsonConstructor]
        public TestCollectionMultipleParameters(string s1, string s2)
        {
        }
    }

    [Fact]
    public void CollectionJsonConstructorMultipleParameters() =>
        XUnitAssert.Throws<JsonException>(
            () => JsonConvert.SerializeObject(new TestCollectionMultipleParameters(null, null)),
            "Constructor for 'JsonSerializerCollectionsTests+TestCollectionMultipleParameters' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Int32]'.");

    public class TestCollectionBadIEnumerableParameter : List<int>
    {
        [Argon.JsonConstructor]
        public TestCollectionBadIEnumerableParameter(List<string> l)
        {
        }
    }

    [Fact]
    public void CollectionJsonConstructorBadIEnumerableParameter() =>
        XUnitAssert.Throws<JsonException>(
            () => JsonConvert.SerializeObject(new TestCollectionBadIEnumerableParameter(null)),
            "Constructor for 'JsonSerializerCollectionsTests+TestCollectionBadIEnumerableParameter' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Int32]'.");

    public class TestCollectionNonGeneric : ArrayList
    {
        [Argon.JsonConstructor]
        public TestCollectionNonGeneric(IEnumerable l)
            : base(l.Cast<object>().ToList())
        {
        }
    }

    [Fact]
    public void CollectionJsonConstructorNonGeneric()
    {
        var json = @"[1,2,3]";
        var l = JsonConvert.DeserializeObject<TestCollectionNonGeneric>(json);

        Assert.Equal(3, l.Count);
        Assert.Equal(1L, l[0]);
        Assert.Equal(2L, l[1]);
        Assert.Equal(3L, l[2]);
    }

    public class TestDictionaryPrivateParameterized : Dictionary<string, int>
    {
        public TestDictionaryPrivateParameterized()
        {
        }

        [Argon.JsonConstructor]
        TestDictionaryPrivateParameterized(IEnumerable<KeyValuePair<string, int>> bars)
            : base(bars.ToDictionary(k => k.Key, k => k.Value))
        {
        }
    }

    [Fact]
    public void DictionaryJsonConstructorPrivateParameterized()
    {
        var c1 = new TestDictionaryPrivateParameterized
        {
            {
                "zero", 0
            },
            {
                "one", 1
            },
            {
                "two", 2
            }
        };
        var json = JsonConvert.SerializeObject(c1);
        var c2 = JsonConvert.DeserializeObject<TestDictionaryPrivateParameterized>(json);

        Assert.Equal(3, c2.Count);
        Assert.Equal(0, c2["zero"]);
        Assert.Equal(1, c2["one"]);
        Assert.Equal(2, c2["two"]);
    }

    public class TestDictionaryPrivate : Dictionary<string, int>
    {
        [Argon.JsonConstructor]
        TestDictionaryPrivate()
        {
        }

        public static TestDictionaryPrivate Create() =>
            new();
    }

    [Fact]
    public void DictionaryJsonConstructorPrivate()
    {
        var c1 = TestDictionaryPrivate.Create();
        c1.Add("zero", 0);
        c1.Add("one", 1);
        c1.Add("two", 2);
        var json = JsonConvert.SerializeObject(c1);
        var c2 = JsonConvert.DeserializeObject<TestDictionaryPrivate>(json);

        Assert.Equal(3, c2.Count);
        Assert.Equal(0, c2["zero"]);
        Assert.Equal(1, c2["one"]);
        Assert.Equal(2, c2["two"]);
    }

    public class TestDictionaryMultipleParameters : Dictionary<string, int>
    {
        [Argon.JsonConstructor]
        public TestDictionaryMultipleParameters(string s1, string s2)
        {
        }
    }

    [Fact]
    public void DictionaryJsonConstructorMultipleParameters() =>
        XUnitAssert.Throws<JsonException>(
            () => JsonConvert.SerializeObject(new TestDictionaryMultipleParameters(null, null)),
            "Constructor for 'JsonSerializerCollectionsTests+TestDictionaryMultipleParameters' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Collections.Generic.KeyValuePair`2[System.String,System.Int32]]'.");

    public class TestDictionaryBadIEnumerableParameter : Dictionary<string, int>
    {
        [Argon.JsonConstructor]
        public TestDictionaryBadIEnumerableParameter(Dictionary<string, string> l)
        {
        }
    }

    [Fact]
    public void DictionaryJsonConstructorBadIEnumerableParameter() =>
        XUnitAssert.Throws<JsonException>(
            () => JsonConvert.SerializeObject(new TestDictionaryBadIEnumerableParameter(null)),
            "Constructor for 'JsonSerializerCollectionsTests+TestDictionaryBadIEnumerableParameter' must have no parameters or a single parameter that implements 'System.Collections.Generic.IEnumerable`1[System.Collections.Generic.KeyValuePair`2[System.String,System.Int32]]'.");

    public class TestDictionaryNonGeneric : Hashtable
    {
        [Argon.JsonConstructor]
        public TestDictionaryNonGeneric(IDictionary d)
            : base(d)
        {
        }
    }

    [Fact]
    public void DictionaryJsonConstructorNonGeneric()
    {
        var json = @"{'zero':0,'one':1,'two':2}";
        var d = JsonConvert.DeserializeObject<TestDictionaryNonGeneric>(json);

        Assert.Equal(3, d.Count);
        Assert.Equal(0L, d["zero"]);
        Assert.Equal(1L, d["one"]);
        Assert.Equal(2L, d["two"]);
    }

    public class NameValueCollectionTestClass
    {
        public NameValueCollection Collection { get; set; }
    }

    [Fact]
    public void DeserializeNameValueCollection() =>
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<NameValueCollectionTestClass>("{Collection:[]}"),
            "Cannot create and populate list type System.Collections.Specialized.NameValueCollection. Path 'Collection', line 1, position 13.");

    public class SomeObject
    {
        public string Text1 { get; set; }
    }

    public class CustomConcurrentDictionary :
        ConcurrentDictionary<string, List<SomeObject>>,
        IJsonOnDeserialized
    {
        public void OnDeserialized() =>
            ((IDictionary) this).Add("key2", new List<SomeObject>
            {
                new()
                {
                    Text1 = "value2"
                }
            });
    }

    [Fact]
    public void SerializeCustomConcurrentDictionary()
    {
        IDictionary d = new CustomConcurrentDictionary();
        d.Add("key", new List<SomeObject>
        {
            new()
            {
                Text1 = "value1"
            }
        });

        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "key": [
                {
                  "Text1": "value1"
                }
              ]
            }
            """, json);

        var d2 = JsonConvert.DeserializeObject<CustomConcurrentDictionary>(json);

        Assert.Equal(2, d2.Count);
        Assert.Equal("value1", d2["key"][0].Text1);
        Assert.Equal("value2", d2["key2"][0].Text1);
    }

    [Fact]
    public void NonZeroBasedArray()
    {
        var onebasedArray = Array.CreateInstance(typeof(string), new[]
        {
            3
        }, new[]
        {
            2
        });

        for (var i = onebasedArray.GetLowerBound(0); i <= onebasedArray.GetUpperBound(0); i++)
        {
            onebasedArray.SetValue(i.ToString(InvariantCulture), new[]
            {
                i
            });
        }

        var output = JsonConvert.SerializeObject(onebasedArray, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"[
  ""2"",
  ""3"",
  ""4""
]", output);
    }

    [Fact]
    public void NonZeroBasedMultiArray()
    {
        // lets create a two dimensional array, each rank is 1-based of with a capacity of 4.
        var onebasedArray = Array.CreateInstance(typeof(string), new[]
        {
            3,
            3
        }, new[]
        {
            1,
            2
        });

        // Iterate of the array elements and assign a random double
        for (var i = onebasedArray.GetLowerBound(0); i <= onebasedArray.GetUpperBound(0); i++)
        {
            for (var j = onebasedArray.GetLowerBound(1); j <= onebasedArray.GetUpperBound(1); j++)
            {
                onebasedArray.SetValue($"{i}_{j}", new[]
                {
                    i,
                    j
                });
            }
        }

        // Now lets try and serialize the Array
        var output = JsonConvert.SerializeObject(onebasedArray, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"[
  [
    ""1_2"",
    ""1_3"",
    ""1_4""
  ],
  [
    ""2_2"",
    ""2_3"",
    ""2_4""
  ],
  [
    ""3_2"",
    ""3_3"",
    ""3_4""
  ]
]", output);
    }

    [Fact]
    public void MultiDObjectArray()
    {
        object[,] myOtherArray =
        {
            {
                new KeyValuePair<string, double>("my value", 0.8),
                "foobar"
            },
            {
                true,
                0.4d
            },
            {
                0.05f,
                6
            }
        };

        var myOtherArrayAsString = JsonConvert.SerializeObject(myOtherArray, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            [
              [
                {
                  "Key": "my value",
                  "Value": 0.8
                },
                "foobar"
              ],
              [
                true,
                0.4
              ],
              [
                0.05,
                6
              ]
            ]
            """, myOtherArrayAsString);

        var o = JObject.Parse("""
            {
              "Key": "my value",
              "Value": 0.8
            }
            """);

        var myOtherResult = JsonConvert.DeserializeObject<object[,]>(myOtherArrayAsString);
        Assert.True(JToken.DeepEquals(o, (JToken) myOtherResult[0, 0]));
        Assert.Equal("foobar", myOtherResult[0, 1]);

        XUnitAssert.True(myOtherResult[1, 0]);
        Assert.Equal(0.4, myOtherResult[1, 1]);

        Assert.Equal(0.05, myOtherResult[2, 0]);
        Assert.Equal(6L, myOtherResult[2, 1]);
    }

    public class EnumerableClass<T> : IEnumerable<T>
    {
        readonly IList<T> _values;

        public EnumerableClass(IEnumerable<T> values) =>
            _values = new List<T>(values);

        public IEnumerator<T> GetEnumerator() =>
            _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    [Fact]
    public void DeserializeIEnumerableFromConstructor()
    {
        var json = @"[
  1,
  2,
  null
]";

        var result = JsonConvert.DeserializeObject<EnumerableClass<int?>>(json);

        Assert.Equal(3, result.Count());
        Assert.Equal(1, result.ElementAt(0));
        Assert.Equal(2, result.ElementAt(1));
        Assert.Equal(null, result.ElementAt(2));
    }

    public class EnumerableClassFailure<T> : IEnumerable<T>
    {
        readonly IList<T> values;

        public EnumerableClassFailure() =>
            values = new List<T>();

        public IEnumerator<T> GetEnumerator() =>
            values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    [Fact]
    public void DeserializeIEnumerableFromConstructor_Failure()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<EnumerableClassFailure<string>>(json),
            "Cannot create and populate list type JsonSerializerCollectionsTests+EnumerableClassFailure`1[System.String]. Path '', line 1, position 1.");
    }

    public class PrivateDefaultCtorList<T> : List<T>
    {
        PrivateDefaultCtorList()
        {
        }
    }

    [Fact]
    public void DeserializePrivateListCtor()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<PrivateDefaultCtorList<int>>("[1,2]"),
            "Unable to find a constructor to use for type JsonSerializerCollectionsTests+PrivateDefaultCtorList`1[System.Int32]. Path '', line 1, position 1.");

        var list = JsonConvert.DeserializeObject<PrivateDefaultCtorList<int>>("[1,2]",
            new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });

        Assert.Equal(2, list.Count);
    }

    public class PrivateDefaultCtorWithIEnumerableCtorList<T> : List<T>
    {
        PrivateDefaultCtorWithIEnumerableCtorList()
        {
        }

        public PrivateDefaultCtorWithIEnumerableCtorList(IEnumerable<T> values)
            : base(values) =>
            Add(default);
    }

    [Fact]
    public void DeserializePrivateListConstructor()
    {
        var list = JsonConvert.DeserializeObject<PrivateDefaultCtorWithIEnumerableCtorList<int>>("[1,2]");

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(0, list[2]);
    }

    [Fact]
    public void DeserializeNonIsoDateDictionaryKey()
    {
        var d = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(@"{""04/28/2013 00:00:00"":""test""}");

        Assert.Equal(1, d.Count);

        var key = DateTime.Parse("04/28/2013 00:00:00", InvariantCulture);
        Assert.Equal("test", d[key]);
    }

    [Fact]
    public void DeserializeNonGenericList()
    {
        var l = JsonConvert.DeserializeObject<IList>("['string!']");

        Assert.Equal(typeof(List<object>), l.GetType());
        Assert.Equal(1, l.Count);
        Assert.Equal("string!", l[0]);
    }

    [Fact]
    public void DeserializeReadOnlyListInterface()
    {
        var list = JsonConvert.DeserializeObject<IReadOnlyList<int>>("[1,2,3]");

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void DeserializeReadOnlyCollectionInterface()
    {
        var list = JsonConvert.DeserializeObject<IReadOnlyCollection<int>>("[1,2,3]");

        Assert.Equal(3, list.Count);

        Assert.Equal(1, list.ElementAt(0));
        Assert.Equal(2, list.ElementAt(1));
        Assert.Equal(3, list.ElementAt(2));
    }

    [Fact]
    public void DeserializeReadOnlyCollection()
    {
        var list = JsonConvert.DeserializeObject<ReadOnlyCollection<int>>("[1,2,3]");

        Assert.Equal(3, list.Count);

        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void DeserializeReadOnlyDictionaryInterface()
    {
        var dic = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, int>>("{'one':1,'two':2}");

        Assert.Equal(2, dic.Count);

        Assert.Equal(1, dic["one"]);
        Assert.Equal(2, dic["two"]);

        Assert.IsType(typeof(ReadOnlyDictionary<string, int>), dic);
    }

    [Fact]
    public void DeserializeReadOnlyDictionary()
    {
        var dic = JsonConvert.DeserializeObject<ReadOnlyDictionary<string, int>>("{'one':1,'two':2}");

        Assert.Equal(2, dic.Count);

        Assert.Equal(1, dic["one"]);
        Assert.Equal(2, dic["two"]);
    }

    public class CustomReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        readonly IDictionary<TKey, TValue> dictionary;

        public CustomReadOnlyDictionary(IDictionary<TKey, TValue> dictionary) =>
            this.dictionary = dictionary;

        public bool ContainsKey(TKey key) =>
            dictionary.ContainsKey(key);

        public IEnumerable<TKey> Keys => dictionary.Keys;

        public bool TryGetValue(TKey key, out TValue value) =>
            dictionary.TryGetValue(key, out value);

        public IEnumerable<TValue> Values => dictionary.Values;

        public TValue this[TKey key] => dictionary[key];

        public int Count => dictionary.Count;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            dictionary.GetEnumerator();
    }

    [Fact]
    public void SerializeCustomReadOnlyDictionary()
    {
        var d = new Dictionary<string, int>
        {
            {
                "one", 1
            },
            {
                "two", 2
            }
        };

        var dic = new CustomReadOnlyDictionary<string, int>(d);

        var json = JsonConvert.SerializeObject(dic, Formatting.Indented);
        XUnitAssert.AreEqualNormalized("""
            {
              "one": 1,
              "two": 2
            }
            """, json);
    }

    public class CustomReadOnlyCollection<T> : IReadOnlyCollection<T>
    {
        readonly IList<T> _values;

        public CustomReadOnlyCollection(IList<T> values) =>
            _values = values;

        public int Count => _values.Count;

        public IEnumerator<T> GetEnumerator() =>
            _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _values.GetEnumerator();
    }

    [Fact]
    public void SerializeCustomReadOnlyCollection()
    {
        var l = new List<int>
        {
            1,
            2,
            3
        };

        var list = new CustomReadOnlyCollection<int>(l);

        var json = JsonConvert.SerializeObject(list, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", json);
    }

    [Fact]
    public void TestEscapeDictionaryStrings()
    {
        const string s = @"host\user";
        var serialized = JsonConvert.SerializeObject(s);
        Assert.Equal(@"""host\\user""", serialized);

        var d1 = new Dictionary<int, object>
        {
            {
                5, s
            }
        };
        Assert.Equal(@"{""5"":""host\\user""}", JsonConvert.SerializeObject(d1));

        var d2 = new Dictionary<string, object>
        {
            {
                s, 5
            }
        };
        Assert.Equal(@"{""host\\user"":5}", JsonConvert.SerializeObject(d2));
    }

    public class GenericListTestClass
    {
        public List<string> GenericList { get; set; }

        public GenericListTestClass() =>
            GenericList = new();
    }

    [Fact]
    public void DeserializeExistingGenericList()
    {
        var c = new GenericListTestClass();
        c.GenericList.Add("1");
        c.GenericList.Add("2");

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        var newValue = JsonConvert.DeserializeObject<GenericListTestClass>(json);
        Assert.Equal(2, newValue.GenericList.Count);
        Assert.Equal(typeof(List<string>), newValue.GenericList.GetType());
    }

    [Fact]
    public void DeserializeSimpleKeyValuePair()
    {
        var list = new List<KeyValuePair<string, string>>
        {
            new("key1", "value1"),
            new("key2", "value2")
        };

        var json = JsonConvert.SerializeObject(list);

        Assert.Equal(@"[{""Key"":""key1"",""Value"":""value1""},{""Key"":""key2"",""Value"":""value2""}]", json);

        var result = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(json);
        Assert.Equal(2, result.Count);
        Assert.Equal("key1", result[0].Key);
        Assert.Equal("value1", result[0].Value);
        Assert.Equal("key2", result[1].Key);
        Assert.Equal("value2", result[1].Value);
    }

    [Fact]
    public void DeserializeComplexKeyValuePair()
    {
        var dateTime = new DateTime(2000, 12, 1, 23, 1, 1, DateTimeKind.Utc);

        var list = new List<KeyValuePair<string, WagePerson>>
        {
            new("key1", new()
            {
                BirthDate = dateTime,
                Department = "Department1",
                LastModified = dateTime,
                HourlyWage = 1
            }),
            new("key2", new()
            {
                BirthDate = dateTime,
                Department = "Department2",
                LastModified = dateTime,
                HourlyWage = 2
            })
        };

        var json = JsonConvert.SerializeObject(list, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            [
              {
                "Key": "key1",
                "Value": {
                  "HourlyWage": 1.0,
                  "Name": null,
                  "BirthDate": "2000-12-01T23:01:01Z",
                  "LastModified": "2000-12-01T23:01:01Z"
                }
              },
              {
                "Key": "key2",
                "Value": {
                  "HourlyWage": 2.0,
                  "Name": null,
                  "BirthDate": "2000-12-01T23:01:01Z",
                  "LastModified": "2000-12-01T23:01:01Z"
                }
              }
            ]
            """, json);

        var result = JsonConvert.DeserializeObject<List<KeyValuePair<string, WagePerson>>>(json);
        Assert.Equal(2, result.Count);
        Assert.Equal("key1", result[0].Key);
        Assert.Equal(1, result[0].Value.HourlyWage);
        Assert.Equal("key2", result[1].Key);
        Assert.Equal(2, result[1].Value.HourlyWage);
    }

    [Fact]
    public void StringListAppenderConverterTest()
    {
        var p = new Movie
        {
            ReleaseCountries = new()
            {
                "Existing"
            }
        };

        JsonConvert.PopulateObject("{'ReleaseCountries':['Appended']}", p,
            new()
            {
                Converters = new()
                {
                    new StringListAppenderConverter()
                }
            });

        Assert.Equal(2, p.ReleaseCountries.Count);
        Assert.Equal("Existing", p.ReleaseCountries[0]);
        Assert.Equal("Appended", p.ReleaseCountries[1]);
    }

    [Fact]
    public void StringAppenderConverterTest()
    {
        var p = new Movie
        {
            Name = "Existing,"
        };

        JsonConvert.PopulateObject("{'Name':'Appended'}", p,
            new()
            {
                Converters = new()
                {
                    new StringAppenderConverter()
                }
            });

        Assert.Equal("Existing,Appended", p.Name);
    }

    [Fact]
    public void DeserializeIDictionary()
    {
        var dictionary = JsonConvert.DeserializeObject<IDictionary>("{'name':'value!'}");
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("value!", dictionary["name"]);
    }

    [Fact]
    public void DeserializeIList()
    {
        var list = JsonConvert.DeserializeObject<IList>("['1', 'two', 'III']");
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void NullableValueGenericDictionary()
    {
        var v1 = new Dictionary<string, int?>
        {
            {
                "First", 1
            },
            {
                "Second", null
            },
            {
                "Third", 3
            }
        };

        var json = JsonConvert.SerializeObject(v1, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "First": 1,
              "Second": null,
              "Third": 3
            }
            """, json);

        var v2 = JsonConvert.DeserializeObject<IDictionary<string, int?>>(json);
        Assert.Equal(3, v2.Count);
        Assert.Equal(1, v2["First"]);
        Assert.Equal(null, v2["Second"]);
        Assert.Equal(3, v2["Third"]);
    }

    [Fact]
    public void DeserializeConcurrentDictionary()
    {
        var components = new Dictionary<string, Component>
        {
            {
                "Key!", new Component()
            }
        };
        var go = new GameObject
        {
            Components = new(components),
            Id = "Id!",
            Name = "Name!"
        };

        var originalJson = JsonConvert.SerializeObject(go, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "Components": {
                "Key!": {}
              },
              "Id": "Id!",
              "Name": "Name!"
            }
            """, originalJson);

        var newObject = JsonConvert.DeserializeObject<GameObject>(originalJson);

        Assert.Equal(1, newObject.Components.Count);
        Assert.Equal("Id!", newObject.Id);
        Assert.Equal("Name!", newObject.Name);
    }

    [Fact]
    public void DeserializeKeyValuePairArray()
    {
        var json = @"[ { ""Value"": [ ""1"", ""2"" ], ""Key"": ""aaa"", ""BadContent"": [ 0 ] }, { ""Value"": [ ""3"", ""4"" ], ""Key"": ""bbb"" } ]";

        var values = JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>>>(json);

        Assert.Equal(2, values.Count);
        Assert.Equal("aaa", values[0].Key);
        Assert.Equal(2, values[0].Value.Count);
        Assert.Equal("1", values[0].Value[0]);
        Assert.Equal("2", values[0].Value[1]);
        Assert.Equal("bbb", values[1].Key);
        Assert.Equal(2, values[1].Value.Count);
        Assert.Equal("3", values[1].Value[0]);
        Assert.Equal("4", values[1].Value[1]);
    }

    [Fact]
    public void DeserializeNullableKeyValuePairArray()
    {
        var json = @"[ { ""Value"": [ ""1"", ""2"" ], ""Key"": ""aaa"", ""BadContent"": [ 0 ] }, null, { ""Value"": [ ""3"", ""4"" ], ""Key"": ""bbb"" } ]";

        var values = JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>?>>(json);

        Assert.Equal(3, values.Count);
        Assert.Equal("aaa", values[0].Value.Key);
        Assert.Equal(2, values[0].Value.Value.Count);
        Assert.Equal("1", values[0].Value.Value[0]);
        Assert.Equal("2", values[0].Value.Value[1]);
        Assert.Equal(null, values[1]);
        Assert.Equal("bbb", values[2].Value.Key);
        Assert.Equal(2, values[2].Value.Value.Count);
        Assert.Equal("3", values[2].Value.Value[0]);
        Assert.Equal("4", values[2].Value.Value[1]);
    }

    [Fact]
    public void DeserializeNullToNonNullableKeyValuePairArray()
    {
        var json = @"[ null ]";

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<IList<KeyValuePair<string, IList<string>>>>(json),
            "Cannot convert null value to KeyValuePair. Path '[0]', line 1, position 6.");
    }

    public class PopulateReadOnlyTestClass
    {
        public IList<int> NonReadOnlyList { get; set; }
        public IDictionary<string, int> NonReadOnlyDictionary { get; set; }

        public IList<int> Array { get; set; }

        public IList<int> List { get; set; }
        public IDictionary<string, int> Dictionary { get; set; }

        public IReadOnlyCollection<int> IReadOnlyCollection { get; set; }
        public ReadOnlyCollection<int> ReadOnlyCollection { get; set; }
        public IReadOnlyList<int> IReadOnlyList { get; set; }

        public IReadOnlyDictionary<string, int> IReadOnlyDictionary { get; set; }
        public ReadOnlyDictionary<string, int> ReadOnlyDictionary { get; set; }

        public PopulateReadOnlyTestClass()
        {
            NonReadOnlyList = new List<int>
            {
                1
            };
            NonReadOnlyDictionary = new Dictionary<string, int>
            {
                {
                    "first", 2
                }
            };

            Array = new[]
            {
                3
            };

            List = new ReadOnlyCollection<int>(new[]
            {
                4
            });
            Dictionary = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>
            {
                {
                    "first", 5
                }
            });

            IReadOnlyCollection = new ReadOnlyCollection<int>(new[]
            {
                6
            });
            ReadOnlyCollection = new(new[]
            {
                7
            });
            IReadOnlyList = new ReadOnlyCollection<int>(new[]
            {
                8
            });

            IReadOnlyDictionary = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>
            {
                {
                    "first", 9
                }
            });
            ReadOnlyDictionary = new(new Dictionary<string, int>
            {
                {
                    "first", 10
                }
            });
        }
    }

    [Fact]
    public void SerializeReadOnlyCollections()
    {
        var c1 = new PopulateReadOnlyTestClass();

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "NonReadOnlyList": [
                1
              ],
              "NonReadOnlyDictionary": {
                "first": 2
              },
              "Array": [
                3
              ],
              "List": [
                4
              ],
              "Dictionary": {
                "first": 5
              },
              "IReadOnlyCollection": [
                6
              ],
              "ReadOnlyCollection": [
                7
              ],
              "IReadOnlyList": [
                8
              ],
              "IReadOnlyDictionary": {
                "first": 9
              },
              "ReadOnlyDictionary": {
                "first": 10
              }
            }
            """, json);
    }

    [Fact]
    public void PopulateReadOnlyCollections()
    {
        var json = """
            {
              "NonReadOnlyList": [
                11
              ],
              "NonReadOnlyDictionary": {
                "first": 12
              },
              "Array": [
                13
              ],
              "List": [
                14
              ],
              "Dictionary": {
                "first": 15
              },
              "IReadOnlyCollection": [
                16
              ],
              "ReadOnlyCollection": [
                17
              ],
              "IReadOnlyList": [
                18
              ],
              "IReadOnlyDictionary": {
                "first": 19
              },
              "ReadOnlyDictionary": {
                "first": 20
              }
            }
            """;

        var c2 = JsonConvert.DeserializeObject<PopulateReadOnlyTestClass>(json);

        Assert.Equal(1, c2.NonReadOnlyDictionary.Count);
        Assert.Equal(12, c2.NonReadOnlyDictionary["first"]);

        Assert.Equal(2, c2.NonReadOnlyList.Count);
        Assert.Equal(1, c2.NonReadOnlyList[0]);
        Assert.Equal(11, c2.NonReadOnlyList[1]);

        Assert.Equal(1, c2.Array.Count);
        Assert.Equal(13, c2.Array[0]);
    }

    [Fact]
    public void SerializeArray2D()
    {
        var aa = new Array2D
        {
            Before = "Before!",
            After = "After!",
            Coordinates = new[,]
            {
                {
                    1,
                    1
                },
                {
                    1,
                    2
                },
                {
                    2,
                    1
                },
                {
                    2,
                    2
                }
            }
        };

        var json = JsonConvert.SerializeObject(aa);

        Assert.Equal(@"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}", json);
    }

    [Fact]
    public void SerializeArray3D()
    {
        var aa = new Array3D
        {
            Before = "Before!",
            After = "After!",
            Coordinates = new[,,]
            {
                {
                    {
                        1,
                        1,
                        1
                    },
                    {
                        1,
                        1,
                        2
                    }
                },
                {
                    {
                        1,
                        2,
                        1
                    },
                    {
                        1,
                        2,
                        2
                    }
                },
                {
                    {
                        2,
                        1,
                        1
                    },
                    {
                        2,
                        1,
                        2
                    }
                },
                {
                    {
                        2,
                        2,
                        1
                    },
                    {
                        2,
                        2,
                        2
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(aa);

        Assert.Equal(@"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}", json);
    }

    [Fact]
    public void SerializeArray3DWithConverter()
    {
        var aa = new Array3DWithConverter
        {
            Before = "Before!",
            After = "After!",
            Coordinates = new[,,]
            {
                {
                    {
                        1,
                        1,
                        1
                    },
                    {
                        1,
                        1,
                        2
                    }
                },
                {
                    {
                        1,
                        2,
                        1
                    },
                    {
                        1,
                        2,
                        2
                    }
                },
                {
                    {
                        2,
                        1,
                        1
                    },
                    {
                        2,
                        1,
                        2
                    }
                },
                {
                    {
                        2,
                        2,
                        1
                    },
                    {
                        2,
                        2,
                        2
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(aa, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "Before": "Before!",
              "Coordinates": [
                [
                  [
                    1.0,
                    1.0,
                    1.0
                  ],
                  [
                    1.0,
                    1.0,
                    2.0
                  ]
                ],
                [
                  [
                    1.0,
                    2.0,
                    1.0
                  ],
                  [
                    1.0,
                    2.0,
                    2.0
                  ]
                ],
                [
                  [
                    2.0,
                    1.0,
                    1.0
                  ],
                  [
                    2.0,
                    1.0,
                    2.0
                  ]
                ],
                [
                  [
                    2.0,
                    2.0,
                    1.0
                  ],
                  [
                    2.0,
                    2.0,
                    2.0
                  ]
                ]
              ],
              "After": "After!"
            }
            """, json);
    }

    [Fact]
    public void DeserializeArray3DWithConverter()
    {
        var json = """
            {
              "Before": "Before!",
              "Coordinates": [
                [
                  [
                    1.0,
                    1.0,
                    1.0
                  ],
                  [
                    1.0,
                    1.0,
                    2.0
                  ]
                ],
                [
                  [
                    1.0,
                    2.0,
                    1.0
                  ],
                  [
                    1.0,
                    2.0,
                    2.0
                  ]
                ],
                [
                  [
                    2.0,
                    1.0,
                    1.0
                  ],
                  [
                    2.0,
                    1.0,
                    2.0
                  ]
                ],
                [
                  [
                    2.0,
                    2.0,
                    1.0
                  ],
                  [
                    2.0,
                    2.0,
                    2.0
                  ]
                ]
              ],
              "After": "After!"
            }
            """;

        var aa = JsonConvert.DeserializeObject<Array3DWithConverter>(json);

        Assert.Equal("Before!", aa.Before);
        Assert.Equal("After!", aa.After);
        Assert.Equal(4, aa.Coordinates.GetLength(0));
        Assert.Equal(2, aa.Coordinates.GetLength(1));
        Assert.Equal(3, aa.Coordinates.GetLength(2));
        Assert.Equal(1, aa.Coordinates[0, 0, 0]);
        Assert.Equal(2, aa.Coordinates[1, 1, 1]);
    }

    [Fact]
    public void DeserializeArray2D()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}";

        var aa = JsonConvert.DeserializeObject<Array2D>(json);

        Assert.Equal("Before!", aa.Before);
        Assert.Equal("After!", aa.After);
        Assert.Equal(4, aa.Coordinates.GetLength(0));
        Assert.Equal(2, aa.Coordinates.GetLength(1));
        Assert.Equal(1, aa.Coordinates[0, 0]);
        Assert.Equal(2, aa.Coordinates[1, 1]);

        var after = JsonConvert.SerializeObject(aa);

        Assert.Equal(json, after);
    }

    [Fact]
    public void DeserializeArray2D_WithTooManyItems()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2,3],[2,1],[2,2]],""After"":""After!""}";

        XUnitAssert.Throws<Exception>(
            () => JsonConvert.DeserializeObject<Array2D>(json),
            "Cannot deserialize non-cubical array as multidimensional array.");
    }

    [Fact]
    public void DeserializeArray2D_WithTooFewItems()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1],[2,1],[2,2]],""After"":""After!""}";

        XUnitAssert.Throws<Exception>(
            () => JsonConvert.DeserializeObject<Array2D>(json),
            "Cannot deserialize non-cubical array as multidimensional array.");
    }

    [Fact]
    public void DeserializeArray3D()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

        var aa = JsonConvert.DeserializeObject<Array3D>(json);

        Assert.Equal("Before!", aa.Before);
        Assert.Equal("After!", aa.After);
        Assert.Equal(4, aa.Coordinates.GetLength(0));
        Assert.Equal(2, aa.Coordinates.GetLength(1));
        Assert.Equal(3, aa.Coordinates.GetLength(2));
        Assert.Equal(1, aa.Coordinates[0, 0, 0]);
        Assert.Equal(2, aa.Coordinates[1, 1, 1]);

        var after = JsonConvert.SerializeObject(aa);

        Assert.Equal(json, after);
    }

    [Fact]
    public void DeserializeArray3D_WithTooManyItems()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2,1]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

        XUnitAssert.Throws<Exception>(
            () => JsonConvert.DeserializeObject<Array3D>(json),
            "Cannot deserialize non-cubical array as multidimensional array.");
    }

    [Fact]
    public void DeserializeArray3D_WithBadItems()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1,2]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],{}]],""After"":""After!""}";

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<Array3D>(json),
            "Unexpected token when deserializing multidimensional array: StartObject. Path 'Coordinates[3][1]', line 1, position 99.");
    }

    [Fact]
    public void DeserializeArray3D_WithTooFewItems()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[[1,1,1],[1,1]],[[1,2,1],[1,2,2]],[[2,1,1],[2,1,2]],[[2,2,1],[2,2,2]]],""After"":""After!""}";

        XUnitAssert.Throws<Exception>(
            () => JsonConvert.DeserializeObject<Array3D>(json),
            "Cannot deserialize non-cubical array as multidimensional array.");
    }

    [Fact]
    public void SerializeEmpty3DArray()
    {
        var aa = new Array3D
        {
            Before = "Before!",
            After = "After!",
            Coordinates = new int[0, 0, 0]
        };

        var json = JsonConvert.SerializeObject(aa);

        Assert.Equal(@"{""Before"":""Before!"",""Coordinates"":[],""After"":""After!""}", json);
    }

    [Fact]
    public void DeserializeEmpty3DArray()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[],""After"":""After!""}";

        var aa = JsonConvert.DeserializeObject<Array3D>(json);

        Assert.Equal("Before!", aa.Before);
        Assert.Equal("After!", aa.After);
        Assert.Equal(0, aa.Coordinates.GetLength(0));
        Assert.Equal(0, aa.Coordinates.GetLength(1));
        Assert.Equal(0, aa.Coordinates.GetLength(2));

        var after = JsonConvert.SerializeObject(aa);

        Assert.Equal(json, after);
    }

    [Fact]
    public void DeserializeIncomplete3DArray()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[/*hi*/[/*hi*/[1/*hi*/,/*hi*/1/*hi*/,1]/*hi*/,/*hi*/[1,1";

        XUnitAssert.Throws<JsonException>(() => JsonConvert.DeserializeObject<Array3D>(json));
    }

    [Fact]
    public void DeserializeIncompleteNotTopLevel3DArray()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[/*hi*/[/*hi*/";

        XUnitAssert.Throws<JsonException>(() => JsonConvert.DeserializeObject<Array3D>(json));
    }

    [Fact]
    public void DeserializeNull3DArray()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":null,""After"":""After!""}";

        var aa = JsonConvert.DeserializeObject<Array3D>(json);

        Assert.Equal("Before!", aa.Before);
        Assert.Equal("After!", aa.After);
        Assert.Equal(null, aa.Coordinates);

        var after = JsonConvert.SerializeObject(aa);

        Assert.Equal(json, after);
    }

    [Fact]
    public void DeserializeSemiEmpty3DArray()
    {
        var json = @"{""Before"":""Before!"",""Coordinates"":[[]],""After"":""After!""}";

        var aa = JsonConvert.DeserializeObject<Array3D>(json);

        Assert.Equal("Before!", aa.Before);
        Assert.Equal("After!", aa.After);
        Assert.Equal(1, aa.Coordinates.GetLength(0));
        Assert.Equal(0, aa.Coordinates.GetLength(1));
        Assert.Equal(0, aa.Coordinates.GetLength(2));

        var after = JsonConvert.SerializeObject(aa);

        Assert.Equal(json, after);
    }

    [Fact]
    public void SerializeReferenceTracked3DArray()
    {
        var e1 = new Event1
        {
            EventName = "EventName!"
        };
        var array1 = new[,]
        {
            {
                e1,
                e1
            },
            {
                e1,
                e1
            }
        };
        var values1 = new List<Event1[,]>
        {
            array1,
            array1
        };

        var json = JsonConvert.SerializeObject(values1, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized("""
            {
              "$id": "1",
              "$values": [
                {
                  "$id": "2",
                  "$values": [
                    [
                      {
                        "$id": "3",
                        "EventName": "EventName!",
                        "Venue": null,
                        "Performances": null
                      },
                      {
                        "$ref": "3"
                      }
                    ],
                    [
                      {
                        "$ref": "3"
                      },
                      {
                        "$ref": "3"
                      }
                    ]
                  ]
                },
                {
                  "$ref": "2"
                }
              ]
            }
            """, json);
    }

    [Fact]
    public void SerializeTypeName3DArray()
    {
        var e1 = new Event1
        {
            EventName = "EventName!"
        };
        var array1 = new[,]
        {
            {
                e1,
                e1
            },
            {
                e1,
                e1
            }
        };
        var values1 = new List<Event1[,]>
        {
            array1,
            array1
        };

        var json = JsonConvert.SerializeObject(values1, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized($@"{{
  ""$type"": ""{typeof(List<Event1[,]>).GetTypeName(0, DefaultSerializationBinder.Instance)}"",
  ""$values"": [
    {{
      ""$type"": ""TestObjects.Event1[,], Tests"",
      ""$values"": [
        [
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }},
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }}
        ],
        [
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }},
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }}
        ]
      ]
    }},
    {{
      ""$type"": ""TestObjects.Event1[,], Tests"",
      ""$values"": [
        [
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }},
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }}
        ],
        [
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }},
          {{
            ""$type"": ""TestObjects.Event1, Tests"",
            ""EventName"": ""EventName!"",
            ""Venue"": null,
            ""Performances"": null
          }}
        ]
      ]
    }}
  ]
}}", json);

        var values2 = (IList<Event1[,]>) JsonConvert.DeserializeObject(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        Assert.Equal(2, values2.Count);
        Assert.Equal("EventName!", values2[0][0, 0].EventName);
    }

    [Fact]
    public void PrimitiveValuesInObjectArray()
    {
        var json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",null],""type"":""rpc"",""tid"":2}";

        var o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

        Assert.Equal("Router", o.Action);
        Assert.Equal("Navigate", o.Method);
        Assert.Equal(2, o.Data.Length);
        Assert.Equal("dashboard", o.Data[0]);
        Assert.Equal(null, o.Data[1]);
    }

    [Fact]
    public void ComplexValuesInObjectArray()
    {
        var json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",[""id"", 1, ""teststring"", ""test""],{""one"":1}],""type"":""rpc"",""tid"":2}";

        var o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

        Assert.Equal("Router", o.Action);
        Assert.Equal("Navigate", o.Method);
        Assert.Equal(3, o.Data.Length);
        Assert.Equal("dashboard", o.Data[0]);
        Assert.IsType(typeof(JArray), o.Data[1]);
        Assert.Equal(4, ((JArray) o.Data[1]).Count);
        Assert.IsType(typeof(JObject), o.Data[2]);
        Assert.Equal(1, ((JObject) o.Data[2]).Count);
        Assert.Equal(1, (int) ((JObject) o.Data[2])["one"]);
    }

    [Fact]
    public void SerializeArrayAsArrayList()
    {
        var jsonText = @"[3, ""somestring"",[1,2,3],{}]";
        var o = JsonConvert.DeserializeObject<ArrayList>(jsonText);

        Assert.Equal(4, o.Count);
        Assert.Equal(3, ((JArray) o[2]).Count);
        Assert.Equal(0, ((JObject) o[3]).Count);
    }

    [Fact]
    public void SerializeMemberGenericList()
    {
        var name = new Name("The Idiot in Next To Me");

        var p1 = new PhoneNumber("555-1212");
        var p2 = new PhoneNumber("444-1212");

        name.pNumbers.Add(p1);
        name.pNumbers.Add(p2);

        var json = JsonConvert.SerializeObject(name, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "personsName": "The Idiot in Next To Me",
              "pNumbers": [
                {
                  "phoneNumber": "555-1212"
                },
                {
                  "phoneNumber": "444-1212"
                }
              ]
            }
            """, json);

        var newName = JsonConvert.DeserializeObject<Name>(json);

        Assert.Equal("The Idiot in Next To Me", newName.personsName);

        // not passed in as part of the constructor but assigned to pNumbers property
        Assert.Equal(2, newName.pNumbers.Count);
        Assert.Equal("555-1212", newName.pNumbers[0].phoneNumber);
        Assert.Equal("444-1212", newName.pNumbers[1].phoneNumber);
    }

    public class MultipleDefinedPropertySerialization
    {
        [Fact]
        public void SerializePropertyDefinedInMultipleInterfaces()
        {
            const string propertyValue = "value";

            var list = new List<ITestInterface>
            {
                new TestClass
                {
                    Property = propertyValue
                }
            };

            var json = JsonConvert.SerializeObject(list);

            XUnitAssert.AreEqualNormalized($"[{{\"Property\":\"{propertyValue}\"}}]", json);
        }

        public interface IFirstInterface
        {
            string Property { get; set; }
        }

        public interface ISecondInterface
        {
            string Property { get; set; }
        }

        public interface ITestInterface : IFirstInterface, ISecondInterface
        {
        }

        public class TestClass : ITestInterface
        {
            public string Property { get; set; }
        }
    }

    [Fact]
    public void CustomCollectionSerialization()
    {
        var collection = new ProductCollection
        {
            new()
            {
                Name = "Test1"
            },
            new()
            {
                Name = "Test2"
            },
            new()
            {
                Name = "Test3"
            }
        };

        var jsonSerializer = new JsonSerializer
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var stringWriter = new StringWriter();

        jsonSerializer.Serialize(stringWriter, collection);

        Assert.Equal(@"[{""Name"":""Test1"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test2"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null},{""Name"":""Test3"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null}]",
            stringWriter.GetStringBuilder().ToString());

        var collectionNew = (ProductCollection) jsonSerializer.Deserialize(new JsonTextReader(new StringReader(stringWriter.GetStringBuilder().ToString())), typeof(ProductCollection));

        Assert.Equal(collection, collectionNew);
    }

    [Fact]
    public void GenericCollectionInheritance()
    {
        var foo1 = new GenericClass<GenericItem<string>, string>();
        foo1.Items.Add(new()
        {
            Value = "Hello"
        });

        var json = JsonConvert.SerializeObject(new
        {
            selectList = foo1
        });
        Assert.Equal(@"{""selectList"":[{""Value"":""Hello""}]}", json);

        var foo2 = new GenericClass<NonGenericItem, string>();
        foo2.Items.Add(new()
        {
            Value = "Hello"
        });

        json = JsonConvert.SerializeObject(new
        {
            selectList = foo2
        });
        Assert.Equal(@"{""selectList"":[{""Value"":""Hello""}]}", json);

        var foo3 = new NonGenericClass();
        foo3.Items.Add(new NonGenericItem
        {
            Value = "Hello"
        });

        json = JsonConvert.SerializeObject(new
        {
            selectList = foo3
        });
        Assert.Equal(@"{""selectList"":[{""Value"":""Hello""}]}", json);
    }

    [Fact]
    public void InheritedListSerialize()
    {
        var a1 = new Article("a1");
        var a2 = new Article("a2");

        var articles1 = new ArticleCollection
        {
            a1,
            a2
        };

        var jsonText = JsonConvert.SerializeObject(articles1);

        var articles2 = JsonConvert.DeserializeObject<ArticleCollection>(jsonText);

        Assert.Equal(articles1.Count, articles2.Count);
        Assert.Equal(articles1[0].Name, articles2[0].Name);
    }

    [Fact]
    public void ReadOnlyCollectionSerialize()
    {
        var r1 = new ReadOnlyCollection<int>(new[]
        {
            0,
            1,
            2,
            3,
            4
        });

        var jsonText = JsonConvert.SerializeObject(r1);

        Assert.Equal("[0,1,2,3,4]", jsonText);

        var r2 = JsonConvert.DeserializeObject<ReadOnlyCollection<int>>(jsonText);

        Assert.Equal(r1, r2);
    }

    [Fact]
    public void SerializeGenericList()
    {
        var p1 = new Product
        {
            Name = "Product 1",
            Price = 99.95m,
            ExpiryDate = new(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
        };
        var p2 = new Product
        {
            Name = "Product 2",
            Price = 12.50m,
            ExpiryDate = new(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        var products = new List<Product>
        {
            p1,
            p2
        };

        var json = JsonConvert.SerializeObject(products, Formatting.Indented);
        //[
        //  {
        //    "Name": "Product 1",
        //    "ExpiryDate": "\/Date(978048000000)\/",
        //    "Price": 99.95,
        //    "Sizes": null
        //  },
        //  {
        //    "Name": "Product 2",
        //    "ExpiryDate": "\/Date(1248998400000)\/",
        //    "Price": 12.50,
        //    "Sizes": null
        //  }
        //]

        XUnitAssert.AreEqualNormalized("""
            [
              {
                "Name": "Product 1",
                "ExpiryDate": "2000-12-29T00:00:00Z",
                "Price": 99.95,
                "Sizes": null
              },
              {
                "Name": "Product 2",
                "ExpiryDate": "2009-07-31T00:00:00Z",
                "Price": 12.50,
                "Sizes": null
              }
            ]
            """, json);
    }

    [Fact]
    public void DeserializeGenericList()
    {
        var json = @"[
        {
          ""Name"": ""Product 1"",
          ""ExpiryDate"": ""2013-08-14T04:38:31.000+0000"",
          ""Price"": 99.95,
          ""Sizes"": null
        },
        {
          ""Name"": ""Product 2"",
          ""ExpiryDate"": ""2013-08-14T04:38:31.000+0000"",
          ""Price"": 12.50,
          ""Sizes"": null
        }
      ]";

        var products = JsonConvert.DeserializeObject<List<Product>>(json);

        var p1 = products[0];

        Assert.Equal(2, products.Count);
        Assert.Equal("Product 1", p1.Name);
    }

    [Fact]
    public void ReadOnlyIntegerListTest()
    {
        var l = new ReadOnlyIntegerList(new()
        {
            1,
            2,
            3,
            int.MaxValue
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3,
  2147483647
]", json);
    }

    [Fact]
    public void EmptyStringInHashtableIsDeserialized()
    {
        var externalJson = @"{""$type"":""System.Collections.Hashtable, mscorlib"",""testkey"":""""}";

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        JsonConvert.SerializeObject(new Hashtable
        {
            {
                "testkey", ""
            }
        }, settings);
        var deserializeTest2 = JsonConvert.DeserializeObject<Hashtable>(externalJson, settings);

        Assert.Equal(deserializeTest2["testkey"], "");
    }

    [Fact]
    public void DeserializeCollectionWithConstructorArrayArgument()
    {
        var v = new ReadOnlyCollectionWithArrayArgument<double>(new[]
        {
            -0.014147478859765236,
            -0.011419606805541858,
            -0.010038461483676238
        });
        var json = JsonConvert.SerializeObject(v);

        XUnitAssert.Throws<JsonSerializationException>(
            () =>
            {
                JsonConvert.DeserializeObject<ReadOnlyCollectionWithArrayArgument<double>>(json);
            },
            "Unable to find a constructor to use for type JsonSerializerCollectionsTests+ReadOnlyCollectionWithArrayArgument`1[System.Double]. Path '', line 1, position 1.");
    }

    [Fact]
    public void NonDefaultConstructor_DuplicateKeyInDictionary_Replace()
    {
        var json = @"{ ""user"":""bpan"", ""Person"":{ ""groups"":""replaced!"", ""domain"":""adm"", ""mail"":""bpan@sdu.dk"", ""sn"":""Pan"", ""gn"":""Benzhi"", ""cn"":""Benzhi Pan"", ""eo"":""BQHLJaVTMr0eWsi1jaIut4Ls/pSuMeNEmsWfWsfKo="", ""guid"":""9A38CE8E5B288942A8DA415CF5E687"", ""employeenumber"":""2674"", ""omk1"":""930"", ""language"":""da"" }, ""XMLResponce"":""<?xml version='1.0' encoding='iso-8859-1' ?>\n<cas:serviceResponse xmlns:cas='http://www.yale.edu/tp/cas'>\n\t<cas:authenticationSuccess>\n\t\t<cas:user>bpan</cas:user>\n\t\t<norEduPerson>\n\t\t\t<groups>FNC-PRI-APP-SUNDB-EDOR-A,FNC-RI-APP-SUB-EDITOR-B</groups>\n\t\t\t<domain>adm</domain>\n\t\t\t<mail>bpan@sdu.dk</mail>\n\t\t\t<sn>Pan</sn>\n\t\t\t<gn>Benzhi</gn>\n\t\t\t<cn>Benzhi Pan</cn>\n\t\t\t<eo>BQHLJaVTMr0eWsi1jaIut4Lsfr/pSuMeNEmsWfWsfKo=</eo>\n\t\t\t<guid>9A38CE8E5B288942A8DA415C2C687</guid>\n\t\t\t<employeenumber>274</employeenumber>\n\t\t\t<omk1>930</omk1>\n\t\t\t<language>da</language>\n\t\t</norEduPerson>\n\t</cas:authenticationSuccess>\n</cas:serviceResponse>\n"", ""Language"":1, ""Groups"":[ ""FNC-PRI-APP-SNDB-EDOR-A"", ""FNC-PI-APP-SUNDB-EDOR-B"" ], ""Domain"":""adm"", ""Mail"":""bpan@sdu.dk"", ""Surname"":""Pan"", ""Givenname"":""Benzhi"", ""CommonName"":""Benzhi Pan"", ""OrganizationName"":null }";

        var result = JsonConvert.DeserializeObject<CASResponce>(json);

        Assert.Equal("replaced!", result.Person["groups"]);
    }

    [Fact]
    public void GenericIListAndOverrideConstructor()
    {
        var deserialized = JsonConvert.DeserializeObject<MyClass>(@"[""apple"", ""monkey"", ""goose""]");

        Assert.Equal("apple", deserialized[0]);
        Assert.Equal("monkey", deserialized[1]);
        Assert.Equal("goose", deserialized[2]);
    }

#if !NET5_0_OR_GREATER
    [Fact]
    public void DeserializeCultureInfoKey()
    {
        var json = @"{ ""en-US"": ""Hi"", ""sv-SE"": ""Hej"" }";

        var values = JsonConvert.DeserializeObject<Dictionary<CultureInfo, string>>(json);
        Assert.Equal(2, values.Count);
    }
#endif

    [Fact]
    public void DeserializeEmptyEnumerable_NoItems()
    {
        var c = JsonConvert.DeserializeObject<ValuesClass>(@"{""Values"":[]}");
        Assert.Equal(0, c.Values.Count());
    }

    [Fact]
    public void DeserializeEmptyEnumerable_HasItems()
    {
        var c = JsonConvert.DeserializeObject<ValuesClass>(@"{""Values"":[""hello""]}");
        Assert.Equal(1, c.Values.Count());
        Assert.Equal("hello", c.Values.ElementAt(0));
    }

    public class ValuesClass
    {
        public IEnumerable<string> Values { get; set; } = Enumerable.Empty<string>();
    }

    [Fact]
    public void DeserializeConstructorWithReadonlyArrayProperty()
    {
        var json = @"{""Endpoint"":""http://localhost"",""Name"":""account1"",""Dimensions"":[{""Key"":""Endpoint"",""Value"":""http://localhost""},{""Key"":""Name"",""Value"":""account1""}]}";

        var values = JsonConvert.DeserializeObject<AccountInfo>(json);
        Assert.Equal("http://localhost", values.Endpoint);
        Assert.Equal("account1", values.Name);
        Assert.Equal(2, values.Dimensions.Length);
    }

    public sealed class AccountInfo
    {
        KeyValuePair<string, string>[] metricDimensions;

        public AccountInfo(string endpoint, string name)
        {
            Endpoint = endpoint;
            Name = name;
        }

        public string Endpoint { get; }

        public string Name { get; }

        public KeyValuePair<string, string>[] Dimensions =>
            metricDimensions ??= new KeyValuePair<string, string>[]
            {
                new("Endpoint", Endpoint),
                new("Name", Name)
            };
    }

    public class MyClass : IList<string>
    {
        List<string> storage;

        [Argon.JsonConstructor]
        MyClass() =>
            storage = new();

        public MyClass(IEnumerable<string> source) =>
            storage = new(source);

        //Below is generated by VS to implement IList<string>
        public string this[int index]
        {
            get => ((IList<string>) storage)[index];

            set => ((IList<string>) storage)[index] = value;
        }

        public int Count => ((IList<string>) storage).Count;

        public bool IsReadOnly => ((IList<string>) storage).IsReadOnly;

        public void Add(string item) =>
            ((IList<string>) storage).Add(item);

        public void Clear() =>
            ((IList<string>) storage).Clear();

        public bool Contains(string item) =>
            ((IList<string>) storage).Contains(item);

        public void CopyTo(string[] array, int arrayIndex) =>
            ((IList<string>) storage).CopyTo(array, arrayIndex);

        public IEnumerator<string> GetEnumerator() =>
            ((IList<string>) storage).GetEnumerator();

        public int IndexOf(string item) =>
            ((IList<string>) storage).IndexOf(item);

        public void Insert(int index, string item) =>
            ((IList<string>) storage).Insert(index, item);

        public bool Remove(string item) =>
            ((IList<string>) storage).Remove(item);

        public void RemoveAt(int index) =>
            ((IList<string>) storage).RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IList<string>) storage).GetEnumerator();
    }


    public class CASResponce
    {
        //<?xml version='1.0' encoding='iso-8859-1' ?>
        //<cas:serviceResponse xmlns:cas='http://www.yale.edu/tp/cas'>
        //    <cas:authenticationSuccess>
        //        <cas:user>and</cas:user>
        //        <norEduPerson>
        //            <groups>IT-service-OD,USR-IT-service,IT-service-udvikling</groups>
        //            <domain>adm</domain>
        //            <mail>and@sdu.dk</mail>
        //            <sn>And</sn>
        //            <gn>Anders</gn>
        //            <cn>Anders And</cn>
        //            <eo>QQT3tKSKjCxQSGsDiR8HTP9L5VsojBvOYyjOu8pwLMA=</eo>
        //            <guid>DE423352CC763649B8F2ECF1DA304750</guid>
        //            <language>da</language>
        //        </norEduPerson>
        //    </cas:authenticationSuccess>
        //</cas:serviceResponse>

        // NemID
        //<cas:serviceResponse xmlns:cas="http://www.yale.edu/tp/cas">
        //  <cas:authenticationSuccess>
        //      <cas:user>
        //          2903851921
        //      </cas:user>
        //  </cas:authenticationSuccess>
        //</cas:serviceResponse>

        //WAYF
        //<cas:serviceResponse xmlns:cas="http://www.yale.edu/tp/cas">
        //  <cas:authenticationSuccess>
        //     <cas:user>
        //          jj@testidp.wayf.dk
        //     </cas:user>
        //  <norEduPerson>
        //     <sn>Jensen</sn>
        //     <gn>Jens</gn>
        //     <cn>Jens farmer</cn>
        //      <eduPersonPrincipalName>jj @testidp.wayf.dk</eduPersonPrincipalName>
        //        <mail>jens.jensen @institution.dk</mail>
        //        <organizationName>Institution</organizationName>
        //        <eduPersonAssurance>2</eduPersonAssurance>
        //        <schacPersonalUniqueID>urn:mace:terena.org:schac:personalUniqueID:dk:CPR:0708741234</schacPersonalUniqueID>
        //        <eduPersonScopedAffiliation>student @course1.testidp.wayf.dk</eduPersonScopedAffiliation>
        //        <eduPersonScopedAffiliation>staff @course1.testidp.wayf.dk</eduPersonScopedAffiliation>
        //        <eduPersonScopedAffiliation>staff @course1.testidp.wsayf.dk</eduPersonScopedAffiliation>
        //        <preferredLanguage>en</preferredLanguage>
        //        <eduPersonEntitlement>test</eduPersonEntitlement>
        //        <eduPersonPrimaryAffiliation>student</eduPersonPrimaryAffiliation>
        //        <schacCountryOfCitizenship>DK</schacCountryOfCitizenship>
        //        <eduPersonTargetedID>WAYF-DK-7a86d1c3b69a9639d7650b64f2eb773bd21a8c6d</eduPersonTargetedID>
        //        <schacHomeOrganization>testidp.wayf.dk</schacHomeOrganization>
        //        <givenName>Jens</givenName>
        //      <o>Institution</o>
        //     <idp>https://testbridge.wayf.dk</idp>
        //  </norEduPerson>
        // </cas:authenticationSuccess>
        //</cas:serviceResponse>

        public enum ssoLanguage
        {
            Unknown,
            Danish,
            English
        }

        public CASResponce(string xmlResponce)
        {
            Domain = "";
            Mail = "";
            Surname = "";
            Givenname = "";
            CommonName = "";

            ParseReplyXML(xmlResponce);
            ExtractGroups();
            ExtractLanguage();
        }

        void ExtractGroups()
        {
            Groups = new();
            if (Person.ContainsKey("groups"))
            {
                var groupsString = Person["groups"];
                var stringList = groupsString.Split(',');

                foreach (var group in stringList)
                {
                    Groups.Add(group);
                }
            }
        }

        void ExtractLanguage()
        {
            if (Person.ContainsKey("language"))
            {
                switch (Person["language"].Trim())
                {
                    case "da":
                        Language = ssoLanguage.Danish;
                        break;
                    case "en":
                        Language = ssoLanguage.English;
                        break;
                    default:
                        Language = ssoLanguage.Unknown;
                        break;
                }
            }
            else
            {
                Language = ssoLanguage.Unknown;
            }
        }

        void ParseReplyXML(string xmlString)
        {
            try
            {
                var xDoc = XDocument.Parse(xmlString);

                var root = xDoc.Root;

                var ns = "http://www.yale.edu/tp/cas";

                var auth = root.Element(XName.Get("authenticationSuccess", ns)) ??
                           root.Element(XName.Get("authenticationFailure", ns));

                var xNodeUser = auth.Element(XName.Get("user", ns));

                var eduPers = auth.Element(XName.Get("norEduPerson", ""));

                var casUser = "";
                var eduPerson = new Dictionary<string, string>();

                if (xNodeUser != null)
                {
                    casUser = xNodeUser.Value;

                    if (eduPers != null)
                    {
                        foreach (var xPersonValue in eduPers.Elements())
                        {
                            if (eduPerson.ContainsKey(xPersonValue.Name.LocalName))
                            {
                                eduPerson[xPersonValue.Name.LocalName] = $"{eduPerson[xPersonValue.Name.LocalName]};{xPersonValue.Value}";
                            }
                            else
                            {
                                eduPerson.Add(xPersonValue.Name.LocalName, xPersonValue.Value);
                            }
                        }
                    }
                }

                if (casUser.Trim() != "")
                {
                    user = casUser;
                }

                if (eduPerson.ContainsKey("domain"))
                {
                    Domain = eduPerson["domain"];
                }

                if (eduPerson.ContainsKey("organizationName"))
                {
                    OrganizationName = eduPerson["organizationName"];
                }

                if (eduPerson.ContainsKey("mail"))
                {
                    Mail = eduPerson["mail"];
                }

                if (eduPerson.ContainsKey("sn"))
                {
                    Surname = eduPerson["sn"];
                }

                if (eduPerson.ContainsKey("gn"))
                {
                    Givenname = eduPerson["gn"];
                }

                if (eduPerson.ContainsKey("cn"))
                {
                    CommonName = eduPerson["cn"];
                }

                Person = eduPerson;
                XMLResponce = xmlString;
            }
            catch
            {
                user = "";
            }
        }

        /// <summary>
        /// Fast felt der altid findes.
        /// </summary>
        public string user { get; private set; }

        /// <summary>
        /// Person type som dictionary indeholdende de ekstra informationer returneret ved login.
        /// </summary>
        public Dictionary<string, string> Person { get; private set; }

        /// <summary>
        /// Den oprindelige xml returneret fra CAS.
        /// </summary>
        public string XMLResponce { get; private set; }

        /// <summary>
        /// Det sprog der benyttes i SSO. Muligheder er da eller en.
        /// </summary>
        public ssoLanguage Language { get; private set; }

        /// <summary>
        /// Liste af grupper som man er medlem af. Kun udvalgt iblandt dem der blev puttet ind i systemet.
        /// </summary>
        public List<string> Groups { get; private set; }

        public string Domain { get; private set; }

        public string Mail { get; private set; }

        public string Surname { get; private set; }

        public string Givenname { get; private set; }

        public string CommonName { get; private set; }

        public string OrganizationName { get; private set; }
    }

    public class ReadOnlyCollectionWithArrayArgument<T> : IList<T>
    {
        readonly IList<T> _values;

        public ReadOnlyCollectionWithArrayArgument(T[] args) =>
            _values = args ?? (IList<T>) new List<T>();

        public IEnumerator<T> GetEnumerator() =>
            _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _values.GetEnumerator();

        public void Add(T item) =>
            throw new NotImplementedException();

        public void Clear() =>
            throw new NotImplementedException();

        public bool Contains(T item) =>
            throw new NotImplementedException();

        public void CopyTo(T[] array, int arrayIndex) =>
            throw new NotImplementedException();

        public bool Remove(T item) =>
            throw new NotImplementedException();

        public int Count { get; }
        public bool IsReadOnly { get; }

        public int IndexOf(T item) =>
            throw new NotImplementedException();

        public void Insert(int index, T item) =>
            throw new NotImplementedException();

        public void RemoveAt(int index) =>
            throw new NotImplementedException();

        public T this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }

    public class ReadOnlyIntegerList : IReadOnlyCollection<int>
    {
        readonly List<int> list;

        public ReadOnlyIntegerList(List<int> l) =>
            list = l;

        public int Count => list.Count;

        public IEnumerator<int> GetEnumerator() =>
            list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    public class Array2D
    {
        public string Before { get; set; }
        public int[,] Coordinates { get; set; }
        public string After { get; set; }
    }

    public class Array3D
    {
        public string Before { get; set; }
        public int[,,] Coordinates { get; set; }
        public string After { get; set; }
    }

    public class Array3DWithConverter
    {
        public string Before { get; set; }

        [JsonProperty(ItemConverterType = typeof(IntToFloatConverter))]
        public int[,,] Coordinates { get; set; }

        public string After { get; set; }
    }

    public class GenericItem<T>
    {
        public T Value { get; set; }
    }

    public class NonGenericItem : GenericItem<string>
    {
    }

    public class GenericClass<T, TValue> : IEnumerable<T>
        where T : GenericItem<TValue>, new()
    {
        public IList<T> Items { get; set; }

        public GenericClass() =>
            Items = new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    public class NonGenericClass : GenericClass<GenericItem<string>, string>
    {
    }

    public class StringListAppenderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value);

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var existingStrings = (List<string>) existingValue;
            var newStrings = new List<string>(existingStrings);

            reader.Read();

            while (reader.TokenType != JsonToken.EndArray)
            {
                var s = (string) reader.Value;
                newStrings.Add(s);

                reader.Read();
            }

            return newStrings;
        }

        public override bool CanConvert(Type type) =>
            type == typeof(List<string>);
    }

    public class StringAppenderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value);

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var existingString = (string) existingValue;
            var newString = existingString + (string) reader.Value;

            return newString;
        }

        public override bool CanConvert(Type type) =>
            type == typeof(string);
    }
}