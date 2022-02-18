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

#if NET5_0_OR_GREATER
using Xunit;

namespace Argon.Tests.Issues;

public class Issue1757 : TestFixtureBase
{
    [Fact]
    public void Test_Serialize()
    {
        JsonConvert.SerializeObject(new TestObject());
    }

    [Fact]
    public void Test_SerializeEncoding()
    {
        JsonConvert.SerializeObject(Encoding.UTF8);
    }

    [Fact]
    public void Test_Deserialize()
    {
        JsonConvert.DeserializeObject<TestObject>(@"{'Room':{},'RefLike':{}}");
    }

    public class TestObject
    {
        public Span<int> this[int i]
        {
            get => default(Span<int>);
            set => DoNothing(value);
        }
        public static Span<int> Space
        {
            get => default(Span<int>);
            set => DoNothing(value);
        }
        public Span<int> Room
        {
            get => default(Span<int>);
            set => DoNothing(value);
        }
        public MyByRefLikeType RefLike
        {
            get => default(MyByRefLikeType);
            set { }
        }
        static void DoNothing(Span<int> param)
        {
            throw new InvalidOperationException("Should never be called.");
        }
        public string PrintMySpan(string str, Span<int> mySpan = default)
        {
            return str;
        }

        public Span<int> GetSpan(int[] array)
        {
            return array.AsSpan();
        }
    }

    public ref struct MyByRefLikeType
    {
        public MyByRefLikeType(int i) { }
        public static int Index;
    }
}
#endif