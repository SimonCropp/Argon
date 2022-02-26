// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1552 : TestFixtureBase
{
    [Fact]
    public void Test_Error()
    {
        var c = new RefAndRefReadonlyTestClass(123);
        c.SetRefField(456);

        var ex = XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(c),
            "Error getting value from 'RefField' on 'RefAndRefReadonlyTestClass'.");

        Assert.Equal("Could not create getter for Int32& RefField. ByRef return values are not supported.", ex.InnerException.Message);
    }

    [Fact]
    public void Test_Ignore()
    {
        var c = new RefAndRefReadonlyIgnoredTestClass(123);
        c.SetRefField(456);

        var json = JsonConvert.SerializeObject(c);

        Assert.Equal("{}", json);
    }
}

public class RefAndRefReadonlyTestClass
{
    int refField;
    readonly int refReadonlyField;

    public RefAndRefReadonlyTestClass(int refReadonlyField)
    {
        this.refReadonlyField = refReadonlyField;
    }

    public ref int RefField => ref refField;

    public ref readonly int RefReadonlyField => ref refReadonlyField;

    public void SetRefField(int value)
    {
        refField = value;
    }
}

public class RefAndRefReadonlyIgnoredTestClass
{
    int _refField;
    readonly int _refReadonlyField;

    public RefAndRefReadonlyIgnoredTestClass(int refReadonlyField)
    {
        _refReadonlyField = refReadonlyField;
    }

    [JsonIgnore]
    public ref int RefField => ref _refField;

    [JsonIgnore]
    public ref readonly int RefReadonlyField => ref _refReadonlyField;

    public void SetRefField(int value)
    {
        _refField = value;
    }
}