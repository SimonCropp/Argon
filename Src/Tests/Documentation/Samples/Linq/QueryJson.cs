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

namespace Argon.Tests.Documentation.Samples.Linq;

public class QueryJson : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Usage

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
            }";

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

        Xunit.Assert.Equal("Json.NET, CodePlex", string.Join(", ", categoriesText));
    }
}