// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeDictionary : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region DeserializeDictionary

        var json = @"{
              'href': '/account/login.aspx',
              'target': '_blank'
            }";

        var htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Console.WriteLine(htmlAttributes["href"]);
        // /account/login.aspx

        Console.WriteLine(htmlAttributes["target"]);
        // _blank

        #endregion

        Assert.Equal("_blank", htmlAttributes["target"]);
    }
}