// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeConstructorHandling : TestFixtureBase
{
    #region DeserializeConstructorHandlingTypes
    public class Website
    {
        public string Url { get; set; }

        Website()
        {
        }

        public Website(Website website)
        {
            Url = website.Url;
        }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeConstructorHandlingUsage
        var json = @"{'Url':'http://www.google.com'}";

        try
        {
            JsonConvert.DeserializeObject<Website>(json);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
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