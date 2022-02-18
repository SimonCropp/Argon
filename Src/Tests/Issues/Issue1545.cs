#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Xunit;

namespace Argon.Tests.Issues;

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
        Xunit.Assert.Equal(2, s.Array.Length);
        Xunit.Assert.Equal("item1", s.Array[0].Value);
        Xunit.Assert.Equal("item2", s.Array[1].Value);
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
        Xunit.Assert.Equal(6, s.Length);
        Xunit.Assert.Equal(1, s[0, 0]);
        Xunit.Assert.Equal(2, s[0, 1]);
        Xunit.Assert.Equal(3, s[0, 2]);
        Xunit.Assert.Equal(4, s[1, 0]);
        Xunit.Assert.Equal(5, s[1, 1]);
        Xunit.Assert.Equal(6, s[1, 2]);
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

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Converter is not writable. Method should not be invoked");
    }

    public override bool CanConvert(Type objectType)
    {
#if NET5_0_OR_GREATER
            return typeof(JsonLineInfo).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
#else
        return typeof(JsonLineInfo).IsAssignableFrom(objectType);
#endif
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var lineInfoObject = Activator.CreateInstance(objectType) as JsonLineInfo;
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