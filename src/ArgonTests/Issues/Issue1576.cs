// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1576 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CustomContractResolver()
        };

        var result = JsonConvert.DeserializeObject<TestClass>("""{ "Items": "11" }""", settings);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(11, result.Items[0]);
    }

    [Fact]
    public void Test_WithJsonConverterAttribute()
    {
        var result = JsonConvert.DeserializeObject<TestClassWithJsonConverter>("""{ "Items": "11" }""");

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(11, result.Items[0]);
    }

    public class TestClass
    {
        public List<int> Items { get; } = [];
    }

    public class TestClassWithJsonConverter
    {
        [JsonConverter(typeof(OneItemListJsonConverter))]
        public List<int> Items { get; } = [];
    }

    public class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member.Name == "Items")
            {
                property.Converter = new OneItemListJsonConverter();
            }

            return property;
        }
    }

    public class OneItemListJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotSupportedException();

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject(type, serializer);
            }

            var array = new JArray {token};

            var list = array.ToObject(type, serializer) as IEnumerable;

            if (list != null && existingValue is IList existing)
            {
                foreach (var item in list)
                {
                    existing.Add(item);
                }
            }

            return list;
        }

        public override bool CanConvert(Type type) =>
            typeof(ICollection).IsAssignableFrom(type);
    }
}