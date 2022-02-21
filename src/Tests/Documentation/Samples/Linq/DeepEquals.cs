#region License
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

public class DeepEquals : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region DeepEquals
        var s1 = new JValue("A string");
        var s2 = new JValue("A string");
        var s3 = new JValue("A STRING");

        Console.WriteLine(JToken.DeepEquals(s1, s2));
        // true

        Console.WriteLine(JToken.DeepEquals(s2, s3));
        // false

        var o1 = new JObject
        {
            { "Integer", 12345 },
            { "String", "A string" },
            { "Items", new JArray(1, 2) }
        };

        var o2 = new JObject
        {
            { "Integer", 12345 },
            { "String", "A string" },
            { "Items", new JArray(1, 2) }
        };

        Console.WriteLine(JToken.DeepEquals(o1, o2));
        // true

        Console.WriteLine(JToken.DeepEquals(s1, o1["String"]));
        // true
        #endregion

        Assert.True(JToken.DeepEquals(o1, o2));
        Assert.True(JToken.DeepEquals(s1, o1["String"]));
    }
}