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

using Xunit;

namespace Argon.Tests.Documentation;

public class ReadingAndWritingJsonTests : TestFixtureBase
{
    [Fact]
    public void ReadingAndWritingJsonText()
    {
        #region ReadingAndWritingJsonText
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

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
        var json = @"{
               'CPU': 'Intel',
               'PSU': '500W',
               'Drives': [
                 'DVD read/writer'
                 /*(broken)*/,
                 '500 gigabyte hard drive',
                 '200 gigabyte hard drive'
               ]
            }";

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
        var p = (Person)serializer.Deserialize(new JTokenReader(o), typeof(Person));

        Console.WriteLine(p.Name);
        // John Smith
        #endregion

        Assert.Equal("John Smith", p.Name);
    }
}