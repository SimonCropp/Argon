// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Web;

namespace Argon.Tests.Documentation.Samples.Linq;

public class SerializeWithLinq : TestFixtureBase
{
    #region SerializeWithLinqTypes

    public class BlogPost
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string AuthorTwitter { get; set; }
        public string Body { get; set; }
        public DateTime PostedDate { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region SerializeWithLinqUsage

        var blogPosts = new List<BlogPost>
        {
            new()
            {
                Title = "Json.NET is awesome!",
                AuthorName = "James Newton-King",
                AuthorTwitter = "JamesNK",
                PostedDate = new(2013, 1, 23, 19, 30, 0),
                Body = @"<h3>Title!</h3><p>Content!</p>"
            }
        };

        var blogPostsArray = new JArray(
            blogPosts.Select(p => new JObject
            {
                {"Title", p.Title},
                {
                    "Author", new JObject
                    {
                        {"Name", p.AuthorName},
                        {"Twitter", p.AuthorTwitter}
                    }
                },
                {"Date", p.PostedDate},
                {"BodyHtml", HttpUtility.HtmlEncode(p.Body)}
            })
        );

        Console.WriteLine(blogPostsArray.ToString());
        // [
        //   {
        //     "Title": "Json.NET is awesome!",
        //     "Author": {
        //       "Name": "James Newton-King",
        //       "Twitter": "JamesNK"
        //     },
        //     "Date": "2013-01-23T19:30:00",
        //     "BodyHtml": "&lt;h3&gt;Title!&lt;/h3&gt;&lt;p&gt;Content!&lt;/p&gt;"
        //   }
        // ]

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            [
              {
                "Title": "Json.NET is awesome!",
                "Author": {
                  "Name": "James Newton-King",
                  "Twitter": "JamesNK"
                },
                "Date": "2013-01-23T19:30:00",
                "BodyHtml": "&lt;h3&gt;Title!&lt;/h3&gt;&lt;p&gt;Content!&lt;/p&gt;"
              }
            ]
            """, blogPostsArray.ToString());
    }
}