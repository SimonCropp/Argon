// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using BenchmarkDotNet.Attributes;
using TestObjects;

public class DeserializeBenchmarks
{
    static readonly string LargeJsonText;
    static readonly string FloatArrayJson;

    static DeserializeBenchmarks()
    {
        LargeJsonText = File.ReadAllText("large.json");

        FloatArrayJson = new JArray(Enumerable.Range(0, 5000).Select(i => i * 1.1m)).ToString(Formatting.None);
    }

    [Benchmark]
    public IList<RootObject> DeserializeLargeJsonText()
    {
        return JsonConvert.DeserializeObject<IList<RootObject>>(LargeJsonText);
    }

    [Benchmark]
    public IList<RootObject> DeserializeLargeJsonFile()
    {
        using var jsonFile = File.OpenText("large.json");
        using var jsonTextReader = new JsonTextReader(jsonFile);
        var serializer = new JsonSerializer();
        return serializer.Deserialize<IList<RootObject>>(jsonTextReader);
    }

    [Benchmark]
    public IList<double> DeserializeDoubleList()
    {
        return JsonConvert.DeserializeObject<IList<double>>(FloatArrayJson);
    }

    [Benchmark]
    public IList<decimal> DeserializeDecimalList()
    {
        return JsonConvert.DeserializeObject<IList<decimal>>(FloatArrayJson);
    }
}