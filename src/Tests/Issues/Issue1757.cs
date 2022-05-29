// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if NET5_0_OR_GREATER

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
            get => default;
            set => DoNothing(value);
        }
        public static Span<int> Space
        {
            get => default;
            set => DoNothing(value);
        }
        public Span<int> Room
        {
            get => default;
            set => DoNothing(value);
        }
        public MyByRefLikeType RefLike
        {
            get => default;
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