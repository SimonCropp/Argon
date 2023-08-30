// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ErrorWhenNoMatchQuery : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ErrorWhenNoMatchQuery

        var items = JArray.Parse(
            """
            [
              {
                'Name': 'John Doe',
              },
              {
                'Name': 'Jane Doe',
              }
            ]
            """);

        // A true value for errorWhenNoMatch will result in an error if the queried value is missing
        string result;
        try
        {
            result = (string) items.SelectToken("$.[3]['Name']", errorWhenNoMatch: true);
        }
        catch (JsonException)
        {
            result = "Unable to find result in JSON.";
        }

        #endregion

        Assert.Equal("Unable to find result in JSON.", result);
    }
}