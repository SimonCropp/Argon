// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2082
{
    [Fact]
    public void Test()
    {
        var namingStrategy = new CamelCaseNamingStrategy(processDictionaryKeys: true, overrideSpecifiedNames: false);

        var c = new TestClass { Value = TestEnum.UpperCaseName };
        var json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = namingStrategy
            },
            Converters = new() { new StringEnumConverter { NamingStrategy = namingStrategy } }
        });

        Assert.Equal(@"{""value"":""UPPER_CASE_NAME""}", json);
    }

    public class TestClass
    {
        public TestEnum Value { get; set; }
    }

    public enum TestEnum
    {
        [EnumMember(Value = "UPPER_CASE_NAME")]
        UpperCaseName
    }
}