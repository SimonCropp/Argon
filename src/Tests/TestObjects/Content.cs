// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Content : IEnumerable<Content>
{
    [JsonProperty]
    public List<Content> Children;

    [JsonProperty]
    public string Text;

    public IEnumerator GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    IEnumerator<Content> IEnumerable<Content>.GetEnumerator()
    {
        return Children.GetEnumerator();
    }
}