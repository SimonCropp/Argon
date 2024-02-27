// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2504 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var jsontext = NestedJson.Build(150);

        var o = JsonConvert.DeserializeObject<TestObject>(
            jsontext,
            new JsonSerializerSettings
            {
                Converters = [new TestConverter()],
                MaxDepth = 150
            });

        Assert.Equal(150, GetDepth(o.Children));
    }

    [Fact]
    public void Test_Failure()
    {
        var jsontext = NestedJson.Build(150);

        var expectedMessage = "The reader's MaxDepth of 100 has been exceeded. Path '0.1.2.3.4.5.6.7.8.9.10.11.12.13.14.15.16.17.18.19.20.21.22.23.24.25.26.27.28.29.30.31.32.33.34.35.36.37.38.39.40.41.42.43.44.45.46.47.48.49.50.51.52.53.54.55.56.57.58.59.60.61.62.63.64.65.66.67.68.69.70.71.72.73.74.75.76.77.78.79.80.81.82.83.84.85.86.87.88.89.90.91.92.93.94.95.96.97.98.99', line 101, position 207.";

        var settings = new JsonSerializerSettings
        {
            Converters = [new TestConverter()],
            MaxDepth = 100
        };
        var exception = Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<TestObject>(jsontext, settings));
        Assert.Equal(expectedMessage, exception.Message);
    }

    static int GetDepth(JToken o)
    {
        var depth = 1;
        while (o.First != null)
        {
            o = o.First;
            if (o.Type == JTokenType.Object)
            {
                depth++;
            }
        }

        return depth;
    }

    class TestObject
    {
        public JToken Children { get; set; }
    }

    class TestConverter : JsonConverter
    {
        public override bool CanConvert(Type type) =>
            type == typeof(TestObject);

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            var newToken = token.ToObject<JObject>(serializer);

            return new TestObject
            {
                Children = newToken
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}