using Xunit;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Converters;

public class KeyValuePairConverterTests : TestFixtureBase
{
    [Fact]
    public void SerializeUsingInternalConverter()
    {
        var contractResolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)contractResolver.ResolveContract(typeof(KeyValuePair<string, int>));

        Xunit.Assert.Equal(typeof(KeyValuePairConverter), contract.InternalConverter.GetType());

        IList<KeyValuePair<string, int>> values = new List<KeyValuePair<string, int>>
        {
            new("123", 123),
            new("456", 456)
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented);

        StringAssert.AreEqual(@"[
  {
    ""Key"": ""123"",
    ""Value"": 123
  },
  {
    ""Key"": ""456"",
    ""Value"": 456
  }
]", json);

        var v2 = JsonConvert.DeserializeObject<IList<KeyValuePair<string, int>>>(json);

        Xunit.Assert.Equal(2, v2.Count);
        Xunit.Assert.Equal("123", v2[0].Key);
        Xunit.Assert.Equal(123, v2[0].Value);
        Xunit.Assert.Equal("456", v2[1].Key);
        Xunit.Assert.Equal(456, v2[1].Value);
    }

    [Fact]
    public void DeserializeUnexpectedEnd()
    {
        ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<KeyValuePair<string, int>>(@"{""Key"": ""123"","), "Unexpected end when reading JSON. Path 'Key', line 1, position 14.");
    }
}