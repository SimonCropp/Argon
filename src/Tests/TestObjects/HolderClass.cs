// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class HolderClass
{
    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public ContentBaseClass TestMember { get; set; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public Dictionary<int, IList<ContentBaseClass>> AnotherTestMember { get; set; }

    public ContentBaseClass AThirdTestMember { get; set; }
}