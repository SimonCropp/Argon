// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using BenchmarkDotNet.Attributes;

public class JValueConvertBenchmarks
{
    static readonly JValue StringJValue = new("String!");

    [Benchmark]
    public string JTokenToObjectFast()
    {
        return (string) StringJValue.ToObject(typeof(string));
    }

    [Benchmark]
    public string JTokenToObjectWithSerializer()
    {
        return (string) StringJValue.ToObject(typeof(string), new());
    }

    [Benchmark]
    public string JTokenToObjectConvert()
    {
        return StringJValue.Value<string>();
    }

    [Benchmark]
    public string JTokenToObjectCast()
    {
        return (string) StringJValue;
    }
}