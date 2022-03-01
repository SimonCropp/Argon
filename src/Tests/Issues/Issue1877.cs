// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1877
{
    [Fact]
    public void Test()
    {
        var f2 = new Fubar2
        {
            Version = new Version("3.0")
        };
        ((Fubar) f2).Version = new Version("4.0");

        var s = JsonConvert.SerializeObject(f2, new JsonSerializerSettings
        {
            Converters = { new VersionConverter() }
        });
        Assert.Equal(@"{""Version"":""4.0""}", s);

        var f3 = JsonConvert.DeserializeObject<Fubar2>(s, new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = { new VersionConverter() }
        });

        Assert.Equal(2, f3.Version.Major);
        Assert.Equal(4, ((Fubar) f3).Version.Major);
    }

    class Fubar
    {
        public Version Version { get; set; } = new("1.0");

        // ...
    }

    class Fubar2 : Fubar
    {
        [JsonIgnore]
        public new Version Version { get; set; } = new("2.0");

        // ...
    }
}