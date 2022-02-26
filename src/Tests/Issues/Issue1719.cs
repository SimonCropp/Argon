// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

public class Issue1719 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var a = JsonConvert.DeserializeObject<ExtensionDataTestClass>("{\"E\":null}", new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });

        Assert.Null(a.PropertyBag);
    }

    [Fact]
    public void Test_PreviousWorkaround()
    {
        var a = JsonConvert.DeserializeObject<ExtensionDataTestClassWorkaround>("{\"E\":null}", new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });

        Assert.Null(a.PropertyBag);
    }

    [Fact]
    public void Test_DefaultValue()
    {
        var a = JsonConvert.DeserializeObject<ExtensionDataWithDefaultValueTestClass>("{\"E\":2}", new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
        });

        Assert.Null(a.PropertyBag);
    }

    class ExtensionDataTestClass
    {
        public B? E { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> PropertyBag { get; set; }
    }

    class ExtensionDataWithDefaultValueTestClass
    {
        [DefaultValue(2)]
        public int? E { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> PropertyBag { get; set; }
    }

    enum B
    {
        One,
        Two
    }

    class ExtensionDataTestClassWorkaround
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Include)]
        public B? E { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> PropertyBag { get; set; }
    }
}