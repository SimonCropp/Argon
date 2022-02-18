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

using BenchmarkDotNet.Attributes;

namespace Argon.Tests.Benchmarks;

public class JTokenBenchmarks
{
    static readonly JObject JObjectSample = JObject.Parse(BenchmarkConstants.JsonText);
    static readonly string JsonTextSample;
    static readonly string NestedJsonText;

    static JTokenBenchmarks()
    {
        var o = new JObject();
        for (var i = 0; i < 50; i++)
        {
            o[i.ToString()] = i;
        }
        JsonTextSample = o.ToString();

        NestedJsonText = new string('[', 100000) + "1" + new string(']', 100000);
    }

    [Benchmark]
    public void TokenWriteTo()
    {
        var sw = new StringWriter();
        JObjectSample.WriteTo(new JsonTextWriter(sw));
    }

    [Benchmark]
    public Task TokenWriteToAsync()
    {
        var sw = new StringWriter();
        return JObjectSample.WriteToAsync(new JsonTextWriter(sw));
    }

    [Benchmark]
    public JObject JObjectParse()
    {
        return JObject.Parse(JsonTextSample);
    }

    [Benchmark]
    public JArray JArrayNestedParse()
    {
        return JArray.Parse(NestedJsonText);
    }

    [Benchmark]
    public JArray JArrayNestedBuild()
    {
        var current = new JArray();
        var root = current;
        for (var j = 0; j < 100000; j++)
        {
            var temp = new JArray();
            current.Add(temp);
            current = temp;
        }
        current.Add(1);

        return root;
    }
}