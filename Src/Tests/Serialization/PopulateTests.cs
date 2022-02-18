﻿#region License
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

using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Serialization;

public class PopulateTests : TestFixtureBase
{
    [Fact]
    public void PopulatePerson()
    {
        var p = new Person();

        JsonConvert.PopulateObject(@"{""Name"":""James""}", p);

        Assert.AreEqual("James", p.Name);
    }

    [Fact]
    public void PopulateArray()
    {
        IList<Person> people = new List<Person>
        {
            new() { Name = "Initial" }
        };

        JsonConvert.PopulateObject(@"[{""Name"":""James""}, null]", people);

        Assert.AreEqual(3, people.Count);
        Assert.AreEqual("Initial", people[0].Name);
        Assert.AreEqual("James", people[1].Name);
        Assert.AreEqual(null, people[2]);
    }

    [Fact]
    public void PopulateStore()
    {
        var s = new Store
        {
            Color = StoreColor.Red,
            product = new List<Product>
            {
                new()
                {
                    ExpiryDate = new DateTime(2000, 12, 3, 0, 0, 0, DateTimeKind.Utc),
                    Name = "ProductName!",
                    Price = 9.9m
                }
            },
            Width = 99.99d,
            Mottos = new List<string> { "Can do!", "We deliver!" }
        };

        var json = @"{
  ""Color"": 2,
  ""Establised"": ""\/Date(1264122061000+0000)\/"",
  ""Width"": 99.99,
  ""Employees"": 999,
  ""RoomsPerFloor"": [
    1,
    2,
    3,
    4,
    5,
    6,
    7,
    8,
    9
  ],
  ""Open"": false,
  ""Symbol"": ""@"",
  ""Mottos"": [
    ""Fail whale""
  ],
  ""Cost"": 100980.1,
  ""Escape"": ""\r\n\t\f\b?{\\r\\n\""'"",
  ""product"": [
    {
      ""Name"": ""ProductName!"",
      ""ExpiryDate"": ""\/Date(975801600000)\/"",
      ""Price"": 9.9,
      ""Sizes"": null
    }
  ]
}";

        JsonConvert.PopulateObject(json, s, new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        });

        Assert.AreEqual(1, s.Mottos.Count);
        Assert.AreEqual("Fail whale", s.Mottos[0]);
        Assert.AreEqual(1, s.product.Count);

        //Assert.AreEqual("James", p.Name);
    }

    [Fact]
    public void PopulateListOfPeople()
    {
        var p = new List<Person>();

        var serializer = new JsonSerializer();
        serializer.Populate(new StringReader(@"[{""Name"":""James""},{""Name"":""Jim""}]"), p);

        Assert.AreEqual(2, p.Count);
        Assert.AreEqual("James", p[0].Name);
        Assert.AreEqual("Jim", p[1].Name);
    }

    [Fact]
    public void PopulateDictionary()
    {
        var p = new Dictionary<string, string>();

        var serializer = new JsonSerializer();
        serializer.Populate(new StringReader(@"{""Name"":""James""}"), p);

        Assert.AreEqual(1, p.Count);
        Assert.AreEqual("James", p["Name"]);
    }

    [Fact]
    public void PopulateWithBadJson()
    {
        ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.PopulateObject("1", new Person()); }, "Unexpected initial token 'Integer' when populating object. Expected JSON object or array. Path '', line 1, position 1.");
    }
}