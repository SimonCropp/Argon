// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

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