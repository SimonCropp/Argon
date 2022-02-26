// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class ListTestClass
{
    [JsonProperty]
    public string id { get; set; }

    [JsonProperty]
    public List<ListItem> items { get; set; }
}

[JsonObject(MemberSerialization.OptIn)]
public class ListItem
{
    [JsonProperty]
    public string id { get; set; }
}