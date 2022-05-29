// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

readonly struct StructMultiKey<T1, T2> : IEquatable<StructMultiKey<T1, T2>>
{
    public readonly T1 Value1;
    public readonly T2 Value2;

    public StructMultiKey(T1 v1, T2 v2)
    {
        Value1 = v1;
        Value2 = v2;
    }

    public override int GetHashCode() =>
        (Value1?.GetHashCode() ?? 0) ^ (Value2?.GetHashCode() ?? 0);

    public override bool Equals(object? obj)
    {
        if (obj is StructMultiKey<T1, T2> key)
        {
            return Equals(key);
        }

        return false;
    }

    public bool Equals(StructMultiKey<T1, T2> other) =>
        Equals(Value1, other.Value1) && Equals(Value2, other.Value2);
}