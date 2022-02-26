// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class PrivateSetterBase
{
    [JsonProperty]
    public string IDontWork { get; private set; }

    protected PrivateSetterBase()
    {
    }

    internal PrivateSetterBase(string dontWork)
    {
        IDontWork = dontWork;
    }
}