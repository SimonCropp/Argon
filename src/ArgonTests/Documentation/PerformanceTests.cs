// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation;

#region JsonConverterAttribute

[JsonConverter(typeof(PersonConverter))]
public class Person
{
    public Person() =>
        Likes = new List<string>();

    public string Name { get; set; }
    public IList<string> Likes { get; }
}

#endregion

public class PersonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var o = (JObject) JToken.ReadFrom(reader);

        var p = new Person
        {
            Name = (string) o["Name"]
        };

        return p;
    }

    public override bool CanConvert(Type type) =>
        type == typeof(Person);
}

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
        JsonConvert.SerializeObject(person, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        });

        // GOOD - reuse the contract resolver from a shared location
        JsonConvert.SerializeObject(person, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = AppSettings.SnakeCaseContractResolver
        });

        // GOOD - an internal contract resolver is used
        JsonConvert.SerializeObject(person, new JsonSerializerSettings
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
            ContractResolver = DefaultContractResolver.Instance
        });

        Console.WriteLine(json);
    }

    public class HttpClient
    {
        public Task<Stream> GetStreamAsync(string url) =>
            Task.FromResult<Stream>(new MemoryStream());

        public Task<string> GetStringAsync(string url) =>
            Task.FromResult("{}");
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

    public void DeserializeStream(Stream stream)
    {
        #region DeserializeStream

        using var streamReader = new StreamReader(stream);
        using var reader = new JsonTextReader(streamReader);
        var serializer = new JsonSerializer();

        // read the json from a stream
        // json size doesn't matter because only a small piece is read at a time from the stream
        var p = serializer.Deserialize<Person>(reader);

        #endregion
    }
}

public static class PersonWriter
{
    #region ReaderWriter

    public static string ToJson(this Person p)
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);

        // {
        jsonWriter.WriteStartObject();

        // "name" : "Jerry"
        jsonWriter.WritePropertyName("name");
        jsonWriter.WriteValue(p.Name);

        // "likes": ["Comedy", "Superman"]
        jsonWriter.WritePropertyName("likes");
        jsonWriter.WriteStartArray();
        foreach (var like in p.Likes)
        {
            jsonWriter.WriteValue(like);
        }

        jsonWriter.WriteEndArray();

        // }
        jsonWriter.WriteEndObject();

        return stringWriter.ToString();
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
            p.Likes.Add((string) reader.Value);
        }

        // }
        reader.Read();

        return p;
    }
}