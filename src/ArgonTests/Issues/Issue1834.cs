// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1834 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var json = "{'foo':'test!'}";
        var c = JsonConvert.DeserializeObject<ItemWithJsonConstructor>(json);

        Assert.Null(c.ExtensionData);
    }

    [Fact]
    public void Test_UnsetRequired()
    {
        var json = "{'foo':'test!'}";
        var c = JsonConvert.DeserializeObject<ItemWithJsonConstructorAndDefaultValue>(json);

        Assert.Null(c.ExtensionData);
    }

    public class ItemWithJsonConstructor
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData;

        [Argon.JsonConstructor]
        ItemWithJsonConstructor(string foo) =>
            Foo = foo;

        [JsonProperty(PropertyName = "foo", Required = Required.Always)]
        public string Foo { get; set; }
    }

    public class ItemWithJsonConstructorAndDefaultValue
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData;

        [Argon.JsonConstructor]
        ItemWithJsonConstructorAndDefaultValue(string foo) =>
            Foo = foo;

        [JsonProperty("foo")]
        public string Foo { get; set; }

        [JsonProperty(PropertyName = "bar", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [System.ComponentModel.DefaultValue("default")]
        public string Bar { get; set; }
    }
}