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
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Documentation;

#region JsonConverterAttribute
[JsonConverter(typeof(PersonConverter))]
public class Person
{
    public Person()
    {
        Likes = new List<string>();
    }

    public string Name { get; set; }
    public IList<string> Likes { get; private set; }
}
#endregion

#region JsonConverterContractResolver
public class ConverterContractResolver : DefaultContractResolver
{
    public new static readonly ConverterContractResolver Instance = new();

    protected override JsonContract CreateContract(Type objectType)
    {
        var contract = base.CreateContract(objectType);

        // this will only be called once and then cached
        if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset))
        {
            contract.Converter = new JavaScriptDateTimeConverter();
        }

        return contract;
    }
}
#endregion

public class PersonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var o = (JObject)JToken.ReadFrom(reader);

        var p = new Person
        {
            Name = (string)o["Name"]
        };

        return p;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Person);
    }
}

[TestFixture]
public class PerformanceTests : TestFixtureBase
{
    static class AppSettings
    {
        public static readonly IContractResolver SnakeCaseContractResolver = new DefaultContractResolver();
    }

    [Fact]
    public void ReuseContractResolverTest()
    {
        var person = new Person();

        #region ReuseContractResolver
        // BAD - a new contract resolver is created each time, forcing slow reflection to be used
        var json1 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        });

        // GOOD - reuse the contract resolver from a shared location
        var json2 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = AppSettings.SnakeCaseContractResolver
        });

        // GOOD - an internal contract resolver is used
        var json3 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        });
        #endregion
    }

    [Fact]
    public void ConverterContractResolverTest()
    {
        var json = JsonConvert.SerializeObject(new DateTime(2000, 10, 10, 10, 10, 10, DateTimeKind.Utc), new JsonSerializerSettings
        {
            ContractResolver = ConverterContractResolver.Instance
        });

        Console.WriteLine(json);
    }

    public class HttpClient
    {
        public Task<Stream> GetStreamAsync(string url)
        {
            return Task.FromResult<Stream>(new MemoryStream());
        }

        public Task<string> GetStringAsync(string url)
        {
            return Task.FromResult("{}");
        }
    }

    [Fact]
    public void DeserializeString()
    {
        #region DeserializeString
        var client = new HttpClient();

        // read the json into a string
        // string could potentially be very large and cause memory problems
        var json = client.GetStringAsync("http://www.test.com/large.json").Result;

        var p = JsonConvert.DeserializeObject<Person>(json);
        #endregion
    }

    [Fact]
    public void DeserializeStream()
    {
        #region DeserializeStream
        var client = new HttpClient();

        using (var s = client.GetStreamAsync("http://www.test.com/large.json").Result)
        using (var sr = new StreamReader(s))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            var serializer = new JsonSerializer();

            // read the json from a stream
            // json size doesn't matter because only a small piece is read at a time from the HTTP request
            var p = serializer.Deserialize<Person>(reader);
        }
        #endregion
    }
}

public static class PersonWriter
{
    #region ReaderWriter
    public static string ToJson(this Person p)
    {
        var sw = new StringWriter();
        var writer = new JsonTextWriter(sw);

        // {
        writer.WriteStartObject();

        // "name" : "Jerry"
        writer.WritePropertyName("name");
        writer.WriteValue(p.Name);

        // "likes": ["Comedy", "Superman"]
        writer.WritePropertyName("likes");
        writer.WriteStartArray();
        foreach (var like in p.Likes)
        {
            writer.WriteValue(like);
        }
        writer.WriteEndArray();

        // }
        writer.WriteEndObject();

        return sw.ToString();
    }
    #endregion

    public static Person ToPerson(this string s)
    {
        var sr = new StringReader(s);
        var reader = new JsonTextReader(sr);

        var p = new Person();

        // {
        reader.Read();
        // "name"
        reader.Read();
        // "Jerry"
        p.Name = reader.ReadAsString();
        // "likes"
        reader.Read();
        // [
        reader.Read();
        // "Comedy", "Superman", ]
        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
        {
            p.Likes.Add((string)reader.Value);
        }
        // }
        reader.Read();

        return p;
    }
}