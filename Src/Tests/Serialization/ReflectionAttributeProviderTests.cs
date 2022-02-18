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

using System.ComponentModel;
using Xunit;

namespace Argon.Tests.Serialization;

public class ReflectionAttributeProviderTests : TestFixtureBase
{
    public class ReflectionTestObject
    {
        [DefaultValue("1")]
        [JsonProperty]
        public int TestProperty { get; set; }

        [DefaultValue("1")]
        [JsonProperty]
        public int TestField;

        public ReflectionTestObject(
            [DefaultValue("1")] [JsonProperty] int testParameter)
        {
            TestProperty = testParameter;
            TestField = testParameter;
        }
    }

    [Fact]
    public void GetAttributes_Property()
    {
        var property = typeof(ReflectionTestObject).GetProperty("TestProperty");

        var provider = new ReflectionAttributeProvider(property);

        var attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Count);

        attributes = provider.GetAttributes(false);
        Assert.Equal(2, attributes.Count);
    }

    [Fact]
    public void GetAttributes_Field()
    {
        var field = typeof(ReflectionTestObject).GetField("TestField");

        var provider = new ReflectionAttributeProvider(field);

        var attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Count);

        attributes = provider.GetAttributes(false);
        Assert.Equal(2, attributes.Count);
    }

    [Fact]
    public void GetAttributes_Parameter()
    {
        var parameters = typeof(ReflectionTestObject).GetConstructor(new[] { typeof(int) }).GetParameters();

        var parameter = parameters[0];

        var provider = new ReflectionAttributeProvider(parameter);

        var attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Count);

        attributes = provider.GetAttributes(false);
        Assert.Equal(2, attributes.Count);
    }
}