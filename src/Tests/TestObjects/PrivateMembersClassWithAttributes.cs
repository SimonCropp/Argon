// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PrivateMembersClassWithAttributes
{
    public PrivateMembersClassWithAttributes(string privateString, string internalString, string readonlyString)
    {
        _privateString = privateString;
        _readonlyString = readonlyString;
        _internalString = internalString;
    }

    public PrivateMembersClassWithAttributes() =>
        _readonlyString = "default!";

    [JsonProperty]
    string _privateString;

    [JsonProperty]
    readonly string _readonlyString;

    [JsonProperty]
    internal string _internalString;

    public string UseValue() =>
        _readonlyString;
}