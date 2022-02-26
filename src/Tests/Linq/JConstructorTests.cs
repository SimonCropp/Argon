// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


public class JConstructorTests : TestFixtureBase
{
    [Fact]
    public void Load()
    {
        JsonReader reader = new JsonTextReader(new StringReader("new Date(123)"));
        reader.Read();

        var constructor = JConstructor.Load(reader);
        Assert.Equal("Date", constructor.Name);
        Assert.True(JToken.DeepEquals(new JValue(123), constructor.Values().ElementAt(0)));
    }

    [Fact]
    public void CreateWithMultiValue()
    {
        var constructor = new JConstructor("Test", new List<int> {1, 2, 3});
        Assert.Equal("Test", constructor.Name);
        Assert.Equal(3, constructor.Children().Count());
        Assert.Equal(1, (int) constructor.Children().ElementAt(0));
        Assert.Equal(2, (int) constructor.Children().ElementAt(1));
        Assert.Equal(3, (int) constructor.Children().ElementAt(2));
    }

    [Fact]
    public void Iterate()
    {
        var c = new JConstructor("MrConstructor", 1, 2, 3, 4, 5);

        var i = 1;
        foreach (var token in c)
        {
            Assert.Equal(i, (int) token);
            i++;
        }
    }

    [Fact]
    public void SetValueWithInvalidIndex()
    {
        XUnitAssert.Throws<ArgumentException>(
            () =>
            {
                var c = new JConstructor
                {
                    ["badvalue"] = new JValue(3)
                };
            },
            @"Set JConstructor values with invalid key value: ""badvalue"". Argument position index expected.");
    }

    [Fact]
    public void SetValue()
    {
        object key = 0;

        var c = new JConstructor
        {
            Name = "con"
        };
        c.Add(null);
        c[key] = new JValue(3);

        Assert.Equal(3, (int) c[key]);
    }
}