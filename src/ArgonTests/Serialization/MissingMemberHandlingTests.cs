﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class MissingMemberHandlingTests : TestFixtureBase
{
    [Fact]
    public void MissingMemberDeserialize()
    {
        var product = new Product
        {
            Name = "Apple",
            ExpiryDate = new(2008, 12, 28),
            Price = 3.99M,
            Sizes = new[] {"Small", "Medium", "Large"}
        };

        var output = JsonConvert.SerializeObject(product, Formatting.Indented);
        //{
        //  "Name": "Apple",
        //  "ExpiryDate": new Date(1230422400000),
        //  "Price": 3.99,
        //  "Sizes": [
        //    "Small",
        //    "Medium",
        //    "Large"
        //  ]
        //}

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject(output, typeof(ProductShort), new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error}),
            "Could not find member 'Price' on object of type 'ProductShort'. Path 'Price', line 4, position 10.");
    }

    [Fact]
    public void MissingMemberDeserializeOkay()
    {
        var product = new Product
        {
            Name = "Apple",
            ExpiryDate = new(2008, 12, 28),
            Price = 3.99M,
            Sizes = new[] {"Small", "Medium", "Large"}
        };

        var output = JsonConvert.SerializeObject(product);
        //{
        //  "Name": "Apple",
        //  "ExpiryDate": new Date(1230422400000),
        //  "Price": 3.99,
        //  "Sizes": [
        //    "Small",
        //    "Medium",
        //    "Large"
        //  ]
        //}

        var serializer = new JsonSerializer
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        object deserializedValue;

        using (JsonReader reader = new JsonTextReader(new StringReader(output)))
        {
            deserializedValue = serializer.Deserialize(reader, typeof(ProductShort));
        }

        var deserializedProductShort = (ProductShort) deserializedValue;

        Assert.Equal("Apple", deserializedProductShort.Name);
        Assert.Equal(new(2008, 12, 28), deserializedProductShort.ExpiryDate);
        Assert.Equal("Small", deserializedProductShort.Sizes[0]);
        Assert.Equal("Medium", deserializedProductShort.Sizes[1]);
        Assert.Equal("Large", deserializedProductShort.Sizes[2]);
    }

    [Fact]
    public void MissingMemberIgnoreComplexValue()
    {
        var serializer = new JsonSerializer {MissingMemberHandling = MissingMemberHandling.Ignore};

        var response = "{PreProperty:1, DateProperty:'2000-12-05T05:07:59-10:00', PostProperty:2}";

        var myClass = (MyClass) serializer.Deserialize(new StringReader(response), typeof(MyClass));

        Assert.Equal(1, myClass.PreProperty);
        Assert.Equal(2, myClass.PostProperty);
    }

    [Fact]
    public void CaseInsensitive()
    {
        var json = @"{""height"":1}";

        var c = JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error});

        Assert.Equal(1d, c.Height);
    }

    [Fact]
    public void MissingMember()
    {
        var json = """{"Missing":1}""";

        var settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };
        XUnitAssert.Throws<JsonSerializationException>(
            () =>
            {
                JsonConvert.DeserializeObject<DoubleClass>(json, settings);
            },
            "Could not find member 'Missing' on object of type 'DoubleClass'. Path 'Missing', line 1, position 11.");
    }

    [Fact]
    public void MissingJson()
    {
        var json = "{}";

        JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error
        });
    }

    [Fact]
    public void MissingErrorAttribute()
    {
        var json = """{"Missing":1}""";

        var settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<NameWithMissingError>(json, settings),
            "Could not find member 'Missing' on object of type 'NameWithMissingError'. Path 'Missing', line 1, position 11.");
    }

    public class NameWithMissingError
    {
        public string First { get; set; }
    }

    public class Name
    {
        public string First { get; set; }
    }

    public class Person
    {
        public Name Name { get; set; }
    }
}