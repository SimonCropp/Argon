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
using Assert = Argon.Tests.XUnitAssert;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;

namespace Argon.Tests.Linq;

public class LinqToJsonTest : TestFixtureBase
{
    [Fact]
    public void EscapedQuotePath()
    {
        var v = new JValue(1);
        var o = new JObject
        {
            ["We're offline!"] = v
        };

        Xunit.Assert.Equal(@"['We\'re offline!']", v.Path);
    }

    public class DemoClass
    {
        public decimal maxValue;
    }

    [Fact]
    public void ToObjectDecimal()
    {
        var jArray = JArray.Parse("[{ maxValue:10000000000000000000 }]");
        var list = jArray.ToObject<List<DemoClass>>();

        Xunit.Assert.Equal(10000000000000000000m, list[0].maxValue);
    }

    [Fact]
    public void ToObjectFromGuidToString()
    {
        var token = new JValue(new Guid("91274484-3b20-48b4-9d18-7d936b2cb88f"));
        var value = token.ToObject<string>();
        Xunit.Assert.Equal("91274484-3b20-48b4-9d18-7d936b2cb88f", value);
    }

    [Fact]
    public void ToObjectFromIntegerToString()
    {
        var token = new JValue(1234);
        var value = token.ToObject<string>();
        Xunit.Assert.Equal("1234", value);
    }

    [Fact]
    public void ToObjectFromStringToInteger()
    {
        var token = new JValue("1234");
        var value = token.ToObject<int>();
        Xunit.Assert.Equal(1234, value);
    }

    [Fact]
    public void FromObjectGuid()
    {
        var token1 = new JValue(Guid.NewGuid());
        var token2 = JToken.FromObject(token1);
        Xunit.Assert.True(JToken.DeepEquals(token1, token2));
        Xunit.Assert.Equal(token1.Type, token2.Type);
    }

    [Fact]
    public void FromObjectTimeSpan()
    {
        var token1 = new JValue(TimeSpan.FromDays(1));
        var token2 = JToken.FromObject(token1);
        Xunit.Assert.True(JToken.DeepEquals(token1, token2));
        Xunit.Assert.Equal(token1.Type, token2.Type);
    }

    [Fact]
    public void FromObjectUri()
    {
        var token1 = new JValue(new Uri("http://www.newtonsoft.com"));
        var token2 = JToken.FromObject(token1);
        Xunit.Assert.True(JToken.DeepEquals(token1, token2));
        Xunit.Assert.Equal(token1.Type, token2.Type);
    }

    [Fact]
    public void ToObject_Guid()
    {
        var anon = new JObject
        {
            ["id"] = Guid.NewGuid()
        };
        Xunit.Assert.Equal(JTokenType.Guid, anon["id"].Type);

        var dict = anon.ToObject<Dictionary<string, JToken>>();
        Xunit.Assert.Equal(JTokenType.Guid, dict["id"].Type);
    }

    public class TestClass_ULong
    {
        public ulong Value { get; set; }
    }

    [Fact]
    public void FromObject_ULongMaxValue()
    {
        var instance = new TestClass_ULong { Value = ulong.MaxValue };
        var output = JObject.FromObject(instance);

        StringAssert.AreEqual(@"{
  ""Value"": 18446744073709551615
}", output.ToString());
    }

    public class TestClass_Byte
    {
        public byte Value { get; set; }
    }

    [Fact]
    public void FromObject_ByteMaxValue()
    {
        var instance = new TestClass_Byte { Value = byte.MaxValue };
        var output = JObject.FromObject(instance);

        StringAssert.AreEqual(@"{
  ""Value"": 255
}", output.ToString());
    }

    [Fact]
    public void ToObject_Base64AndGuid()
    {
        var o = JObject.Parse("{'responseArray':'AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA'}");
        var data = o["responseArray"].ToObject<byte[]>();
        var expected = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA");

        Xunit.Assert.Equal(expected, data);

        o = JObject.Parse("{'responseArray':'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA'}");
        data = o["responseArray"].ToObject<byte[]>();
        expected = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA").ToByteArray();

        Xunit.Assert.Equal(expected, data);
    }

    [Fact]
    public void IncompleteContainers()
    {
        ExceptionAssert.Throws<JsonReaderException>(
            () => JArray.Parse("[1,"),
            "Unexpected end of content while loading JArray. Path '[0]', line 1, position 3.");

        ExceptionAssert.Throws<JsonReaderException>(
            () => JArray.Parse("[1"),
            "Unexpected end of content while loading JArray. Path '[0]', line 1, position 2.");

        ExceptionAssert.Throws<JsonReaderException>(
            () => JObject.Parse("{'key':1,"),
            "Unexpected end of content while loading JObject. Path 'key', line 1, position 9.");

        ExceptionAssert.Throws<JsonReaderException>(
            () => JObject.Parse("{'key':1"),
            "Unexpected end of content while loading JObject. Path 'key', line 1, position 8.");
    }

    [Fact]
    public void EmptyJEnumerableCount()
    {
        var tokens = new JEnumerable<JToken>();

        Xunit.Assert.Equal(0, tokens.Count());
    }

