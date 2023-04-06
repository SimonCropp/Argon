// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1725 : TestFixtureBase
{
    [Fact]
    public void Test_In()
    {
        var p1 = new InPerson("some name");
        var json = JsonConvert.SerializeObject(p1);

        var p2 = JsonConvert.DeserializeObject<InPerson>(json);
        Assert.Equal("some name", p2.Name);
    }

    [Fact]
    public void Test_Ref()
    {
        var value = "some name";
        var p1 = new RefPerson(ref value);
        var json = JsonConvert.SerializeObject(p1);

        var p2 = JsonConvert.DeserializeObject<RefPerson>(json);
        Assert.Equal("some name", p2.Name);
    }

    [Fact]
    public void Test_InNullable()
    {
        var p1 = new InNullablePerson(1);
        var json = JsonConvert.SerializeObject(p1);

        var p2 = JsonConvert.DeserializeObject<InNullablePerson>(json);
        Assert.Equal(1, p2.Age);
    }

    [Fact]
    public void Test_RefNullable()
    {
        int? value = 1;
        var p1 = new RefNullablePerson(ref value);
        var json = JsonConvert.SerializeObject(p1);

        var p2 = JsonConvert.DeserializeObject<RefNullablePerson>(json);
        Assert.Equal(1, p2.Age);
    }

    public class InPerson
    {
        public InPerson(in string name) =>
            Name = name;

        public string Name { get; }
    }

    public class RefPerson
    {
        public RefPerson(ref string name) =>
            Name = name;

        public string Name { get; }
    }

    public class InNullablePerson
    {
        public InNullablePerson(in int? age) =>
            Age = age;

        public int? Age { get; }
    }

    public class RefNullablePerson
    {
        public RefNullablePerson(ref int? age) =>
            Age = age;

        public int? Age { get; }
    }
}