// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1619 : TestFixtureBase
{
#if !RELEASE
    [Fact]
    public void Test()
    {
        var value = new Foo
        {
            Bar = new(@"c:\temp")
        };

        var json = JsonConvert.SerializeObject(value, new DirectoryInfoJsonConverter());
        Assert.Equal(@"{""Bar"":""c:\\temp""}", json);
    }
#endif

    public class Foo
    {
        public DirectoryInfo Bar { get; set; }
    }

    public class DirectoryInfoJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type) =>
            type == typeof(DirectoryInfo);

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                return new DirectoryInfo(s);
            }

            throw new ArgumentOutOfRangeException(nameof(reader));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not DirectoryInfo directoryInfo)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            writer.WriteValue(directoryInfo.FullName);
        }
    }
}