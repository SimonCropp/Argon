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

#if (!NET5_0_OR_GREATER)
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Script.Serialization;
#endif
using System.Runtime.Serialization.Json;
using BenchmarkDotNet.Attributes;
using Argon.Tests.TestObjects;

namespace Argon.Tests.Benchmarks;

public class SerializeComparisonBenchmarks
{
    static readonly TestClass TestClass = CreateSerializationObject();

    static TestClass CreateSerializationObject()
    {
        var test = new TestClass
        {
            dictionary = new Dictionary<string, int> { { "Val & asd1", 1 }, { "Val2 & asd1", 3 }, { "Val3 & asd1", 4 } },
            Address1 =
            {
                Street = "fff Street",
                Entered = DateTime.Now.AddDays(20)
            },
            BigNumber = 34123123123.121M,
            Now = DateTime.Now.AddHours(1),
            strings = new List<string> { null, "Markus egger ]><[, (2nd)", null }
        };

        var address = new Address
        {
            Entered = DateTime.Now.AddDays(-1),
            Street = "\u001farray\u003caddress"
        };

        test.Addresses.Add(address);

        address = new Address
        {
            Entered = DateTime.Now.AddDays(-2),
            Street = "array 2 address"
        };
        test.Addresses.Add(address);
        return test;
    }

    [Benchmark]
    public string DataContractSerializer()
    {
        return SerializeDataContract(TestClass);
    }

    static string SerializeDataContract(object value)
    {
        var dataContractSerializer = new DataContractSerializer(value.GetType());

        var ms = new MemoryStream();
        dataContractSerializer.WriteObject(ms, value);

        ms.Seek(0, SeekOrigin.Begin);

        using var sr = new StreamReader(ms);
        return sr.ReadToEnd();
    }
    static readonly byte[] Buffer = new byte[4096];

#if (!NET5_0_OR_GREATER)
    [Benchmark]
    public byte[] BinaryFormatter()
    {
        return SerializeBinaryFormatter(TestClass);
    }

    static byte[] SerializeBinaryFormatter(object value)
    {
        var ms = new MemoryStream(Buffer);
        var formatter = new BinaryFormatter();
        formatter.Serialize(ms, value);

        return ms.ToArray();
    }

    [Benchmark]
    public string JavaScriptSerializer()
    {
        return SerializeWebExtensions(TestClass);
    }

    static string SerializeWebExtensions(object value)
    {
        var ser = new JavaScriptSerializer();

        return ser.Serialize(value);
    }
#endif

    [Benchmark]
    public string DataContractJsonSerializer()
    {
        return SerializeDataContractJson(TestClass);
    }

    public string SerializeDataContractJson(object value)
    {
        var dataContractSerializer = new DataContractJsonSerializer(value.GetType());

        var ms = new MemoryStream();
        dataContractSerializer.WriteObject(ms, value);

        ms.Seek(0, SeekOrigin.Begin);

        using var sr = new StreamReader(ms);
        return sr.ReadToEnd();
    }

    [Benchmark]
    public string JsonNet()
    {
        return JsonConvert.SerializeObject(TestClass);
    }

    [Benchmark]
    public string JsonNetLinq()
    {
        return SerializeJsonNetLinq(TestClass);
    }

    #region SerializeJsonNetManual

    static string SerializeJsonNetLinq(TestClass c)
    {
        var o = new JObject(
            new JProperty("strings", new JArray(c.strings)),
            new JProperty("dictionary", new JObject(c.dictionary.Select(d => new JProperty(d.Key, d.Value)))),
            new JProperty("Name", c.Name),
            new JProperty("Now", c.Now),
            new JProperty("BigNumber", c.BigNumber),
            new JProperty("Address1", new JObject(
                new JProperty("Street", c.Address1.Street),
                new JProperty("Phone", c.Address1.Phone),
                new JProperty("Entered", c.Address1.Entered))),
            new JProperty("Addresses", new JArray(c.Addresses.Select(a =>
                new JObject(
                    new JProperty("Street", a.Street),
                    new JProperty("Phone", a.Phone),
                    new JProperty("Entered", a.Entered)))))
        );

        return o.ToString(Formatting.None);
    }
    #endregion

    [Benchmark]
    public string JsonNetManual()
    {
        return SerializeJsonNetManual(TestClass);
    }

