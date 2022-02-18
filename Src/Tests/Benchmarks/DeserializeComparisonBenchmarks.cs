﻿#region License
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

public class DeserializeComparisonBenchmarks
{
    static readonly byte[] BinaryFormatterData = BenchmarkConstants.BinaryFormatterHex.HexToBytes();

    [Benchmark]
    public TestClass DataContractSerializer()
    {
        return DeserializeDataContract<TestClass>(BenchmarkConstants.XmlText);
    }

    T DeserializeDataContract<T>(string xml)
    {
        var serializer = new DataContractSerializer(typeof(T));
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        return (T)serializer.ReadObject(ms);
    }

    T DeserializeDataContractJson<T>(string json)
    {
        var dataContractSerializer = new DataContractJsonSerializer(typeof(T));
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

        return (T)dataContractSerializer.ReadObject(ms);
    }

#if (!NET5_0_OR_GREATER)
        [Benchmark]
        public TestClass BinaryFormatter()
        {
            return DeserializeBinaryFormatter<TestClass>(BinaryFormatterData);
        }

        T DeserializeBinaryFormatter<T>(byte[] bytes)
        {
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(new MemoryStream(bytes));
        }

        [Benchmark]
        public TestClass JavaScriptSerializer()
        {
            return DeserializeWebExtensions<TestClass>(BenchmarkConstants.JsonText);
        }

        public T DeserializeWebExtensions<T>(string json)
        {
            var ser = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

            return ser.Deserialize<T>(json);
        }
#endif

    [Benchmark]
    public TestClass DataContractJsonSerializer()
    {
        return DeserializeDataContractJson<TestClass>(BenchmarkConstants.JsonText);
    }

    [Benchmark]
    public TestClass JsonNet()
    {
        return JsonConvert.DeserializeObject<TestClass>(BenchmarkConstants.JsonText);
    }

    [Benchmark]
    public TestClass JsonNetIso()
    {
        return JsonConvert.DeserializeObject<TestClass>(BenchmarkConstants.JsonIsoText);
    }

    [Benchmark]
    public TestClass JsonNetManual()
    {
        return DeserializeJsonNetManual(BenchmarkConstants.JsonText);
    }

    #region DeserializeJsonNetManual
    TestClass DeserializeJsonNetManual(string json)
    {
        var c = new TestClass();

        var reader = new JsonTextReader(new StringReader(json));
        reader.Read();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = (string)reader.Value;
                switch (propertyName)
                {
                    case "strings":
                        reader.Read();
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            c.strings.Add((string)reader.Value);
                        }
                        break;
                    case "dictionary":
                        reader.Read();
                        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                        {
                            var key = (string)reader.Value;
                            c.dictionary.Add(key, reader.ReadAsInt32().GetValueOrDefault());
                        }
                        break;
                    case "Name":
                        c.Name = reader.ReadAsString();
                        break;
                    case "Now":
                        c.Now = reader.ReadAsDateTime().GetValueOrDefault();
                        break;
                    case "BigNumber":
                        c.BigNumber = reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "Address1":
                        reader.Read();
                        c.Address1 = CreateAddress(reader);
                        break;
                    case "Addresses":
                        reader.Read();
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            var address = CreateAddress(reader);
                            c.Addresses.Add(address);
                        }
                        break;
                }
            }
            else
            {
                break;
            }
        }

        return c;
    }

    static Address CreateAddress(JsonTextReader reader)
    {
        var a = new Address();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                switch ((string)reader.Value)
                {
                    case "Street":
                        a.Street = reader.ReadAsString();
                        break;
                    case "Phone":
                        a.Phone = reader.ReadAsString();
                        break;
                    case "Entered":
                        a.Entered = reader.ReadAsDateTime().GetValueOrDefault();
                        break;
                }
            }
            else
            {
                break;
            }
        }
        return a;
    }
    #endregion

    [Benchmark]
    public Task<TestClass> JsonNetManualAsync()
    {
        return DeserializeJsonNetManualAsync(BenchmarkConstants.JsonText);
    }

    [Benchmark]
    public Task<TestClass> JsonNetManualIndentedAsync()
    {
        return DeserializeJsonNetManualAsync(BenchmarkConstants.JsonIndentedText);
    }

    async Task<TestClass> DeserializeJsonNetManualAsync(string json)
    {
        var c = new TestClass();

        var reader = new JsonTextReader(new StringReader(json));
        await reader.ReadAsync();
        while (await reader.ReadAsync())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = (string)reader.Value;
                switch (propertyName)
                {
                    case "strings":
                        await reader.ReadAsync();
                        while (await reader.ReadAsync() && reader.TokenType != JsonToken.EndArray)
                        {
                            c.strings.Add((string)reader.Value);
                        }
                        break;
                    case "dictionary":
                        await reader.ReadAsync();
                        while (await reader.ReadAsync() && reader.TokenType != JsonToken.EndObject)
                        {
                            var key = (string)reader.Value;
                            c.dictionary.Add(key, (await reader.ReadAsInt32Async()).GetValueOrDefault());
                        }
                        break;
                    case "Name":
                        c.Name = await reader.ReadAsStringAsync();
                        break;
                    case "Now":
                        c.Now = (await reader.ReadAsDateTimeAsync()).GetValueOrDefault();
                        break;
                    case "BigNumber":
                        c.BigNumber = (await reader.ReadAsDecimalAsync()).GetValueOrDefault();
                        break;
                    case "Address1":
                        await reader.ReadAsync();
                        c.Address1 = await CreateAddressAsync(reader);
                        break;
                    case "Addresses":
                        await reader.ReadAsync();
                        while (await reader.ReadAsync() && reader.TokenType != JsonToken.EndArray)
                        {
                            var address = await CreateAddressAsync(reader);
                            c.Addresses.Add(address);
                        }
                        break;
                }
            }
            else
            {
                break;
            }
        }

        return c;
    }

    static async Task<Address> CreateAddressAsync(JsonTextReader reader)
    {
        var a = new Address();
        while (await reader.ReadAsync())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                switch ((string)reader.Value)
                {
                    case "Street":
                        a.Street = await reader.ReadAsStringAsync();
                        break;
                    case "Phone":
                        a.Phone = await reader.ReadAsStringAsync();
                        break;
                    case "Entered":
                        a.Entered = (await reader.ReadAsDateTimeAsync()).GetValueOrDefault();
                        break;
                }
            }
            else
            {
                break;
            }
        }
        return a;
    }
}