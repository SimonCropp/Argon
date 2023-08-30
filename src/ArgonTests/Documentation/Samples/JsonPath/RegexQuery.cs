// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class RegexQuery : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region RegexQuery

        var array = JArray.Parse("""
                                 [
                                   {
                                     'PackageId': 'Argon',
                                     'Version': '11.0.1',
                                     'ReleaseDate': '2018-02-17T00:00:00'
                                   },
                                   {
                                     'PackageId': 'NUnit',
                                     'Version': '3.9.0',
                                     'ReleaseDate': '2017-11-10T00:00:00'
                                   }
                                 ]
                                 """);

        // Find packages
        var packages = array.SelectTokens("$.[?(@.PackageId =~ /^Argon/)]").ToList();

        foreach (var item in packages)
        {
            Console.WriteLine((string) item["PackageId"]);
        }

        // Argon

        #endregion

        Assert.Equal(1, packages.Count);
        Assert.Equal("Argon", (string) packages[0]["PackageId"]);
    }
}