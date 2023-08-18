// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable NotAccessedField.Local
namespace TestObjects;

public class PrivateMembersClass
{
    public PrivateMembersClass(string privateString, string internalString)
    {
        _privateString = privateString;
        _internalString = internalString;
    }

    public PrivateMembersClass() =>
        i = default(int);

    string _privateString;
    readonly int i;
    internal string _internalString;

    public int UseValue() =>
        i;
}