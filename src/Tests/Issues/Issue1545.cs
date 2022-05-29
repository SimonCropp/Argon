// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1545 : TestFixtureBase
{
    [Fact]
    public void Test_Populate()
    {
        var json = @"{
                ""array"": [
                    /* comment0 */
                    {
                        ""value"": ""item1""
                    },
                    /* comment1 */
                    {
                        ""value"": ""item2""
                    }
                    /* comment2 */
                ]
            }";

        var s = JsonConvert.DeserializeObject<Simple>(json);
        Assert.Equal(2, s.Array.Length);
        Assert.Equal("item1", s.Array[0].Value);
        Assert.Equal("item2", s.Array[1].Value);
    }

    [Fact]
    public void Test_Multidimensional()
    {
        var json = @"[
                /* comment0 */
                [1,2,3],
                /* comment1 */
                [
                    /* comment2 */
                    4,
                    /* comment3 */
                    5,
                    /* comment4 */
                    6
                ]
                /* comment5 */
            ]";

        var s = JsonConvert.DeserializeObject<int[,]>(json);
        Assert.Equal(6, s.Length);
        Assert.Equal(1, s[0, 0]);
        Assert.Equal(2, s[0, 1]);
        Assert.Equal(3, s[0, 2]);
        Assert.Equal(4, s[1, 0]);
        Assert.Equal(5, s[1, 1]);
        Assert.Equal(6, s[1, 2]);
    }
}

public class Simple
{
    [JsonProperty(Required = Required.Always)]
    public SimpleObject[] Array { get; set; }
}

[JsonConverter(typeof(LineInfoConverter))]
public class SimpleObject : JsonLineInfo
{
    public string Value { get; set; }
}

public class JsonLineInfo
{
    [JsonIgnore]
    public int? LineNumber { get; set; }

    [JsonIgnore]
    public int? LinePosition { get; set; }
}

public class LineInfoConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException("Converter is not writable. Method should not be invoked");

    public override bool CanConvert(Type type) =>
        typeof(JsonLineInfo).IsAssignableFrom(type);

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var lineInfoObject = Activator.CreateInstance(type) as JsonLineInfo;
        serializer.Populate(reader, lineInfoObject);

        var jsonLineInfo = reader as IJsonLineInfo;
        if (jsonLineInfo != null && jsonLineInfo.HasLineInfo())
        {
            lineInfoObject.LineNumber = jsonLineInfo.LineNumber;
            lineInfoObject.LinePosition = jsonLineInfo.LinePosition;
        }

        return lineInfoObject;
    }
}