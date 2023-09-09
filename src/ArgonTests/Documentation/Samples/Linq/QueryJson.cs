// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class QueryJson : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region QueryJson

        var json = """
            {
              'channel': {
                'title': 'James Newton-King',
                'link': 'http://james.newtonking.com',
                'description': 'James Newton-King\'s blog.',
                'item': [
                  {
                    'title': 'Json.NET 1.3 + New license + Now on CodePlex',
                    'description': 'Announcing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'category': [
                      'Json.NET',
                      'CodePlex'
                    ]
                  },
                  {
                    'title': 'LINQ to JSON beta',
                    'description': 'Announcing LINQ to JSON',
                    'link': 'http://james.newtonking.com/projects/json-net.aspx',
                    'category': [
                      'Json.NET',
                      'LINQ'
                    ]
                  }
                ]
              }
            }
            """;

        var rss = JObject.Parse(json);

        var rssTitle = (string) rss["channel"]["title"];

        Console.WriteLine(rssTitle);
        // James Newton-King

        var itemTitle = (string) rss["channel"]["item"][0]["title"];

        Console.WriteLine(itemTitle);
        // Json.NET 1.3 + New license + Now on CodePlex

        var categories = (JArray) rss["channel"]["item"][0]["category"];

        Console.WriteLine(categories);
        // [
        //   "Json.NET",
        //   "CodePlex"
        // ]

        var categoriesText = categories.Select(c => (string) c).ToArray();

        Console.WriteLine(string.Join(", ", categoriesText));
        // Json.NET, CodePlex

        #endregion

        Assert.Equal("Json.NET, CodePlex", string.Join(", ", categoriesText));
    }
}