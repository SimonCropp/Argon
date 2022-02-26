// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class ToStringJsonConverter : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ToStringJsonConverter
        var o = JObject.Parse(@"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00'}");

        Console.WriteLine(o.ToString(Formatting.None, new JavaScriptDateTimeConverter()));
        // {"string1":"value","integer2":99,"datetime3":new Date(959032800000)}
        #endregion

        Assert.NotNull(o.ToString(Formatting.None, new JavaScriptDateTimeConverter()));
    }
}