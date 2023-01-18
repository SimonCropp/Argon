// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ErrorHandlingEvent : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ErrorHandlingEventUsage

        var errors = new List<string>();

        var c = JsonConvert.DeserializeObject<List<DateTime>>(@"[
              '2009-09-09T00:00:00Z',
              'I am not a date and will error!',
              [
                1
              ],
              '1977-02-20T00:00:00Z',
              null,
              '2000-12-01T00:00:00Z'
            ]",
            new JsonSerializerSettings
            {
                Error = (currentObject, originalObject, location, exception, markAsHandled) =>
                {
                    errors.Add(exception.Message);
                    markAsHandled();
                },
                Converters = {new IsoDateTimeConverter()}
            });

        // 2009-09-09T00:00:00Z
        // 1977-02-20T00:00:00Z
        // 2000-12-01T00:00:00Z

        // The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
        // Unexpected token parsing date. Expected String, got StartArray.
        // Cannot convert null value to System.DateTime.

        #endregion

        Assert.Equal(3, errors.Count);
    }
}