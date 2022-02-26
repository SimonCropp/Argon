// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializeWithJsonConverters : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region SerializeWithJsonConvertersUsage
        var stringComparisons = new List<StringComparison>
        {
            StringComparison.CurrentCulture,
            StringComparison.Ordinal
        };

        var jsonWithoutConverter = JsonConvert.SerializeObject(stringComparisons);

        Console.WriteLine(jsonWithoutConverter);
        // [0,4]

        var jsonWithConverter = JsonConvert.SerializeObject(stringComparisons, new StringEnumConverter());

        Console.WriteLine(jsonWithConverter);
        // ["CurrentCulture","Ordinal"]

        var newStringComparsions = JsonConvert.DeserializeObject<List<StringComparison>>(
            jsonWithConverter,
            new StringEnumConverter());

        Console.WriteLine(string.Join(", ", newStringComparsions.Select(c => c.ToString()).ToArray()));
        // CurrentCulture, Ordinal
        #endregion

        Assert.Equal("CurrentCulture, Ordinal", string.Join(", ", newStringComparsions.Select(c => c.ToString()).ToArray()));
    }
}