// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2450
{
    [Fact]
    public void Test()
    {
        var resolver = new DefaultContractResolver();

        var contract = resolver.ResolveContract(typeof(Dict));
        Assert.True(contract is JsonDictionaryContract);

        contract = resolver.ResolveContract(typeof(Dict?));
        Assert.True(contract is JsonDictionaryContract);
    }

    [Fact]
    public void Test_Serialize()
    {
        var d = new Dict(new Dictionary<string, object>
        {
            ["prop1"] = 1,
            ["prop2"] = 2
        });

        var json = JsonConvert.SerializeObject(d);
        Assert.Equal("""{"prop1":1,"prop2":2}""", json);
    }

    [Fact]
    public void Test_Deserialize()
    {
        var json = """{"prop1":1,"prop2":2}""";

        var d = JsonConvert.DeserializeObject<Dict?>(json);
        Assert.Equal((long)1, d.Value["prop1"]);
        Assert.Equal((long)2, d.Value["prop2"]);
    }

    public struct Dict(IDictionary<string, object> dict) :
        IReadOnlyDictionary<string, object>
    {
        public object this[string key] => dict[key];
        public IEnumerable<string> Keys => dict.Keys;
        public IEnumerable<object> Values => dict.Values;
        public int Count => dict.Count;
        public bool ContainsKey(string key) => dict.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => dict.GetEnumerator();
        public bool TryGetValue(string key, out object value) => dict.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}