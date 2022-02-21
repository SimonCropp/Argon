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

public class CreateJsonAnonymousObject : TestFixtureBase
{
    #region CreateJsonAnonymousObjectTypes
    public class Post
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public IList<string> Categories { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region CreateJsonAnonymousObjectUsage
        var posts = new List<Post>
        {
            new()
            {
                Title = "Episode VII",
                Description = "Episode VII production",
                Categories = new List<string>
                {
                    "episode-vii",
                    "movie"
                },
                Link = "episode-vii-production.aspx"
            }
        };

        var o = JObject.FromObject(new
        {
            channel = new
            {
                title = "Star Wars",
                link = "http://www.starwars.com",
                description = "Star Wars blog.",
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

        Console.WriteLine(o.ToString());
        // {
        //   "channel": {
        //     "title": "Star Wars",
        //     "link": "http://www.starwars.com",
        //     "description": "Star Wars blog.",
        //     "item": [
        //       {
        //         "title": "Episode VII",
        //         "description": "Episode VII production",
        //         "link": "episode-vii-production.aspx",
        //         "category": [
        //           "episode-vii",
        //           "movie"
        //         ]
        //       }
        //     ]
        //   }
        // }
        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""channel"": {
    ""title"": ""Star Wars"",
    ""link"": ""http://www.starwars.com"",
    ""description"": ""Star Wars blog."",
    ""item"": [
      {
        ""title"": ""Episode VII"",
        ""description"": ""Episode VII production"",
        ""link"": ""episode-vii-production.aspx"",
        ""category"": [
          ""episode-vii"",
          ""movie""
        ]
      }
    ]
  }
}", o.ToString());
    }
}