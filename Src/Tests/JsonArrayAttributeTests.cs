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

namespace Argon.Tests;

public class JsonArrayAttributeTests : TestFixtureBase
{
    [Fact]
    public void IsReferenceTest()
    {
        var attribute = new JsonPropertyAttribute();
        Xunit.Assert.Equal(null, attribute._isReference);
        XUnitAssert.False(attribute.IsReference);

        attribute.IsReference = false;
        XUnitAssert.False(attribute._isReference);
        XUnitAssert.False(attribute.IsReference);

        attribute.IsReference = true;
        XUnitAssert.True(attribute._isReference);
        XUnitAssert.True(attribute.IsReference);
    }

    [Fact]
    public void NullValueHandlingTest()
    {
        var attribute = new JsonPropertyAttribute();
        Xunit.Assert.Equal(null, attribute._nullValueHandling);
        Xunit.Assert.Equal(NullValueHandling.Include, attribute.NullValueHandling);

        attribute.NullValueHandling = NullValueHandling.Ignore;
        Xunit.Assert.Equal(NullValueHandling.Ignore, attribute._nullValueHandling);
        Xunit.Assert.Equal(NullValueHandling.Ignore, attribute.NullValueHandling);
    }

    [Fact]
    public void DefaultValueHandlingTest()
    {
        var attribute = new JsonPropertyAttribute();
        Xunit.Assert.Equal(null, attribute._defaultValueHandling);
        Xunit.Assert.Equal(DefaultValueHandling.Include, attribute.DefaultValueHandling);

        attribute.DefaultValueHandling = DefaultValueHandling.Ignore;
        Xunit.Assert.Equal(DefaultValueHandling.Ignore, attribute._defaultValueHandling);
        Xunit.Assert.Equal(DefaultValueHandling.Ignore, attribute.DefaultValueHandling);
    }
}