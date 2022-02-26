// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.Fields)]
public class MyTuple<T1>
{
    readonly T1 m_Item1;

    public MyTuple(T1 item1)
    {
        m_Item1 = item1;
    }

    public T1 Item1 => m_Item1;
}