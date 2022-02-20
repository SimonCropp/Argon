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

namespace Argon.Tests.Documentation;

public static class File
{
    public static StreamReader OpenText(string path)
    {
        return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("{}")));
    }

    public static StreamWriter CreateText(string path)
    {
        return new StreamWriter(new MemoryStream());
    }

    public static void WriteAllText(string path, string contents)
    {
    }

    public static string ReadAllText(string path)
    {
        return null;
    }
}

public class LinqToJsonTests : TestFixtureBase
{
    [Fact]
    public void LinqToJsonBasic()
    {
        #region LinqToJsonBasic
        var o = JObject.Parse(@"{
              'CPU': 'Intel',
              'Drives': [
                'DVD read/writer',
                '500 gigabyte hard drive'
              ]
            }");

        var cpu = (string)o["CPU"];
        // Intel

        var firstDrive = (string)o["Drives"][0];
        // DVD read/writer

        IList<string> allDrives = o["Drives"].Select(t => (string)t).ToList();
        // DVD read/writer
        // 500 gigabyte hard drive
        #endregion
    }

    [Fact]
    public void LinqToJsonCreateNormal()
    {
        #region LinqToJsonCreateNormal
        var array = new JArray();
        var text = new JValue("Manual text");
        var date = new JValue(new DateTime(2000, 5, 23));

        array.Add(text);
        array.Add(date);

        var json = array.ToString();
        // [
        //   "Manual text",
        //   "2000-05-23T00:00:00"
        // ]
        #endregion
    }

