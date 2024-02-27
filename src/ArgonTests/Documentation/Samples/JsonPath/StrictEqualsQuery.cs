// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class StrictEqualsQuery : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region StrictEqualsQueryUsage

        var items = JArray.Parse("""
                                 [
                                   {
                                     'Name': 'Valid JSON',
                                     'Valid': true
                                   },
                                   {
                                     'Name': 'Invalid JSON',
                                     'Valid': 'true'
                                   }
                                 ]
                                 """);

        // Use === operator. Compared types must be the same to be valid
        var strictResults = items.SelectTokens("$.[?(@.Valid === true)]").ToList();

        foreach (var item in strictResults)
        {
            Console.WriteLine((string) item["Name"]);
        }

        // Valid JSON

        #endregion

        Assert.Single(strictResults);
    }
}