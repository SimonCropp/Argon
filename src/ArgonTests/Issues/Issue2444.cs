// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2444
{
    [Fact]
    public void Test()
    {
        var namingStrategy = new SnakeCaseNamingStrategy();
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = namingStrategy
            }
        };

        var json = """{"dict":{"value1":"a","text_value":"b"}}""";
        var c = JsonConvert.DeserializeObject<DataClass>(json, settings);

        Assert.Equal(2, c.Dict.Count);
        Assert.Equal("a", c.Dict[MyEnum.Value1]);
        Assert.Equal("b", c.Dict[MyEnum.TextValue]);

        var json1 = """{"dict":{"Value1":"a","TextValue":"b"}}""";
        var c1 = JsonConvert.DeserializeObject<DataClass>(json1, settings);

        Assert.Equal(2, c1.Dict.Count);
        Assert.Equal("a", c1.Dict[MyEnum.Value1]);
        Assert.Equal("b", c1.Dict[MyEnum.TextValue]);

        // Non-dictionary values should still error
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<List<MyEnum>>(
                """["text_value"]""",
                settings),
            """Error converting value "text_value" to type 'Issue2444+MyEnum'. Path '[0]', line 1, position 13.""");
    }

    public enum MyEnum
    {
        Value1,
        TextValue
    }

    public class DataClass
    {
        public Dictionary<MyEnum, string> Dict { get; set; }
    }
}