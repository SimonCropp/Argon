// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Web;

public class DeserializeWithLinq : TestFixtureBase
{
    #region DeserializeWithLinqTypes

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
        #region DeserializeWithLinqUsage

        var json = """
                   [
                     {
                       'Title': 'Json.NET is awesome!',
                       'Author': {
                         'Name': 'James Newton-King',
                         'Twitter': '@JamesNK',
                         'Picture': '/jamesnk.png'
                       },
                       'Date': '2013-01-23T19:30:00',
                       'BodyHtml': '&lt;h3&gt;Title!&lt;/h3&gt;\r\n&lt;p&gt;Content!&lt;/p&gt;'
                     }
                   ]
                   """;

        var blogPostArray = JArray.Parse(json);

        var blogPosts = blogPostArray.Select(p => new BlogPost
        {
            Title = (string) p["Title"],
            AuthorName = (string) p["Author"]["Name"],
            AuthorTwitter = (string) p["Author"]["Twitter"],
            PostedDate = (DateTime) p["Date"],
            Body = HttpUtility.HtmlDecode((string) p["BodyHtml"])
        }).ToList();

        Console.WriteLine(blogPosts[0].Body);
        // <h3>Title!</h3>
        // <p>Content!</p>

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            <h3>Title!</h3>
            <p>Content!</p>
            """,
            blogPosts[0].Body);
    }
}