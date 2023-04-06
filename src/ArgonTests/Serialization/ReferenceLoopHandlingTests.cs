﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Dynamic;

// ReSharper disable UseObjectOrCollectionInitializer

public class ReferenceLoopHandlingTests : TestFixtureBase
{
    [Fact]
    public void ReferenceLoopHandlingTest()
    {
        var attribute = new JsonPropertyAttribute();
        Assert.Equal(null, attribute.defaultValueHandling);
        Assert.Equal(ReferenceLoopHandling.Error, attribute.ReferenceLoopHandling);

        attribute.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        Assert.Equal(ReferenceLoopHandling.Ignore, attribute.referenceLoopHandling);
        Assert.Equal(ReferenceLoopHandling.Ignore, attribute.ReferenceLoopHandling);
    }

    [Fact]
    public void IgnoreObjectReferenceLoop()
    {
        var o = new ReferenceLoopHandlingObjectContainerAttribute();
        o.Value = o;

        var json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        });
        Assert.Equal("{}", json);
    }

    [Fact]
    public void IgnoreObjectReferenceLoopWithPropertyOverride()
    {
        var o = new ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride();
        o.Value = o;

        var json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        });
        XUnitAssert.AreEqualNormalized("""
            {
              "Value": {
                "Value": {
                  "Value": {
                    "Value": {
                      "Value": {
                        "Value": null
                      }
                    }
                  }
                }
              }
            }
            """, json);
    }

    [Fact]
    public void IgnoreArrayReferenceLoop()
    {
        var a = new ReferenceLoopHandlingList();
        a.Add(a);

        var json = JsonConvert.SerializeObject(a, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        });
        Assert.Equal("[]", json);
    }

    [Fact]
    public void IgnoreDictionaryReferenceLoop()
    {
        var d = new ReferenceLoopHandlingDictionary();
        d.Add("First", d);

        var json = JsonConvert.SerializeObject(d, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        });
        Assert.Equal("{}", json);
    }

    [Fact]
    public void SerializePropertyItemReferenceLoopHandling()
    {
        var c = new PropertyItemReferenceLoopHandling
        {
            Text = "Text!"
        };
        c.SetData(new List<PropertyItemReferenceLoopHandling> {c});

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized("""
            {
              "Text": "Text!",
              "Data": [
                {
                  "Text": "Text!",
                  "Data": [
                    {
                      "Text": "Text!",
                      "Data": [
                        {
                          "Text": "Text!",
                          "Data": null
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """, json);
    }

    public class DictionaryDynamicObject : DynamicObject
    {
        public IDictionary<string, object> Values { get; }

        public DictionaryDynamicObject() =>
            Values = new Dictionary<string, object>();

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Values[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) =>
            Values.TryGetValue(binder.Name, out result);

        public override IEnumerable<string> GetDynamicMemberNames() =>
            Values.Keys;
    }

    [Fact]
    public void ErrorDynamicCyclicReferenceLoop()
    {
        dynamic parent = new DictionaryDynamicObject();
        dynamic child = new DictionaryDynamicObject();
        parent.child = child;
        child.parent = parent;

        var settings = new JsonSerializerSettings();

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(parent, settings),
            "Self referencing loop detected with type 'ReferenceLoopHandlingTests+DictionaryDynamicObject'. Path 'child'.");
    }

    [Fact]
    public void IgnoreDynamicCyclicReferenceLoop()
    {
        dynamic parent = new DictionaryDynamicObject();
        dynamic child = new DictionaryDynamicObject();
        parent.child = child;
        parent.name = "parent";
        child.parent = parent;
        child.name = "child";

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var c = JsonConvert.SerializeObject(parent, settings);
        Assert.Equal(@"{""child"":{""name"":""child""},""name"":""parent""}", c);
    }

    [Fact]
    public void EqualityComparer()
    {
        var account = new AccountWithEquals
        {
            Name = "main"
        };
        var manager = new AccountWithEquals
        {
            Name = "main"
        };
        account.Manager = manager;

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(account),
            "Self referencing loop detected for property 'Manager' with type 'ReferenceLoopHandlingTests+AccountWithEquals'. Path ''.");

        var json = JsonConvert.SerializeObject(account, new JsonSerializerSettings
        {
            EqualityComparer = new ReferenceEqualsEqualityComparer(),
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized("""
            {
              "Name": "main",
              "Manager": {
                "Name": "main",
                "Manager": null
              }
            }
            """, json);
    }


    public class ReferenceEqualsEqualityComparer : IEqualityComparer
    {
        bool IEqualityComparer.Equals(object x, object y) =>
            ReferenceEquals(x, y);

        int IEqualityComparer.GetHashCode(object obj) =>
            // put objects in a bucket based on their reference
            RuntimeHelpers.GetHashCode(obj);
    }

    public class AccountWithEquals
    {
        public string Name { get; set; }
        public AccountWithEquals Manager { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not AccountWithEquals a)
            {
                return false;
            }

            return Name == a.Name;
        }

        public override int GetHashCode()
        {
            if (Name == null)
            {
                return 0;
            }

            return Name.GetHashCode();
        }
    }

    public class PropertyItemReferenceLoopHandling
    {
        IList<PropertyItemReferenceLoopHandling> _data;
        int _accessCount;

        public string Text { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public IList<PropertyItemReferenceLoopHandling> Data
        {
            get
            {
                if (_accessCount >= 3)
                {
                    return null;
                }

                _accessCount++;
                return new List<PropertyItemReferenceLoopHandling>(_data);
            }
        }

        public void SetData(IList<PropertyItemReferenceLoopHandling> data) =>
            _data = data;
    }

    [JsonArray(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingList : List<ReferenceLoopHandlingList>
    {
    }

    [JsonDictionary(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingDictionary : Dictionary<string, ReferenceLoopHandlingDictionary>
    {
    }

    [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingObjectContainerAttribute
    {
        public ReferenceLoopHandlingObjectContainerAttribute Value { get; set; }
    }

    [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride
    {
        ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride _value;
        int _getCount;

        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride Value
        {
            get
            {
                if (_getCount < 5)
                {
                    _getCount++;
                    return _value;
                }

                return null;
            }
            set => _value = value;
        }
    }
}