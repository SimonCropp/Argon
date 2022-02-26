// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class KVPair<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }

    public KVPair(TKey k, TValue v)
    {
        Key = k;
        Value = v;
    }
}