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

using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Linq;

public class JConstructorTests : TestFixtureBase
{
    [Fact]
    public void Load()
    {
        JsonReader reader = new JsonTextReader(new StringReader("new Date(123)"));
        reader.Read();

        var constructor = JConstructor.Load(reader);
        Assert.AreEqual("Date", constructor.Name);
        Assert.IsTrue(JToken.DeepEquals(new JValue(123), constructor.Values().ElementAt(0)));
    }

    [Fact]
    public void CreateWithMultiValue()
    {
        var constructor = new JConstructor("Test", new List<int> { 1, 2, 3 });
        Assert.AreEqual("Test", constructor.Name);
        Assert.AreEqual(3, constructor.Children().Count());
        Assert.AreEqual(1, (int)constructor.Children().ElementAt(0));
        Assert.AreEqual(2, (int)constructor.Children().ElementAt(1));
        Assert.AreEqual(3, (int)constructor.Children().ElementAt(2));
    }

    [Fact]
    public void Iterate()
    {
        var c = new JConstructor("MrConstructor", 1, 2, 3, 4, 5);

        var i = 1;
        foreach (var token in c)
        {
            Assert.AreEqual(i, (int)token);
            i++;
        }
    }

    [Fact]
    public void SetValueWithInvalidIndex()
    {
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            var c = new JConstructor
            {
                ["badvalue"] = new JValue(3)
            };
        }, @"Set JConstructor values with invalid key value: ""badvalue"". Argument position index expected.");
    }

    [Fact]
    public void SetValue()
    {
        object key = 0;

        var c = new JConstructor
        {
            Name = "con"
        };
        c.Add(null);
        c[key] = new JValue(3);

        Assert.AreEqual(3, (int)c[key]);
    }
}