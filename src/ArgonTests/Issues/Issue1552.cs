// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable ReplaceWithFieldKeyword
public class Issue1552 : TestFixtureBase
{
    [Fact]
    public void Test_Error()
    {
        var c = new RefAndRefReadonlyTestClass(123);
        c.SetRefField(456);

        var exception = Assert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(c));
        Assert.Equal("Error getting value from 'RefField' on 'RefAndRefReadonlyTestClass'.", exception.Message);
        Assert.Equal("Could not create getter for Int32& RefField. ByRef return values are not supported.", exception.InnerException.Message);
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

    // ReSharper disable once ConvertToPrimaryConstructor
    public RefAndRefReadonlyTestClass(int refReadonlyField) =>
        this.refReadonlyField = refReadonlyField;

    public ref int RefField => ref refField;

    public ref readonly int RefReadonlyField => ref refReadonlyField;

    public void SetRefField(int value) =>
        refField = value;
}

public class RefAndRefReadonlyIgnoredTestClass
{
    int refField;
    readonly int refReadonlyField;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RefAndRefReadonlyIgnoredTestClass(int refReadonlyField) =>
        this.refReadonlyField = refReadonlyField;

    [JsonIgnore]
    public ref int RefField => ref refField;

    [JsonIgnore]
    public ref readonly int RefReadonlyField => ref refReadonlyField;

    public void SetRefField(int value) =>
        refField = value;
}