    #region SerializeJsonNetManual
    static string SerializeJsonNetManual(TestClass c)
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("strings");
        jsonWriter.WriteStartArray();
        foreach (var s in c.strings)
        {
            jsonWriter.WriteValue(s);
        }
        jsonWriter.WriteEndArray();
        jsonWriter.WritePropertyName("dictionary");
        jsonWriter.WriteStartObject();
        foreach (var keyValuePair in c.dictionary)
        {
            jsonWriter.WritePropertyName(keyValuePair.Key);
            jsonWriter.WriteValue(keyValuePair.Value);
        }
        jsonWriter.WriteEndObject();
        jsonWriter.WritePropertyName("Name");
        jsonWriter.WriteValue(c.Name);
        jsonWriter.WritePropertyName("Now");
        jsonWriter.WriteValue(c.Now);
        jsonWriter.WritePropertyName("BigNumber");
        jsonWriter.WriteValue(c.BigNumber);
        jsonWriter.WritePropertyName("Address1");
        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("Street");
        jsonWriter.WriteValue(c.BigNumber);
        jsonWriter.WritePropertyName("Street");
        jsonWriter.WriteValue(c.BigNumber);
        jsonWriter.WritePropertyName("Street");
        jsonWriter.WriteValue(c.BigNumber);
        jsonWriter.WriteEndObject();
        jsonWriter.WritePropertyName("Addresses");
        jsonWriter.WriteStartArray();
        foreach (var address in c.Addresses)
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("Street");
            jsonWriter.WriteValue(address.Street);
            jsonWriter.WritePropertyName("Phone");
            jsonWriter.WriteValue(address.Phone);
            jsonWriter.WritePropertyName("Entered");
            jsonWriter.WriteValue(address.Entered);
            jsonWriter.WriteEndObject();
        }
        jsonWriter.WriteEndArray();
        jsonWriter.WriteEndObject();

        jsonWriter.Flush();
        return stringWriter.ToString();
    }
    #endregion

    [Benchmark]
    public Task<string> JsonNetManualAsync()
    {
        return SerializeJsonNetManualAsync(TestClass, Formatting.None);
    }

    [Benchmark]
    public Task<string> JsonNetManualIndentedAsync()
    {
        return SerializeJsonNetManualAsync(TestClass, Formatting.Indented);
    }

    static async Task<string> SerializeJsonNetManualAsync(TestClass c, Formatting formatting)
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.Formatting = formatting;

        await jsonWriter.WriteStartObjectAsync();
        await jsonWriter.WritePropertyNameAsync("strings");
        await jsonWriter.WriteStartArrayAsync();
        foreach (var s in c.strings)
        {
            await jsonWriter.WriteValueAsync(s);
        }
        await jsonWriter.WriteEndArrayAsync();
        await jsonWriter.WritePropertyNameAsync("dictionary");
        await jsonWriter.WriteStartObjectAsync();
        foreach (var keyValuePair in c.dictionary)
        {
            await jsonWriter.WritePropertyNameAsync(keyValuePair.Key);
            await jsonWriter.WriteValueAsync(keyValuePair.Value);
        }
        await jsonWriter.WriteEndObjectAsync();
        await jsonWriter.WritePropertyNameAsync("Name");
        await jsonWriter.WriteValueAsync(c.Name);
        await jsonWriter.WritePropertyNameAsync("Now");
        await jsonWriter.WriteValueAsync(c.Now);
        await jsonWriter.WritePropertyNameAsync("BigNumber");
        await jsonWriter.WriteValueAsync(c.BigNumber);
        await jsonWriter.WritePropertyNameAsync("Address1");
        await jsonWriter.WriteStartObjectAsync();
        await jsonWriter.WritePropertyNameAsync("Street");
        await jsonWriter.WriteValueAsync(c.BigNumber);
        await jsonWriter.WritePropertyNameAsync("Street");
        await jsonWriter.WriteValueAsync(c.BigNumber);
        await jsonWriter.WritePropertyNameAsync("Street");
        await jsonWriter.WriteValueAsync(c.BigNumber);
        await jsonWriter.WriteEndObjectAsync();
        await jsonWriter.WritePropertyNameAsync("Addresses");
        await jsonWriter.WriteStartArrayAsync();
        foreach (var address in c.Addresses)
        {
            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WritePropertyNameAsync("Street");
            await jsonWriter.WriteValueAsync(address.Street);
            await jsonWriter.WritePropertyNameAsync("Phone");
            await jsonWriter.WriteValueAsync(address.Phone);
            await jsonWriter.WritePropertyNameAsync("Entered");
            await jsonWriter.WriteValueAsync(address.Entered);
            await jsonWriter.WriteEndObjectAsync();
        }
        await jsonWriter.WriteEndArrayAsync();
        await jsonWriter.WriteEndObjectAsync();

        await jsonWriter.FlushAsync();
        return stringWriter.ToString();
    }
}