// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation;

public class ReadingAndWritingJsonTests : TestFixtureBase
{
    [Fact]
    public void ReadingAndWritingJsonText()
    {
        #region ReadingAndWritingJsonText

        var stringWriter = new StringWriter();

        using var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.Formatting = Formatting.Indented;

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("CPU");
        jsonWriter.WriteValue("Intel");
        jsonWriter.WritePropertyName("PSU");
        jsonWriter.WriteValue("500W");
        jsonWriter.WritePropertyName("Drives");
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue("DVD read/writer");
        jsonWriter.WriteComment("(broken)");
        jsonWriter.WriteValue("500 gigabyte hard drive");
        jsonWriter.WriteValue("200 gigabyte hard drive");
        jsonWriter.WriteEnd();
        jsonWriter.WriteEndObject();

        // {
        //   "CPU": "Intel",
        //   "PSU": "500W",
        //   "Drives": [
        //     "DVD read/writer"
        //     /*(broken)*/,
        //     "500 gigabyte hard drive",
        //     "200 gigabyte hard drive"
        //   ]
        // }

        #endregion
    }

    [Fact]
    public void ReadingJsonText()
    {
        #region ReadingJsonText

        var json = """
            {
               'CPU': 'Intel',
               'PSU': '500W',
               'Drives': [
                 'DVD read/writer'
                 /*(broken)*/,
                 '500 gigabyte hard drive',
                 '200 gigabyte hard drive'
               ]
            }
            """;

        var reader = new JsonTextReader(new StringReader(json));
        while (reader.Read())
        {
            if (reader.Value != null)
            {
                Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
            }
            else
            {
                Console.WriteLine("Token: {0}", reader.TokenType);
            }
        }

        // Token: StartObject
        // Token: PropertyName, Value: CPU
        // Token: String, Value: Intel
        // Token: PropertyName, Value: PSU
        // Token: String, Value: 500W
        // Token: PropertyName, Value: Drives
        // Token: StartArray
        // Token: String, Value: DVD read/writer
        // Token: Comment, Value: (broken)
        // Token: String, Value: 500 gigabyte hard drive
        // Token: String, Value: 200 gigabyte hard drive
        // Token: EndArray
        // Token: EndObject

        #endregion
    }

    [Fact]
    public void ReadingAndWritingJsonLinq()
    {
        #region ReadingAndWritingJsonLinq

        var o = new JObject(
            new JProperty("Name", "John Smith"),
            new JProperty("BirthDate", new DateTime(1983, 3, 20))
        );

        var serializer = new JsonSerializer();
        var p = (Person) serializer.Deserialize(new JTokenReader(o), typeof(Person));

        Console.WriteLine(p.Name);
        // John Smith

        #endregion

        Assert.Equal("John Smith", p.Name);
    }
}