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

using System.Web;
using Xunit;

namespace Argon.Tests.Documentation.Samples.Linq;

public class DeserializeWithLinq : TestFixtureBase
{
    #region Types
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
        #region Usage
        var json = @"[
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
            ]";

        var blogPostArray = JArray.Parse(json);

        IList<BlogPost> blogPosts = blogPostArray.Select(p => new BlogPost
        {
            Title = (string)p["Title"],
            AuthorName = (string)p["Author"]["Name"],
            AuthorTwitter = (string)p["Author"]["Twitter"],
            PostedDate = (DateTime)p["Date"],
            Body = HttpUtility.HtmlDecode((string)p["BodyHtml"])
        }).ToList();

        Console.WriteLine(blogPosts[0].Body);
        // <h3>Title!</h3>
        // <p>Content!</p>
        #endregion

        XUnitAssert.AreEqualNormalized(@"<h3>Title!</h3>
<p>Content!</p>", blogPosts[0].Body);
    }
}