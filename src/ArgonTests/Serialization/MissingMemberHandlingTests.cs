// Copyright (c) 2007 James Newton-King. All rights reserved.
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
    public void MissingMemeber()
    {
        var json = @"{""Missing"":1}";

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
        var json = @"{""Missing"":1}";

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<NameWithMissingError>(json),
            "Could not find member 'Missing' on object of type 'NameWithMissingError'. Path 'Missing', line 1, position 11.");
    }

    [JsonObject(MissingMemberHandling = MissingMemberHandling.Error)]
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

    [Fact]
    public void MissingMemberHandling_RootObject()
    {
        var errors = new List<string>();

        var settings = new JsonSerializerSettings
        {
            //This works on properties but not on a objects property.
            /* So nameERROR:{"first":"ni"} would throw. The payload name:{"firstERROR":"hi"} would not */
            MissingMemberHandling = MissingMemberHandling.Error,
            Error = (currentObject, originalObject, location, exception, markAsHandled) =>
            {
                // A more concrete error type would be nice but we are limited by Newtonsofts library here.
                errors.Add(exception.Message);
                markAsHandled();
            }
        };

        var p = new Person();

        JsonConvert.PopulateObject(@"{nameERROR:{""first"":""hi""}}", p, settings);

        Assert.Equal(1, errors.Count);
        Assert.Equal("Could not find member 'nameERROR' on object of type 'Person'. Path 'nameERROR', line 1, position 11.", errors[0]);
    }

    [Fact]
    public void MissingMemberHandling_InnerObject()
    {
        var errors = new List<string>();

        var settings = new JsonSerializerSettings
        {
            //This works on properties but not on a objects property.
            /* So nameERROR:{"first":"ni"} would throw. The payload name:{"firstERROR":"hi"} would not */
            MissingMemberHandling = MissingMemberHandling.Error,
            Error = (_, _, _, exception, markAsHandled) =>
            {
                // A more concrete error type would be nice but we are limited by Newtonsofts library here.
                errors.Add(exception.Message);
                markAsHandled();
            }
        };

        var p = new Person();

        JsonConvert.PopulateObject(@"{name:{""firstERROR"":""hi""}}", p, settings);

        Assert.Equal(1, errors.Count);
        Assert.Equal("Could not find member 'firstERROR' on object of type 'Name'. Path 'name.firstERROR', line 1, position 20.", errors[0]);
    }

    [JsonObject(MissingMemberHandling = MissingMemberHandling.Ignore)]
    public class SimpleExtendableObject
    {
        [JsonExtensionData] public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
    }

    public class ObjectWithExtendableChild
    {
        public SimpleExtendableObject Data;
    }

    [Fact]
    public void TestMissingMemberHandlingForDirectObjects()
    {
        var json = @"{""extensionData1"": [1,2,3]}";
        var settings = new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error};
        var e2 = JsonConvert.DeserializeObject<SimpleExtendableObject>(json, settings);
        var o1 = (JArray) e2.Data["extensionData1"];
        Assert.Equal(JTokenType.Array, o1.Type);
    }

    [Fact]
    public void TestMissingMemberHandlingForChildObjects()
    {
        var json = @"{""Data"":{""extensionData1"": [1,2,3]}}";
        var settings = new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error};
        var e3 = JsonConvert.DeserializeObject<ObjectWithExtendableChild>(json, settings);
        var o1 = (JArray) e3.Data.Data["extensionData1"];
        Assert.Equal(JTokenType.Array, o1.Type);
    }

    [Fact]
    public void TestMissingMemberHandlingForChildObjectsWithInvalidData()
    {
        var json = @"{""InvalidData"":{""extensionData1"": [1,2,3]}}";

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<ObjectWithExtendableChild>(json, new JsonSerializerSettings {MissingMemberHandling = MissingMemberHandling.Error}),
            "Could not find member 'InvalidData' on object of type 'ObjectWithExtendableChild'. Path 'InvalidData', line 1, position 15.");
    }
}