    public class Post
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public IList<string> Categories { get; set; }
    }

    static List<Post> GetPosts()
    {
        return new List<Post>();
    }

    [Fact]
    public void LinqToJsonCreateDeclaratively()
    {
        #region LinqToJsonCreateDeclaratively
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

        Console.WriteLine(rss.ToString());

        //{
        //  "channel": {
        //    "title": "James Newton-King",
        //    "link": "http://james.newtonking.com",
        //    "description": "James Newton-King\'s blog.",
        //    "item": [
        //      {
        //        "title": "Json.NET 1.3 + New license + Now on CodePlex",
        //        "description": "Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
        //        "link": "http://james.newtonking.com/projects/json-net.aspx",
        //        "category": [
        //          "Json.NET",
        //          "CodePlex"
        //        ]
        //      },
        //      {
        //        "title": "LINQ to JSON beta",
        //        "description": "Announcing LINQ to JSON",
        //        "link": "http://james.newtonking.com/projects/json-net.aspx",
        //        "category": [
        //          "Json.NET",
        //          "LINQ"
        //        ]
        //      }
        //    ]
        //  }
        //}
        #endregion
    }

    [Fact]
    public void LinqToJsonCreateFromObject()
    {
        var posts = GetPosts();

        #region LinqToJsonCreateFromObject
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
        #endregion
    }

    [Fact]
    public void LinqToJsonCreateParse()
    {
        #region LinqToJsonCreateParse
        var json = @"{
              CPU: 'Intel',
              Drives: [
                'DVD read/writer',
                '500 gigabyte hard drive'
              ]
            }";

        var o = JObject.Parse(json);
        #endregion
    }

    [Fact]
    public void LinqToJsonCreateParseArray()
    {
        #region LinqToJsonCreateParseArray
        var json = @"[
              'Small',
              'Medium',
              'Large'
            ]";

        var a = JArray.Parse(json);
        #endregion
    }

    [Fact]
    public void LinqToJsonReadObject()
    {
        #region LinqToJsonReadObject

        using var reader = File.OpenText(@"c:\person.json");
        var o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
        // do stuff

        #endregion
    }

    [Fact]
    public void LinqToJsonSimpleQuerying()
    {
        #region LinqToJsonSimpleQuerying
        var json = @"{
              'channel': {
                'title': 'James Newton-King',
                'link': 'http://james.newtonking.com',
                'description': 'James Newton-King\'s blog.',
                'item': [
                  {
                    'title': 'Json.NET 1.3 + New license + Now on CodePlex',
                    'description': 'Announcing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'categories': [
                      'Json.NET',
                      'CodePlex'
                    ]
                  },
                  {
                    'title': 'LINQ to JSON beta',
                    'description': 'Announcing LINQ to JSON',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'categories': [
                      'Json.NET',
                      'LINQ'
                    ]
                  }
                ]
              }
            }";

        var rss = JObject.Parse(json);

        var rssTitle = (string)rss["channel"]["title"];
        // James Newton-King

        var itemTitle = (string)rss["channel"]["item"][0]["title"];
        // Json.NET 1.3 + New license + Now on CodePlex

        var categories = (JArray)rss["channel"]["item"][0]["categories"];
        // ["Json.NET", "CodePlex"]

        IList<string> categoriesText = categories.Select(c => (string)c).ToList();
        // Json.NET
        // CodePlex
        #endregion
    }

    [Fact]
    public void LinqToJsonQuerying()
    {
        var rss = JObject.Parse(@"{
              'channel': {
                'title': 'James Newton-King',
                'link': 'http://james.newtonking.com',
                'description': 'James Newton-King\'s blog.',
                'item': [
                  {
                    'title': 'Json.NET 1.3 + New license + Now on CodePlex',
                    'description': 'Announcing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'categories': [
                      'Json.NET',
                      'CodePlex'
                    ]
                  },
                  {
                    'title': 'LINQ to JSON beta',
                    'description': 'Announcing LINQ to JSON',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'categories': [
                      'Json.NET',
                      'LINQ'
                    ]
                  }
                ]
              }
            }");

        #region LinqToJsonQuerying
        var postTitles =
            from p in rss["channel"]["item"]
            select (string)p["title"];

        foreach (var item in postTitles)
        {
            Console.WriteLine(item);
        }

        //LINQ to JSON beta
        //Json.NET 1.3 + New license + Now on CodePlex

        var categories =
            from c in rss["channel"]["item"].SelectMany(i => i["categories"]).Values<string>()
            group c by c
            into g
            orderby g.Count() descending
            select new { Category = g.Key, Count = g.Count() };

        foreach (var c in categories)
        {
            Console.WriteLine($"{c.Category} - Count: {c.Count}");
        }

        //Json.NET - Count: 2
        //LINQ - Count: 1
        //CodePlex - Count: 1
        #endregion

        Assert.Equal(2, postTitles.Count());
        Assert.Equal(3, categories.Count());
    }

    #region LinqToJsonDeserializeObject
    public class Shortie
    {
        public string Original { get; set; }
        public string Shortened { get; set; }
        public string Short { get; set; }
        public ShortieException Error { get; set; }
    }

    public class ShortieException
    {
        public int Code { get; set; }
        public string ErrorMessage { get; set; }
    }
    #endregion

    [Fact]
    public void LinqToJsonDeserializeExample()
    {
        #region LinqToJsonDeserializeExample
        var jsonText = @"{
              'short': {
                'original': 'http://www.foo.com/',
                'short': 'krehqk',
                'error': {
                  'code': 0,
                  'msg': 'No action taken'
                }
              }
            }";

        var json = JObject.Parse(jsonText);

        var shortie = new Shortie
        {
            Original = (string)json["short"]["original"],
            Short = (string)json["short"]["short"],
            Error = new ShortieException
            {
                Code = (int)json["short"]["error"]["code"],
                ErrorMessage = (string)json["short"]["error"]["msg"]
            }
        };

        Console.WriteLine(shortie.Original);
        // http://www.foo.com/

        Console.WriteLine(shortie.Error.ErrorMessage);
        // No action taken
        #endregion

        Assert.Equal("http://www.foo.com/", shortie.Original);
        Assert.Equal("No action taken", shortie.Error.ErrorMessage);
    }

    [Fact]
    public void SelectTokenSimple()
    {
        var o = JObject.Parse(@"{
              'Stores': [
                'Lambton Quay',
                'Willis Street'
              ],
              'Manufacturers': [
                {
                  'Name': 'Acme Co',
                  'Products': [
                    {
                      'Name': 'Anvil',
                      'Price': 50
                    }
                  ]
                },
                {
                  'Name': 'Contoso',
                  'Products': [
                    {
                      'Name': 'Elbow Grease',
                      'Price': 99.95
                    },
                    {
                      'Name': 'Headlight Fluid',
                      'Price': 4
                    }
                  ]
                }
              ]
            }");

        #region SelectTokenSimple
        var name = (string)o.SelectToken("Manufacturers[0].Name");
        #endregion

        Assert.Equal("Acme Co", name);
    }

    [Fact]
    public void SelectTokenComplex()
    {
        #region SelectTokenComplex
        var o = JObject.Parse(@"{
              'Stores': [
                'Lambton Quay',
                'Willis Street'
              ],
              'Manufacturers': [
                {
                  'Name': 'Acme Co',
                  'Products': [
                    {
                      'Name': 'Anvil',
                      'Price': 50
                    }
                  ]
                },
                {
                  'Name': 'Contoso',
                  'Products': [
                    {
                      'Name': 'Elbow Grease',
                      'Price': 99.95
                    },
                    {
                      'Name': 'Headlight Fluid',
                      'Price': 4
                    }
                  ]
                }
              ]
            }");

        var name = (string)o.SelectToken("Manufacturers[0].Name");
        // Acme Co

        var productPrice = (decimal)o.SelectToken("Manufacturers[0].Products[0].Price");
        // 50

        var productName = (string)o.SelectToken("Manufacturers[1].Products[0].Name");
        // Elbow Grease
        #endregion

        Assert.Equal("Acme Co", name);
        Assert.Equal(50m, productPrice);
        Assert.Equal("Elbow Grease", productName);
    }

    [Fact]
    public void SelectTokenLinq()
    {
        var o = JObject.Parse(@"{
              'Stores': [
                'Lambton Quay',
                'Willis Street'
              ],
              'Manufacturers': [
                {
                  'Name': 'Acme Co',
                  'Products': [
                    {
                      'Name': 'Anvil',
                      'Price': 50
                    }
                  ]
                },
                {
                  'Name': 'Contoso',
                  'Products': [
                    {
                      'Name': 'Elbow Grease',
                      'Price': 99.95
                    },
                    {
                      'Name': 'Headlight Fluid',
                      'Price': 4
                    }
                  ]
                }
              ]
            }");

        #region SelectTokenLinq
        IList<string> storeNames = o.SelectToken("Stores").Select(s => (string)s).ToList();
        // Lambton Quay
        // Willis Street

        IList<string> firstProductNames = o["Manufacturers"].Select(m => (string)m.SelectToken("Products[1].Name")).ToList();
        // null
        // Headlight Fluid

        var totalPrice = o["Manufacturers"].Sum(m => (decimal)m.SelectToken("Products[0].Price"));
        // 149.95
        #endregion

        Assert.Equal(2, storeNames.Count);
        Assert.Equal(2, firstProductNames.Count);
        Assert.Equal(149.95m, totalPrice);
    }
}