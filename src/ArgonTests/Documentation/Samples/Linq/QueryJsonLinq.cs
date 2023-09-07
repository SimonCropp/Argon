// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable PossibleMultipleEnumeration
namespace Argon.Tests.Documentation.Samples.Linq;

public class QueryJsonLinq : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region QueryJsonLinq

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

        var postTitles =
            from p in rss["channel"]["item"]
            select (string) p["title"];

        foreach (var item in postTitles)
        {
            Console.WriteLine(item);
        }
        //LINQ to JSON beta
        //Json.NET 1.3 + New license + Now on CodePlex

        var categories =
            from c in rss["channel"]["item"].Children()["category"].Values<string>()
            group c by c
            into g
            orderby g.Count() descending
            select new {Category = g.Key, Count = g.Count()};

        foreach (var c in categories)
        {
            Console.WriteLine($"{c.Category} - Count: {c.Count}");
        }

        //Json.NET - Count: 2
        //LINQ - Count: 1
        //CodePlex - Count: 1

        #endregion

        Assert.Equal(3, categories.Count());
    }
}