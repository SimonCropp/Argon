// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if !NET6_0_OR_GREATER

public class QueryJsonDynamic : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region QueryJsonDynamic
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

        dynamic blogPosts = JArray.Parse(json);

        var blogPost = blogPosts[0];

        string title = blogPost.Title;

        Console.WriteLine(title);
        // Json.NET is awesome!

        string author = blogPost.Author.Name;

        Console.WriteLine(author);
        // James Newton-King

        DateTime postDate = blogPost.Date;

        Console.WriteLine(postDate);
        // 23/01/2013 7:30:00 p.m.
        #endregion

        Assert.Equal("Json.NET is awesome!", title);
    }
}

#endif