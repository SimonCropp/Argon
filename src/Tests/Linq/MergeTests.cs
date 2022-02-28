﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


public class MergeTests : TestFixtureBase
{
    [Fact]
    public void MergeInvalidObject()
    {
        var a = new JObject();

        XUnitAssert.Throws<ArgumentException>(
            () => a.Merge(new Version()),
            @"Could not determine JSON object type for type System.Version.
Parameter name: content",
            @"Could not determine JSON object type for type System.Version. (Parameter 'content')");
    }

    [Fact]
    public void MergeArraySelf()
    {
        var a = new JArray { "1", "2" };
        a.Merge(a, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        Assert.Equal(new JArray { "1", "2" }, a);
    }

    [Fact]
    public void MergeObjectSelf()
    {
        var a = new JObject
        {
            ["1"] = 1,
            ["2"] = 2
        };
        a.Merge(a, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        Assert.Equal(new JObject
        {
            ["1"] = 1,
            ["2"] = 2
        }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Replace()
    {
        var a = new JArray { "1", "2" };
        a.Merge(new[] { "3", "4" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        Assert.Equal(new JArray { "3", "4" }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Concat()
    {
        var a = new JArray { "1", "2" };
        a.Merge(new[] { "3", "4" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Concat });
        Assert.Equal(new JArray { "1", "2", "3", "4" }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Union()
    {
        var a = new JArray { "1", "2" };
        a.Merge(new[] { "2", "3", "4" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
        Assert.Equal(new JArray { "1", "2", "3", "4" }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Merge()
    {
        var a = new JArray { "1", "2" };
        a.Merge(new[] { "2" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge });
        Assert.Equal(new JArray { "2", "2" }, a);
    }

    [Fact]
    public void MergeNullString()
    {
        var a = new JObject { ["a"] = 1 };
        var b = new JObject { ["a"] = false ? "2" : null };
        a.Merge(b);

        Assert.Equal(1, (int)a["a"]);
    }

    [Fact]
    public void MergeObjectProperty()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Property1 = 1
        });
        var right = (JObject)JToken.FromObject(new
        {
            Property2 = 2
        });

        left.Merge(right);

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Property1"": 1,
  ""Property2"": 2
}", json);
    }

    [Fact]
    public void MergeChildObject()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Property1 = new { SubProperty1 = 1 }
        });
        var right = (JObject)JToken.FromObject(new
        {
            Property1 = new { SubProperty2 = 2 }
        });

        left.Merge(right);

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Property1"": {
    ""SubProperty1"": 1,
    ""SubProperty2"": 2
  }
}", json);
    }

    [Fact]
    public void MergeMismatchedTypesRoot()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Property1 = new { SubProperty1 = 1 }
        });
        var right = (JArray)JToken.FromObject(new object[]
        {
            new { Property1 = 1 },
            new { Property1 = 1 }
        });

        left.Merge(right);

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Property1"": {
    ""SubProperty1"": 1
  }
}", json);
    }

    [Fact]
    public void MergeMultipleObjects()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Property1 = new { SubProperty1 = 1 }
        });
        var right = (JObject)JToken.FromObject(new
        {
            Property1 = new { SubProperty2 = 2 },
            Property2 = 2
        });

