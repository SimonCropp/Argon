﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Xunit;

namespace Argon.Tests.Documentation.Samples.Linq;

public class Clone : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Usage
        var o1 = new JObject
        {
            { "String", "A string!" },
            { "Items", new JArray(1, 2) }
        };

        Console.WriteLine(o1.ToString());
        // {
        //   "String": "A string!",
        //   "Items": [
        //     1,
        //     2
        //   ]
        // }

        var o2 = (JObject)o1.DeepClone();

        Console.WriteLine(o2.ToString());
        // {
        //   "String": "A string!",
        //   "Items": [
        //     1,
        //     2
        //   ]
        // }

        Console.WriteLine(JToken.DeepEquals(o1, o2));
        // true

        Console.WriteLine(ReferenceEquals(o1, o2));
        // false
        #endregion

        Assert.True(JToken.DeepEquals(o1, o2));
        Assert.False(ReferenceEquals(o1, o2));
    }
}