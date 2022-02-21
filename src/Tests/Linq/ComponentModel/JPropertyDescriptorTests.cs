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

namespace Argon.Tests.Linq.ComponentModel;

public class JPropertyDescriptorTests : TestFixtureBase
{
    [Fact]
    public void GetValue()
    {
        var o = JObject.Parse("{prop1:'12345!',prop2:[1,'two','III']}");

        var prop1 = new JPropertyDescriptor("prop1");
        var prop2 = new JPropertyDescriptor("prop2");

        Assert.Equal("12345!", ((JValue)prop1.GetValue(o)).Value);
        Assert.Equal(o["prop2"], prop2.GetValue(o));
    }

    [Fact]
    public void GetValue_NullOwner_ReturnsNull()
    {
        var prop1 = new JPropertyDescriptor("prop1");

        Assert.Equal(null, prop1.GetValue(null));
    }

    [Fact]
    public void SetValue()
    {
        var o = JObject.Parse("{prop1:'12345!'}");

        var propertyDescriptor1 = new JPropertyDescriptor("prop1");

        propertyDescriptor1.SetValue(o, "54321!");

        Assert.Equal("54321!", (string)o["prop1"]);
    }

    [Fact]
    public void SetValue_NullOwner_NoError()
    {
        var prop1 = new JPropertyDescriptor("prop1");

        prop1.SetValue(null, "value!");
    }

    [Fact]
    public void ResetValue()
    {
        var o = JObject.Parse("{prop1:'12345!'}");

        var propertyDescriptor1 = new JPropertyDescriptor("prop1");
        propertyDescriptor1.ResetValue(o);

        Assert.Equal("12345!", (string)o["prop1"]);
    }

    [Fact]
    public void IsReadOnly()
    {
        var propertyDescriptor1 = new JPropertyDescriptor("prop1");

        XUnitAssert.False(propertyDescriptor1.IsReadOnly);
    }

    [Fact]
    public void PropertyType()
    {
        var propertyDescriptor1 = new JPropertyDescriptor("prop1");

        Assert.Equal(typeof(object), propertyDescriptor1.PropertyType);
    }
}