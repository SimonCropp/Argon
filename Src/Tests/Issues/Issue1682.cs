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

namespace Argon.Tests.Issues;

public class Issue1682 : TestFixtureBase
{
    [Fact]
    public void Test_Serialize()
    {
        var s1 = JsonConvert.SerializeObject(new ConcreteSerializable());
        Assert.Equal("{}", s1);

        var s2 = JsonConvert.SerializeObject(new ClassWithSerializableProperty());
        Assert.Equal(@"{""Serializable"":null}", s2);
    }

    [Fact]
    public void Test_Deserialize()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => { JsonConvert.DeserializeObject<BaseSerializable>("{}"); },
            "Could not create an instance of type Argon.Tests.Issues.Issue1682+BaseSerializable. Type is an interface or abstract class and cannot be instantiated. Path '', line 1, position 2.");
    }

    public class ClassWithSerializableProperty
    {
        public BaseSerializable Serializable { get; }
    }

    [Serializable]
    public class ConcreteSerializable : BaseSerializable
    {
        public ConcreteSerializable()
        {
        }

        protected ConcreteSerializable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable] // it won't blow up after removing that attribute
    public abstract class BaseSerializable : ISerializable //or that interface, or when we will remove "abstract" keyword
    {
        public BaseSerializable()
        {
        }

        //it won't fail when that constructor is missing
        protected BaseSerializable(SerializationInfo info, StreamingContext context)
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
    }
}