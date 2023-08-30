// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class PopulateTests : TestFixtureBase
{
    [Fact]
    public void PopulatePerson()
    {
        var p = new Person();

        JsonConvert.PopulateObject("""{"Name":"James"}""", p);

        Assert.Equal("James", p.Name);
    }

    [Fact]
    public void PopulateArray()
    {
        var people = new List<Person>
        {
            new() {Name = "Initial"}
        };

        JsonConvert.PopulateObject("""[{"Name":"James"}, null]""", people);

        Assert.Equal(3, people.Count);
        Assert.Equal("Initial", people[0].Name);
        Assert.Equal("James", people[1].Name);
        Assert.Equal(null, people[2]);
    }

    [Fact]
    public void PopulateStore()
    {
        var s = new Store
        {
            Color = StoreColor.Red,
            product = new()
            {
                new()
                {
                    ExpiryDate = new(2000, 12, 3, 0, 0, 0, DateTimeKind.Utc),
                    Name = "ProductName!",
                    Price = 9.9m
                }
            },
            Width = 99.99d,
            Mottos = new() {"Can do!", "We deliver!"}
        };

        var json = """
            {
              "Color": 2,
              "Established": "2013-08-14T04:38:31.000+1230",
              "Width": 99.99,
              "Employees": 999,
              "RoomsPerFloor": [
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
              "Open": false,
              "Symbol": "@",
              "Mottos": [
                "Fail whale"
              ],
              "Cost": 100980.1,
              "Escape": "\r\n\t\f\b?{\\r\\n\"'",
              "product": [
                {
                  "Name": "ProductName!",
                  "ExpiryDate": "2013-08-14T04:38:31.000+1230",
                  "Price": 9.9,
                  "Sizes": null
                }
              ]
            }
            """;

        JsonConvert.PopulateObject(json, s, new()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        });

        Assert.Equal(1, s.Mottos.Count);
        Assert.Equal("Fail whale", s.Mottos[0]);
        Assert.Equal(1, s.product.Count);

        //Assert.AreEqual("James", p.Name);
    }

    [Fact]
    public void PopulateListOfPeople()
    {
        var p = new List<Person>();

        var serializer = new JsonSerializer();
        serializer.Populate(new StringReader("""[{"Name":"James"},{"Name":"Jim"}]"""), p);

        Assert.Equal(2, p.Count);
        Assert.Equal("James", p[0].Name);
        Assert.Equal("Jim", p[1].Name);
    }

    [Fact]
    public void PopulateDictionary()
    {
        var p = new Dictionary<string, string>();

        var serializer = new JsonSerializer();
        serializer.Populate(new StringReader("""{"Name":"James"}"""), p);

        Assert.Equal(1, p.Count);
        Assert.Equal("James", p["Name"]);
    }

    [Fact]
    public void PopulateWithBadJson() =>
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.PopulateObject("1", new Person()),
            "Unexpected initial token 'Integer' when populating object. Expected JSON object or array. Path '', line 1, position 1.");
}