    [Fact]
    public void EmptyJEnumerableAsEnumerable()
    {
        IEnumerable tokens = new JEnumerable<JToken>();

        Xunit.Assert.Equal(0, tokens.Cast<JToken>().Count());
    }

    [Fact]
    public void EmptyJEnumerableEquals()
    {
        var tokens1 = new JEnumerable<JToken>();
        var tokens2 = new JEnumerable<JToken>();

        Xunit.Assert.True(tokens1.Equals(tokens2));

        object o1 = new JEnumerable<JToken>();
        object o2 = new JEnumerable<JToken>();

        Xunit.Assert.True(o1.Equals(o2));
    }

    [Fact]
    public void EmptyJEnumerableGetHashCode()
    {
        var tokens = new JEnumerable<JToken>();

        Xunit.Assert.Equal(0, tokens.GetHashCode());
    }

    [Fact]
    public void CommentsAndReadFrom()
    {
        var textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var a = (JArray)JToken.ReadFrom(jsonReader, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Load
        });

        Xunit.Assert.Equal(4, a.Count);
        Xunit.Assert.Equal(JTokenType.Comment, a[0].Type);
        Xunit.Assert.Equal(" hi", ((JValue)a[0]).Value);
    }

    [Fact]
    public void CommentsAndReadFrom_IgnoreComments()
    {
        var textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var a = (JArray)JToken.ReadFrom(jsonReader);

        Xunit.Assert.Equal(3, a.Count);
        Xunit.Assert.Equal(JTokenType.Integer, a[0].Type);
        Xunit.Assert.Equal(1L, ((JValue)a[0]).Value);
    }

    [Fact]
    public void StartingCommentAndReadFrom()
    {
        var textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var v = (JValue)JToken.ReadFrom(jsonReader, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Load
        });

        Xunit.Assert.Equal(JTokenType.Comment, v.Type);

        IJsonLineInfo lineInfo = v;
        XUnitAssert.True(lineInfo.HasLineInfo());
        Xunit.Assert.Equal(2, lineInfo.LineNumber);
        Xunit.Assert.Equal(5, lineInfo.LinePosition);
    }

    [Fact]
    public void StartingCommentAndReadFrom_IgnoreComments()
    {
        var textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var a = (JArray)JToken.ReadFrom(jsonReader, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore
        });

        Xunit.Assert.Equal(JTokenType.Array, a.Type);

        IJsonLineInfo lineInfo = a;
        XUnitAssert.True(lineInfo.HasLineInfo());
        Xunit.Assert.Equal(3, lineInfo.LineNumber);
        Xunit.Assert.Equal(1, lineInfo.LinePosition);
    }

    [Fact]
    public void StartingUndefinedAndReadFrom()
    {
        var textReader = new StringReader(@"
undefined
[
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var v = (JValue)JToken.ReadFrom(jsonReader);

        Xunit.Assert.Equal(JTokenType.Undefined, v.Type);

        IJsonLineInfo lineInfo = v;
        XUnitAssert.True(lineInfo.HasLineInfo());
        Xunit.Assert.Equal(2, lineInfo.LineNumber);
        Xunit.Assert.Equal(9, lineInfo.LinePosition);
    }

    [Fact]
    public void StartingEndArrayAndReadFrom()
    {
        var textReader = new StringReader(@"[]");

        var jsonReader = new JsonTextReader(textReader);
        jsonReader.Read();
        jsonReader.Read();

        ExceptionAssert.Throws<JsonReaderException>(() => JToken.ReadFrom(jsonReader), @"Error reading JToken from JsonReader. Unexpected token: EndArray. Path '', line 1, position 2.");
    }

    [Fact]
    public void JPropertyPath()
    {
        var o = new JObject
        {
            {
                "person",
                new JObject
                {
                    { "$id", 1 }
                }
            }
        };

        var idProperty = o["person"]["$id"].Parent;
        Xunit.Assert.Equal("person.$id", idProperty.Path);
    }

    [Fact]
    public void EscapedPath()
    {
        var json = @"{
  ""frameworks"": {
    ""NET5_0_OR_GREATER"": {
      ""dependencies"": {
        ""System.Xml.ReaderWriter"": {
          ""source"": ""NuGet""
        }
      }
    }
  }
}";

        var o = JObject.Parse(json);

        var v1 = o["frameworks"]["NET5_0_OR_GREATER"]["dependencies"]["System.Xml.ReaderWriter"]["source"];

        Xunit.Assert.Equal("frameworks.NET5_0_OR_GREATER.dependencies['System.Xml.ReaderWriter'].source", v1.Path);

        var v2 = o.SelectToken(v1.Path);

        Xunit.Assert.Equal(v1, v2);
    }

    [Fact]
    public void EscapedPathTests()
    {
        EscapedPathAssert("this has spaces", "['this has spaces']");
        EscapedPathAssert("(RoundBraces)", "['(RoundBraces)']");
        EscapedPathAssert("[SquareBraces]", "['[SquareBraces]']");
        EscapedPathAssert("this.has.dots", "['this.has.dots']");
    }

    void EscapedPathAssert(string propertyName, string expectedPath)
    {
        var v1 = int.MaxValue;
        var value = new JValue(v1);

        var o = new JObject(new JProperty(propertyName, value));

        Xunit.Assert.Equal(expectedPath, value.Path);

        var selectedValue = (JValue)o.SelectToken(value.Path);

        Xunit.Assert.Equal(value, selectedValue);
    }

    [Fact]
    public void ForEach()
    {
        var items = new JArray(new JObject(new JProperty("name", "value!")));

        foreach (JObject friend in items)
        {
            StringAssert.AreEqual(@"{
  ""name"": ""value!""
}", friend.ToString());
        }
    }

    [Fact]
    public void DoubleValue()
    {
        var j = JArray.Parse("[-1E+4,100.0e-2]");

        var value = (double)j[0];
        Xunit.Assert.Equal(-10000d, value);

        value = (double)j[1];
        Xunit.Assert.Equal(1d, value);
    }

    [Fact]
    public void Manual()
    {
        var array = new JArray();
        var text = new JValue("Manual text");
        var date = new JValue(new DateTime(2000, 5, 23));

        array.Add(text);
        array.Add(date);

        var json = array.ToString();
        // [
        //   "Manual text",
        //   "\/Date(958996800000+1200)\/"
        // ]
    }

    [Fact]
    public void LinqToJsonDeserialize()
    {
        var o = new JObject(
            new JProperty("Name", "John Smith"),
            new JProperty("BirthDate", new DateTime(1983, 3, 20))
        );

        var serializer = new JsonSerializer();
        var p = (Person)serializer.Deserialize(new JTokenReader(o), typeof(Person));

        Xunit.Assert.Equal("John Smith", p.Name);
    }

    [Fact]
    public void ObjectParse()
    {
        var json = @"{
        CPU: 'Intel',
        Drives: [
          'DVD read/writer',
          ""500 gigabyte hard drive""
        ]
      }";

        var o = JObject.Parse(json);
        IList<JProperty> properties = o.Properties().ToList();

        Xunit.Assert.Equal("CPU", properties[0].Name);
        Xunit.Assert.Equal("Intel", (string)properties[0].Value);
        Xunit.Assert.Equal("Drives", properties[1].Name);

        var list = (JArray)properties[1].Value;
        Xunit.Assert.Equal(2, list.Children().Count());
        Xunit.Assert.Equal("DVD read/writer", (string)list.Children().ElementAt(0));
        Xunit.Assert.Equal("500 gigabyte hard drive", (string)list.Children().ElementAt(1));

        var parameterValues =
            (from p in o.Properties()
                where p.Value is JValue
                select ((JValue)p.Value).Value).ToList();

        Xunit.Assert.Equal(1, parameterValues.Count);
        Xunit.Assert.Equal("Intel", parameterValues[0]);
    }

    [Fact]
    public void CreateLongArray()
    {
        var json = @"[0,1,2,3,4,5,6,7,8,9]";

        var a = JArray.Parse(json);
        var list = a.Values<int>().ToList();

        var expected = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        Xunit.Assert.Equal(expected, list);
    }

    [Fact]
    public void GoogleSearchAPI()
    {
        #region GoogleJson
        var json = @"{
    results:
        [
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://www.google.com/"",
                url : ""http://www.google.com/"",
                visibleUrl : ""www.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:zhool8dxBV4J:www.google.com"",
                title : ""Google"",
                titleNoFormatting : ""Google"",
                content : ""Enables users to search the Web, Usenet, and 
images. Features include PageRank,   caching and translation of 
results, and an option to find similar pages.""
            },
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://news.google.com/"",
                url : ""http://news.google.com/"",
                visibleUrl : ""news.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:Va_XShOz_twJ:news.google.com"",
                title : ""Google News"",
                titleNoFormatting : ""Google News"",
                content : ""Aggregated headlines and a search engine of many of the world's news sources.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://groups.google.com/"",
                url : ""http://groups.google.com/"",
                visibleUrl : ""groups.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:x2uPD3hfkn0J:groups.google.com"",
                title : ""Google Groups"",
                titleNoFormatting : ""Google Groups"",
                content : ""Enables users to search and browse the Usenet 
archives which consist of over 700   million messages, and post new 
comments.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://maps.google.com/"",
                url : ""http://maps.google.com/"",
                visibleUrl : ""maps.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:dkf5u2twBXIJ:maps.google.com"",
                title : ""Google Maps"",
                titleNoFormatting : ""Google Maps"",
                content : ""Provides directions, interactive maps, and 
satellite/aerial imagery of the United   States. Can also search by 
keyword such as type of business.""
            }
        ],
        
    adResults:
        [
            {
                GsearchResultClass:""GwebSearch.ad"",
                title : ""Gartner Symposium/ITxpo"",
                content1 : ""Meet brilliant Gartner IT analysts"",
                content2 : ""20-23 May 2007- Barcelona, Spain"",
                url : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                impressionUrl : 
""http://www.google.com/uds/css/ad-indicator-on.gif?ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB"", 

                unescapedUrl : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                visibleUrl : ""www.gartner.com""
            }
        ]
}
";
        #endregion

        var o = JObject.Parse(json);

        var resultObjects = o["results"].Children<JObject>().ToList();

        Xunit.Assert.Equal(32, resultObjects.Properties().Count());

        Xunit.Assert.Equal(32, resultObjects.Cast<JToken>().Values().Count());

        Xunit.Assert.Equal(4, resultObjects.Cast<JToken>().Values("GsearchResultClass").Count());

        Xunit.Assert.Equal(5, o.PropertyValues().Cast<JArray>().Children().Count());

        var resultUrls = o["results"].Children().Values<string>("url").ToList();

        var expectedUrls = new List<string> { "http://www.google.com/", "http://news.google.com/", "http://groups.google.com/", "http://maps.google.com/" };

        Xunit.Assert.Equal(expectedUrls, resultUrls);

        var descendants = o.Descendants().ToList();
        Xunit.Assert.Equal(89, descendants.Count);
    }

    [Fact]
    public void JTokenToString()
    {
        var json = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

        var o = JObject.Parse(json);

        StringAssert.AreEqual(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}", o.ToString());

        var list = o.Value<JArray>("Drives");

        StringAssert.AreEqual(@"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", list.ToString());

        var cpuProperty = o.Property("CPU");
        Xunit.Assert.Equal(@"""CPU"": ""Intel""", cpuProperty.ToString());

        var drivesProperty = o.Property("Drives");
        StringAssert.AreEqual(@"""Drives"": [
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", drivesProperty.ToString());
    }

    [Fact]
    public void JTokenToStringTypes()
    {
        var json = @"{""Color"":2,""Establised"":new Date(1264118400000),""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":new Date(949532490000),""Price"":0},{""Name"":""Alien"",""ExpiryDate"":new Date(-62135596800000),""Price"":0}]}";

        var o = JObject.Parse(json);

        StringAssert.AreEqual(@"""Establised"": new Date(
  1264118400000
)", o.Property("Establised").ToString());
        StringAssert.AreEqual(@"new Date(
  1264118400000
)", o.Property("Establised").Value.ToString());
        Xunit.Assert.Equal(@"""Width"": 1.1", o.Property("Width").ToString());
        Xunit.Assert.Equal(@"1.1", ((JValue)o.Property("Width").Value).ToString(CultureInfo.InvariantCulture));
        Xunit.Assert.Equal(@"""Open"": false", o.Property("Open").ToString());
        Xunit.Assert.Equal(@"False", o.Property("Open").Value.ToString());

        json = @"[null,undefined]";

        var a = JArray.Parse(json);
        StringAssert.AreEqual(@"[
  null,
  undefined
]", a.ToString());
        Xunit.Assert.Equal(@"", a.Children().ElementAt(0).ToString());
        Xunit.Assert.Equal(@"", a.Children().ElementAt(1).ToString());
    }

    [Fact]
    public void CreateJTokenTree()
    {
        var o =
            new JObject(
                new JProperty("Test1", "Test1Value"),
                new JProperty("Test2", "Test2Value"),
                new JProperty("Test3", "Test3Value"),
                new JProperty("Test4", null)
            );

        Xunit.Assert.Equal(4, o.Properties().Count());

        StringAssert.AreEqual(@"{
  ""Test1"": ""Test1Value"",
  ""Test2"": ""Test2Value"",
  ""Test3"": ""Test3Value"",
  ""Test4"": null
}", o.ToString());

        var a =
            new JArray(
                o,
                new DateTime(2000, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                55,
                new JArray(
                    "1",
                    2,
                    3.0,
                    new DateTime(4, 5, 6, 7, 8, 9, DateTimeKind.Utc)
                ),
                new JConstructor(
                    "ConstructorName",
                    "param1",
                    2,
                    3.0
                )
            );

        Xunit.Assert.Equal(5, a.Count());
        StringAssert.AreEqual(@"[
  {
    ""Test1"": ""Test1Value"",
    ""Test2"": ""Test2Value"",
    ""Test3"": ""Test3Value"",
    ""Test4"": null
  },
  ""2000-10-10T00:00:00Z"",
  55,
  [
    ""1"",
    2,
    3.0,
    ""0004-05-06T07:08:09Z""
  ],
  new ConstructorName(
    ""param1"",
    2,
    3.0
  )
]", a.ToString());
    }

    class Post
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public IList<string> Categories { get; set; }
    }

    List<Post> GetPosts()
    {
        return new List<Post>
        {
            new()
            {
                Title = "LINQ to JSON beta",
                Description = "Announcing LINQ to JSON",
                Link = "http://james.newtonking.com/projects/json-net.aspx",
                Categories = new List<string> { "Json.NET", "LINQ" }
            },
            new()
            {
                Title = "Json.NET 1.3 + New license + Now on CodePlex",
                Description = "Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
                Link = "http://james.newtonking.com/projects/json-net.aspx",
                Categories = new List<string> { "Json.NET", "CodePlex" }
            }
        };
    }

    [Fact]
    public void FromObjectExample()
    {
        var p = new Post
        {
            Title = "How to use FromObject",
            Categories = new[] { "LINQ to JSON" }
        };

        // serialize Post to JSON then parse JSON – SLOW!
        //JObject o = JObject.Parse(JsonConvert.SerializeObject(p));

        // create JObject directly from the Post
        var o = JObject.FromObject(p);

        o["Title"] = o["Title"] + " - Super effective!";

        var json = o.ToString();
        // {
        //   "Title": "How to use FromObject - It's super effective!",
        //   "Categories": [
        //     "LINQ to JSON"
        //   ]
        // }

        StringAssert.AreEqual(@"{
  ""Title"": ""How to use FromObject - Super effective!"",
  ""Description"": null,
  ""Link"": null,
  ""Categories"": [
    ""LINQ to JSON""
  ]
}", json);
    }

    [Fact]
    public void QueryingExample()
    {
        var posts = JArray.Parse(@"[
              {
                'Title': 'JSON Serializer Basics',
                'Date': '2013-12-21T00:00:00',
                'Categories': []
              },
              {
                'Title': 'Querying LINQ to JSON',
                'Date': '2014-06-03T00:00:00',
                'Categories': [
                  'LINQ to JSON'
                ]
              }
            ]");

        var serializerBasics = posts
            .Single(p => (string)p["Title"] == "JSON Serializer Basics");
        // JSON Serializer Basics

        IList<JToken> since2012 = posts
            .Where(p => (DateTime)p["Date"] > new DateTime(2012, 1, 1)).ToList();
        // JSON Serializer Basics
        // Querying LINQ to JSON

        IList<JToken> linqToJson = posts
            .Where(p => p["Categories"].Any(c => (string)c == "LINQ to JSON")).ToList();
        // Querying LINQ to JSON

        Xunit.Assert.NotNull(serializerBasics);
        Xunit.Assert.Equal(2, since2012.Count);
        Xunit.Assert.Equal(1, linqToJson.Count);
    }

    [Fact]
    public void CreateJTokenTreeNested()
    {
        var posts = GetPosts();

        var rss =
            new JObject(
                new JProperty("channel",
                    new JObject(
                        new JProperty("title", "James Newton-King"),
                        new JProperty("link", "http://james.newtonking.com"),
                        new JProperty("description", "James Newton-King's blog."),
                        new JProperty("item",
                            new JArray(
                                from p in posts
                                orderby p.Title
                                select new JObject(
                                    new JProperty("title", p.Title),
                                    new JProperty("description", p.Description),
                                    new JProperty("link", p.Link),
                                    new JProperty("category",
                                        new JArray(
                                            from c in p.Categories
                                            select new JValue(c)))))))));

        StringAssert.AreEqual(@"{
  ""channel"": {
    ""title"": ""James Newton-King"",
    ""link"": ""http://james.newtonking.com"",
    ""description"": ""James Newton-King's blog."",
    ""item"": [
      {
        ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
        ""description"": ""Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""CodePlex""
        ]
      },
      {
        ""title"": ""LINQ to JSON beta"",
        ""description"": ""Announcing LINQ to JSON"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""LINQ""
        ]
      }
    ]
  }
}", rss.ToString());

        var postTitles =
            from p in rss["channel"]["item"]
            select p.Value<string>("title");

        Xunit.Assert.Equal("Json.NET 1.3 + New license + Now on CodePlex", postTitles.ElementAt(0));
        Xunit.Assert.Equal("LINQ to JSON beta", postTitles.ElementAt(1));

        var categories =
            from c in rss["channel"]["item"].Children()["category"].Values<string>()
            group c by c
            into g
            orderby g.Count() descending
            select new { Category = g.Key, Count = g.Count() };

        Xunit.Assert.Equal("Json.NET", categories.ElementAt(0).Category);
        Xunit.Assert.Equal(2, categories.ElementAt(0).Count);
        Xunit.Assert.Equal("CodePlex", categories.ElementAt(1).Category);
        Xunit.Assert.Equal(1, categories.ElementAt(1).Count);
        Xunit.Assert.Equal("LINQ", categories.ElementAt(2).Category);
        Xunit.Assert.Equal(1, categories.ElementAt(2).Count);
    }

    [Fact]
    public void BasicQuerying()
    {
        var json = @"{
                        ""channel"": {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Announcing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        }
                      }";

        var o = JObject.Parse(json);

        Xunit.Assert.Equal(null, o["purple"]);
        Xunit.Assert.Equal(null, o.Value<string>("purple"));

        Xunit.Assert.IsType(typeof(JArray), o["channel"]["item"]);

        Xunit.Assert.Equal(2, o["channel"]["item"].Children()["title"].Count());
        Xunit.Assert.Equal(0, o["channel"]["item"].Children()["monkey"].Count());

        Xunit.Assert.Equal("Json.NET 1.3 + New license + Now on CodePlex", (string)o["channel"]["item"][0]["title"]);

        Xunit.Assert.Equal(new[] { "Json.NET 1.3 + New license + Now on CodePlex", "LINQ to JSON beta" }, o["channel"]["item"].Children().Values<string>("title").ToArray());
    }

    [Fact]
    public void JObjectIntIndex()
    {
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            var o = new JObject();
            Xunit.Assert.Equal(null, o[0]);
        }, "Accessed JObject values with invalid key value: 0. Object property name expected.");
    }

    [Fact]
    public void JArrayStringIndex()
    {
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            var a = new JArray();
            Xunit.Assert.Equal(null, a["purple"]);
        }, @"Accessed JArray values with invalid key value: ""purple"". Int32 array index expected.");
    }

    [Fact]
    public void JConstructorStringIndex()
    {
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            var c = new JConstructor("ConstructorValue");
            Xunit.Assert.Equal(null, c["purple"]);
        }, @"Accessed JConstructor values with invalid key value: ""purple"". Argument position index expected.");
    }

    [Fact]
    public void ToStringJsonConverter()
    {
        var o =
            new JObject(
                new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0))),
                new JProperty("Test3", "Test3Value"),
                new JProperty("Test4", null)
            );

        var serializer = new JsonSerializer();
        serializer.Converters.Add(new JavaScriptDateTimeConverter());
        var sw = new StringWriter();
        JsonWriter writer = new JsonTextWriter(sw);
        writer.Formatting = Formatting.Indented;
        serializer.Serialize(writer, o);

        var json = sw.ToString();

        StringAssert.AreEqual(@"{
  ""Test1"": new Date(
    971586305000
  ),
  ""Test2"": new Date(
    971546045000
  ),
  ""Test3"": ""Test3Value"",
  ""Test4"": null
}", json);
    }

    [Fact]
    public void DateTimeOffset()
    {
        var testDates = new List<DateTimeOffset>
        {
            new(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
            new(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
            new(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)),
            new(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(-3.5)),
        };

        var jsonSerializer = new JsonSerializer();

        JTokenWriter jsonWriter;
        using (jsonWriter = new JTokenWriter())
        {
            jsonSerializer.Serialize(jsonWriter, testDates);
        }

        Xunit.Assert.Equal(4, jsonWriter.Token.Children().Count());
    }

    [Fact]
    public void FromObject()
    {
        var posts = GetPosts();

        var o = JObject.FromObject(new
        {
            channel = new
            {
                title = "James Newton-King",
                link = "http://james.newtonking.com",
                description = "James Newton-King's blog.",
                item =
                    from p in posts
                    orderby p.Title
                    select new
                    {
                        title = p.Title,
                        description = p.Description,
                        link = p.Link,
                        category = p.Categories
                    }
            }
        });

        StringAssert.AreEqual(@"{
  ""channel"": {
    ""title"": ""James Newton-King"",
    ""link"": ""http://james.newtonking.com"",
    ""description"": ""James Newton-King's blog."",
    ""item"": [
      {
        ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
        ""description"": ""Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""CodePlex""
        ]
      },
      {
        ""title"": ""LINQ to JSON beta"",
        ""description"": ""Announcing LINQ to JSON"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""LINQ""
        ]
      }
    ]
  }
}", o.ToString());

        Xunit.Assert.IsType(typeof(JObject), o);
        Xunit.Assert.IsType(typeof(JObject), o["channel"]);
        Xunit.Assert.Equal("James Newton-King", (string)o["channel"]["title"]);
        Xunit.Assert.Equal(2, o["channel"]["item"].Children().Count());

        var a = JArray.FromObject(new List<int> { 0, 1, 2, 3, 4 });
        Xunit.Assert.IsType(typeof(JArray), a);
        Xunit.Assert.Equal(5, a.Count());
    }

    [Fact]
    public void FromAnonDictionary()
    {
        var posts = GetPosts();

        var o = JObject.FromObject(new
        {
            channel = new Dictionary<string, object>
            {
                { "title", "James Newton-King" },
                { "link", "http://james.newtonking.com" },
                { "description", "James Newton-King's blog." },
                {
                    "item",
                    from p in posts
                    orderby p.Title
                    select new
                    {
                        title = p.Title,
                        description = p.Description,
                        link = p.Link,
                        category = p.Categories
                    }
                }
            }
        });

        StringAssert.AreEqual(@"{
  ""channel"": {
    ""title"": ""James Newton-King"",
    ""link"": ""http://james.newtonking.com"",
    ""description"": ""James Newton-King's blog."",
    ""item"": [
      {
        ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
        ""description"": ""Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""CodePlex""
        ]
      },
      {
        ""title"": ""LINQ to JSON beta"",
        ""description"": ""Announcing LINQ to JSON"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""LINQ""
        ]
      }
    ]
  }
}", o.ToString());

        Xunit.Assert.IsType(typeof(JObject), o);
        Xunit.Assert.IsType(typeof(JObject), o["channel"]);
        Xunit.Assert.Equal("James Newton-King", (string)o["channel"]["title"]);
        Xunit.Assert.Equal(2, o["channel"]["item"].Children().Count());

        var a = JArray.FromObject(new List<int> { 0, 1, 2, 3, 4 });
        Xunit.Assert.IsType(typeof(JArray), a);
        Xunit.Assert.Equal(5, a.Count());
    }

    [Fact]
    public void AsJEnumerable()
    {
        JObject o = null;

        var enumerable = o.AsJEnumerable();
        Xunit.Assert.Null(enumerable);

        o =
            new JObject(
                new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                new JProperty("Test2", "Test2Value"),
                new JProperty("Test3", null)
            );

        enumerable = o.AsJEnumerable();
        Xunit.Assert.NotNull(enumerable);
        Xunit.Assert.Equal(o, enumerable);

        var d = enumerable["Test1"].Value<DateTime>();

        Xunit.Assert.Equal(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), d);
    }

    [Fact]
    public void CovariantIJEnumerable()
    {
        IEnumerable<JObject> o = new[]
        {
            JObject.FromObject(new { First = 1, Second = 2 }),
            JObject.FromObject(new { First = 1, Second = 2 })
        };

        IJEnumerable<JToken> values = o.Properties();
        Xunit.Assert.Equal(4, values.Count());
    }

    [Fact]
    public void LinqCast()
    {
        JToken olist = JArray.Parse("[12,55]");

        var list1 = olist.AsEnumerable().Values<int>().ToList();

        Xunit.Assert.Equal(12, list1[0]);
        Xunit.Assert.Equal(55, list1[1]);
    }

    [Fact]
    public void ChildrenExtension()
    {
        var json = @"[
                        {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Announcing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        },
                        {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Announcing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        }
                      ]";

        var o = JArray.Parse(json);

        Xunit.Assert.Equal(4, o.Children()["item"].Children()["title"].Count());
        Xunit.Assert.Equal(new[]
            {
                "Json.NET 1.3 + New license + Now on CodePlex",
                "LINQ to JSON beta",
                "Json.NET 1.3 + New license + Now on CodePlex",
                "LINQ to JSON beta"
            },
            o.Children()["item"].Children()["title"].Values<string>().ToArray());
    }

    [Fact]
    public void UriGuidTimeSpanTestClassEmptyTest()
    {
        var c1 = new UriGuidTimeSpanTestClass();
        var o = JObject.FromObject(c1);

        StringAssert.AreEqual(@"{
  ""Guid"": ""00000000-0000-0000-0000-000000000000"",
  ""NullableGuid"": null,
  ""TimeSpan"": ""00:00:00"",
  ""NullableTimeSpan"": null,
  ""Uri"": null
}", o.ToString());

        var c2 = o.ToObject<UriGuidTimeSpanTestClass>();
        Xunit.Assert.Equal(c1.Guid, c2.Guid);
        Xunit.Assert.Equal(c1.NullableGuid, c2.NullableGuid);
        Xunit.Assert.Equal(c1.TimeSpan, c2.TimeSpan);
        Xunit.Assert.Equal(c1.NullableTimeSpan, c2.NullableTimeSpan);
        Xunit.Assert.Equal(c1.Uri, c2.Uri);
    }

    [Fact]
    public void UriGuidTimeSpanTestClassValuesTest()
    {
        var c1 = new UriGuidTimeSpanTestClass
        {
            Guid = new Guid("1924129C-F7E0-40F3-9607-9939C531395A"),
            NullableGuid = new Guid("9E9F3ADF-E017-4F72-91E0-617EBE85967D"),
            TimeSpan = TimeSpan.FromDays(1),
            NullableTimeSpan = TimeSpan.FromHours(1),
            Uri = new Uri("http://testuri.com")
        };
        var o = JObject.FromObject(c1);

        StringAssert.AreEqual(@"{
  ""Guid"": ""1924129c-f7e0-40f3-9607-9939c531395a"",
  ""NullableGuid"": ""9e9f3adf-e017-4f72-91e0-617ebe85967d"",
  ""TimeSpan"": ""1.00:00:00"",
  ""NullableTimeSpan"": ""01:00:00"",
  ""Uri"": ""http://testuri.com""
}", o.ToString());

        var c2 = o.ToObject<UriGuidTimeSpanTestClass>();
        Xunit.Assert.Equal(c1.Guid, c2.Guid);
        Xunit.Assert.Equal(c1.NullableGuid, c2.NullableGuid);
        Xunit.Assert.Equal(c1.TimeSpan, c2.TimeSpan);
        Xunit.Assert.Equal(c1.NullableTimeSpan, c2.NullableTimeSpan);
        Xunit.Assert.Equal(c1.Uri, c2.Uri);

        var j = JsonConvert.SerializeObject(c1, Formatting.Indented);

        StringAssert.AreEqual(j, o.ToString());
    }

    [Fact]
    public void ParseWithPrecendingComments()
    {
        var json = @"/* blah */ {'hi':'hi!'}";
        var o = JObject.Parse(json);
        Xunit.Assert.Equal("hi!", (string)o["hi"]);

        json = @"/* blah */ ['hi!']";
        var a = JArray.Parse(json);
        Xunit.Assert.Equal("hi!", (string)a[0]);
    }

    [Fact]
    public void ExceptionFromOverloadWithJValue()
    {
        dynamic name = new JValue("Matthew Doig");

        IDictionary<string, string> users = new Dictionary<string, string>();

        // unfortunatly there doesn't appear to be a way around this
        ExceptionAssert.Throws<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(() =>
        {
            users.Add("name2", name);

            Xunit.Assert.Equal(users["name2"], "Matthew Doig");
        }, "The best overloaded method match for 'System.Collections.Generic.IDictionary<string,string>.Add(string, string)' has some invalid arguments");
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FooBar
    {
        [EnumMember(Value = "SOME_VALUE")]
        SomeValue,

        [EnumMember(Value = "SOME_OTHER_VALUE")]
        SomeOtherValue
    }

    public class MyObject
    {
        public FooBar FooBar { get; set; }
    }

    [Fact]
    public void ToObject_Enum_Converter()
    {
        var o = JObject.Parse("{'FooBar':'SOME_OTHER_VALUE'}");

        var e = o["FooBar"].ToObject<FooBar>();
        Xunit.Assert.Equal(FooBar.SomeOtherValue, e);
    }

    public enum FooBarNoEnum
    {
        [EnumMember(Value = "SOME_VALUE")]
        SomeValue,

        [EnumMember(Value = "SOME_OTHER_VALUE")]
        SomeOtherValue
    }

    public class MyObjectNoEnum
    {
        public FooBarNoEnum FooBarNoEnum { get; set; }
    }

    [Fact]
    public void ToObject_Enum_NoConverter()
    {
        var o = JObject.Parse("{'FooBarNoEnum':'SOME_OTHER_VALUE'}");

        var e = o["FooBarNoEnum"].ToObject<FooBarNoEnum>();
        Xunit.Assert.Equal(FooBarNoEnum.SomeOtherValue, e);
    }

    [Fact]
    public void SerializeWithNoRedundentIdPropertiesTest()
    {
        var dic1 = new Dictionary<string, object>();
        var dic2 = new Dictionary<string, object>();
        var dic3 = new Dictionary<string, object>();
        var list1 = new List<object>();
        var list2 = new List<object>();

        dic1.Add("list1", list1);
        dic1.Add("list2", list2);
        dic1.Add("dic1", dic1);
        dic1.Add("dic2", dic2);
        dic1.Add("dic3", dic3);
        dic1.Add("integer", 12345);

        list1.Add("A string!");
        list1.Add(dic1);
        list1.Add(new List<object>());

        dic3.Add("dic3", dic3);

        var json = SerializeWithNoRedundentIdProperties(dic1);

        StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""list1"": [
    ""A string!"",
    {
      ""$ref"": ""1""
    },
    []
  ],
  ""list2"": [],
  ""dic1"": {
    ""$ref"": ""1""
  },
  ""dic2"": {},
  ""dic3"": {
    ""$id"": ""3"",
    ""dic3"": {
      ""$ref"": ""3""
    }
  },
  ""integer"": 12345
}", json);
    }

    static string SerializeWithNoRedundentIdProperties(object o)
    {
        var writer = new JTokenWriter();
        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });
        serializer.Serialize(writer, o);

        var t = writer.Token;

        if (t is JContainer)
        {
            var c = t as JContainer;

            // find all the $id properties in the JSON
            IList<JProperty> ids = c.Descendants().OfType<JProperty>().Where(d => d.Name == "$id").ToList();

            if (ids.Count > 0)
            {
                // find all the $ref properties in the JSON
                IList<JProperty> refs = c.Descendants().OfType<JProperty>().Where(d => d.Name == "$ref").ToList();

                foreach (var idProperty in ids)
                {
                    // check whether the $id property is used by a $ref
                    var idUsed = refs.Any(r => idProperty.Value.ToString() == r.Value.ToString());

                    if (!idUsed)
                    {
                        // remove unused $id
                        idProperty.Remove();
                    }
                }
            }
        }

        var json = t.ToString();
        return json;
    }

    [Fact]
    public void HashCodeTests()
    {
        var o1 = new JObject
        {
            ["prop"] = 1
        };
        var o2 = new JObject
        {
            ["prop"] = 1
        };

        Xunit.Assert.False(ReferenceEquals(o1, o2));
        Xunit.Assert.False(Equals(o1, o2));
        Xunit.Assert.False(o1.GetHashCode() == o2.GetHashCode());
        Xunit.Assert.True(o1.GetDeepHashCode() == o2.GetDeepHashCode());
        Xunit.Assert.True(JToken.DeepEquals(o1, o2));

        var a1 = new JArray
        {
            1
        };
        var a2 = new JArray
        {
            1
        };

        Xunit.Assert.False(ReferenceEquals(a1, a2));
        Xunit.Assert.False(Equals(a1, a2));
        Xunit.Assert.False(a1.GetHashCode() == a2.GetHashCode());
        Xunit.Assert.True(a1.GetDeepHashCode() == a2.GetDeepHashCode());
        Xunit.Assert.True(JToken.DeepEquals(a1, a2));
    }
}