// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1752 : TestFixtureBase
{
    [Fact]
    public void Test_EmptyString()
    {
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var s1 = JsonConvert.SerializeObject(new TestObject { Obj = new byte[] { } }, settings);

        var t1 = JsonConvert.DeserializeObject<TestObject>(s1, settings);
        Assert.NotNull(t1.Obj);

        var data = (byte[])t1.Obj;
        Assert.Equal(0, data.Length);
    }

    [Fact]
    public void Test_Null()
    {
        var t1 = JsonConvert.DeserializeObject<TestObject1>("{'Obj':null}");
        Assert.Null(t1.Obj);
    }

    class TestObject
    {
        public object Obj { get; set; }
    }

    class TestObject1
    {
        public byte[] Obj { get; set; }
    }
}