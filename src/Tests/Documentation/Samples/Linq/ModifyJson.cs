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

namespace Argon.Tests.Documentation.Samples.Linq;

public class ModifyJson : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ModifyJson

        var json = @"{
              'channel': {
                'title': 'Star Wars',
                'link': 'http://www.starwars.com',
                'description': 'Star Wars blog.',
                'obsolete': 'Obsolete value',
                'item': []
              }
            }";

        var rss = JObject.Parse(json);

        var channel = (JObject) rss["channel"];

        channel["title"] = ((string) channel["title"]).ToUpper();
        channel["description"] = ((string) channel["description"]).ToUpper();

        channel.Property("obsolete").Remove();

        channel.Property("description").AddAfterSelf(new JProperty("new", "New value"));

        var item = (JArray) channel["item"];
        item.Add("Item 1");
        item.Add("Item 2");

        Console.WriteLine(rss.ToString());
        // {
        //   "channel": {
        //     "title": "STAR WARS",
        //     "link": "http://www.starwars.com",
        //     "description": "STAR WARS BLOG.",
        //     "new": "New value",
        //     "item": [
        //       "Item 1",
        //       "Item 2"
        //     ]
        //   }
        // }

        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""channel"": {
    ""title"": ""STAR WARS"",
    ""link"": ""http://www.starwars.com"",
    ""description"": ""STAR WARS BLOG."",
    ""new"": ""New value"",
    ""item"": [
      ""Item 1"",
      ""Item 2""
    ]
  }
}", rss.ToString());
    }
}