        left.Merge(right);

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Property1"": {
    ""SubProperty1"": 1,
    ""SubProperty2"": 2
  },
  ""Property2"": 2
}", json);
    }

    [Fact]
    public void MergeArray()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                new
                {
                    Property1 = new
                    {
                        Property1 = 1,
                        Property2 = 2,
                        Property3 = 3,
                        Property4 = 4,
                        Property5 = (object)null
                    }
                },
                new { },
                3,
                null,
                5,
                null
            }
        });
        var right = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                new
                {
                    Property1 = new
                    {
                        Property1 = (object)null,
                        Property2 = 3,
                        Property3 = new
                        {
                        },
                        Property5 = (object)null
                    }
                },
                null,
                null,
                4,
                5.1,
                null,
                new
                {
                    Property1 = 1
                }
            }
        });

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Merge
        });

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Array1"": [
    {
      ""Property1"": {
        ""Property1"": 1,
        ""Property2"": 3,
        ""Property3"": {},
        ""Property4"": 4,
        ""Property5"": null
      }
    },
    {},
    3,
    4,
    5.1,
    null,
    {
      ""Property1"": 1
    }
  ]
}", json);
    }

    [Fact]
    public void ConcatArray()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                new { Property1 = 1 },
                new { Property1 = 1 }
            }
        });
        var right = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                new { Property1 = 1 },
                new { Property2 = 2 },
                new { Property3 = 3 }
            }
        });

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Concat
        });

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Array1"": [
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property2"": 2
    },
    {
      ""Property3"": 3
    }
  ]
}", json);
    }

    [Fact]
    public void MergeMismatchingTypesInArray()
    {
        var left = (JArray)JToken.FromObject(new object[]
        {
            true,
            null,
            new { Property1 = 1 },
            new object[] { 1 },
            new { Property1 = 1 },
            1,
            new object[] { 1 }
        });
        var right = (JArray)JToken.FromObject(new object[]
        {
            1,
            5,
            new object[] { 1 },
            new { Property1 = 1 },
            true,
            new { Property1 = 1 },
            null
        });

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Merge
        });

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"[
  1,
  5,
  {
    ""Property1"": 1
  },
  [
    1
  ],
  {
    ""Property1"": 1
  },
  {
    ""Property1"": 1
  },
  [
    1
  ]
]", json);
    }

    [Fact]
    public void MergeMismatchingTypesInObject()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Property1 = new object[]
            {
                1
            },
            Property2 = new object[]
            {
                1
            },
            Property3 = true,
            Property4 = true
        });
        var right = (JObject)JToken.FromObject(new
        {
            Property1 = new { Nested = true },
            Property2 = true,
            Property3 = new object[]
            {
                1
            },
            Property4 = (object)null
        });

        left.Merge(right);

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Property1"": {
    ""Nested"": true
  },
  ""Property2"": true,
  ""Property3"": [
    1
  ],
  ""Property4"": true
}", json);
    }

    [Fact]
    public void MergeArrayOverwrite_Nested()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                1,
                2,
                3
            }
        });
        var right = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                4,
                5
            }
        });

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace
        });

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Array1"": [
    4,
    5
  ]
}", json);
    }

    [Fact]
    public void MergeArrayOverwrite_Root()
    {
        var left = (JArray)JToken.FromObject(new object[]
        {
            1,
            2,
            3
        });
        var right = (JArray)JToken.FromObject(new object[]
        {
            4,
            5
        });

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace
        });

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"[
  4,
  5
]", json);
    }

    [Fact]
    public void UnionArrays()
    {
        var left = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                new { Property1 = 1 },
                new { Property1 = 1 }
            }
        });
        var right = (JObject)JToken.FromObject(new
        {
            Array1 = new object[]
            {
                new { Property1 = 1 },
                new { Property2 = 2 },
                new { Property3 = 3 }
            }
        });

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        var json = left.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""Array1"": [
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property2"": 2
    },
    {
      ""Property3"": 3
    }
  ]
}", json);
    }

    [Fact]
    public void MergeJProperty()
    {
        var p1 = new JProperty("p1", 1);
        var p2 = new JProperty("p2", 2);

        p1.Merge(p2);
        Assert.Equal(2, (int)p1.Value);

        var p3 = new JProperty("p3");

        p1.Merge(p3);
        Assert.Equal(2, (int)p1.Value);

        var p4 = new JProperty("p4", null);

        p1.Merge(p4);
        Assert.Equal(2, (int)p1.Value);
    }

    [Fact]
    public void MergeNullValue()
    {
        var source = new JObject
        {
            {"Property1", "value"},
            {"Property2", new JObject()},
            {"Property3", JValue.CreateNull()},
            {"Property4", JValue.CreateUndefined()},
            {"Property5", new JArray()}
        };

        var patch = JObject.Parse("{Property1: null, Property2: null, Property3: null, Property4: null, Property5: null}");

        source.Merge(patch, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Merge
        });

        Assert.NotNull(source["Property1"]);
        Assert.Equal(JTokenType.Null, source["Property1"].Type);
        Assert.NotNull(source["Property2"]);
        Assert.Equal(JTokenType.Null, source["Property2"].Type);
        Assert.NotNull(source["Property3"]);
        Assert.Equal(JTokenType.Null, source["Property3"].Type);
        Assert.NotNull(source["Property4"]);
        Assert.Equal(JTokenType.Null, source["Property4"].Type);
        Assert.NotNull(source["Property5"]);
        Assert.Equal(JTokenType.Null, source["Property5"].Type);
    }

    [Fact]
    public void MergeNullValueHandling_Array()
    {
        var originalJson = @"{
  ""Bar"": [
    ""a"",
    ""b"",
    ""c""
  ]
}";
        var newJson = @"{
  ""Bar"": null
}";

        var oldFoo = JObject.Parse(originalJson);
        var newFoo = JObject.Parse(newJson);

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        });

        XUnitAssert.AreEqualNormalized(originalJson, oldFoo.ToString());

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Merge
        });

        XUnitAssert.AreEqualNormalized(newJson, newFoo.ToString());
    }

    [Fact]
    public void MergeNullValueHandling_Object()
    {
        var originalJson = @"{
  ""Bar"": {}
}";
        var newJson = @"{
  ""Bar"": null
}";

        var oldFoo = JObject.Parse(originalJson);
        var newFoo = JObject.Parse(newJson);

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        });

        XUnitAssert.AreEqualNormalized(originalJson, oldFoo.ToString());

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Merge
        });

        XUnitAssert.AreEqualNormalized(newJson, newFoo.ToString());
    }

    [Fact]
    public void Merge_IgnorePropertyCase()
    {
        var o1 = JObject.Parse(@"{
                                          'Id': '1',
                                          'Words': [ 'User' ]
                                        }");
        var o2 = JObject.Parse(@"{
                                            'Id': '1',
                                            'words': [ 'Name' ]
                                        }");

        o1.Merge(o2, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Concat,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
            PropertyNameComparison = StringComparison.OrdinalIgnoreCase
        });

        Assert.Null(o1["words"]);
        Assert.NotNull(o1["Words"]);

        var words = (JArray)o1["Words"];
        Assert.Equal("User", (string)words[0]);
        Assert.Equal("Name", (string)words[1]);
    }

    [Fact]
    public void MergeSettingsComparisonDefault()
    {
        var settings = new JsonMergeSettings();

        Assert.Equal(StringComparison.Ordinal, settings.PropertyNameComparison);
    }
}