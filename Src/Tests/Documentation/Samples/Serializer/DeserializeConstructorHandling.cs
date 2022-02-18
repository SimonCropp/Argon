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

using Xunit;

namespace Argon.Tests.Documentation.Samples.Serializer;

public class DeserializeConstructorHandling : TestFixtureBase
{
    #region Types
    public class Website
    {
        public string Url { get; set; }

        Website()
        {
        }

        public Website(Website website)
        {
            if (website == null)
            {
                throw new ArgumentNullException(nameof(website));
            }

            Url = website.Url;
        }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region Usage
        var json = @"{'Url':'http://www.google.com'}";

        try
        {
            JsonConvert.DeserializeObject<Website>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Value cannot be null.
            // Parameter name: website
        }

        var website = JsonConvert.DeserializeObject<Website>(json, new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });

        Console.WriteLine(website.Url);
        // http://www.google.com
        #endregion

        Assert.Equal("http://www.google.com", website.Url);
    }
}