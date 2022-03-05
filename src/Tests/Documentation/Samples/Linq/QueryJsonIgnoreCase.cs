// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class QueryJsonIgnoreCase : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Usage

        var json = @"{
              'name': 'James Newton-King',
              'blog': 'http://james.newtonking.com'
            }";

        var profile = JObject.Parse(json);

        var name = (string) profile.GetValue("Name", StringComparison.OrdinalIgnoreCase);
        Console.WriteLine(name);

        #endregion

        Assert.Equal("James Newton-King", name);
    }
}