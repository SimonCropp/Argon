// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeAnonymousType : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region DeserializeAnonymousType
        var definition = new { Name = "" };

        var json1 = @"{'Name':'James'}";
        var customer1 = JsonConvert.DeserializeAnonymousType(json1, definition);

        Console.WriteLine(customer1.Name);
        // James

        var json2 = @"{'Name':'Mike'}";
        var customer2 = JsonConvert.DeserializeAnonymousType(json2, definition);

        Console.WriteLine(customer2.Name);
        // Mike
        #endregion

        Assert.Equal("Mike", customer2.Name);
    }
}